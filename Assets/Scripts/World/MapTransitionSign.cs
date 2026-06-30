using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapTransitionSign : MonoBehaviour
{
    [Header("Scene target")]
    [Tooltip("Nazwa sceny docelowej dokladnie jak w Build Settings.")]
    [SerializeField] private string targetSceneName;

    [Header("Target spawn point")]
    [Tooltip("ID punktu wejscia na scenie docelowej.")]
    [SerializeField] private string targetSpawnId;

    [Header("Approach")]
    [Tooltip("Jak daleko od srodka tabliczki zatrzyma sie gracz.")]
    [SerializeField] private float approachOffset = 1.5f;

    [Tooltip("Dopuszczalny blad przy sprawdzaniu, czy gracz dotarl.")]
    [SerializeField] private float arrivalTolerance = 0.15f;

    [Tooltip("Dodatkowy czas oczekiwania, zanim dojscie zostanie anulowane.")]
    [SerializeField] private float timeoutPadding = 2f;

    private Coroutine approachRoutine;
    private PlayerClickController lockedClickController;
    private bool sceneLoading;

    public bool CanEnter()
    {
        return !string.IsNullOrWhiteSpace(targetSceneName);
    }

    public void Interact(PlayerMovement playerMovement)
    {
        if (!CanEnter())
        {
            Debug.LogWarning(
                $"[MapTransitionSign] Brak targetSceneName na obiekcie: {name}");
            return;
        }

        if (playerMovement == null)
        {
            Debug.LogWarning(
                $"[MapTransitionSign] Brak PlayerMovement dla tabliczki: {name}");
            return;
        }

        if (approachRoutine != null)
            StopCoroutine(approachRoutine);

        approachRoutine = StartCoroutine(ApproachAndEnter(playerMovement));
    }

    private IEnumerator ApproachAndEnter(PlayerMovement playerMovement)
    {
        sceneLoading = false;
        lockedClickController =
            playerMovement.GetComponent<PlayerClickController>();

        if (lockedClickController != null)
            lockedClickController.enabled = false;

        try
        {
            float playerX = playerMovement.CurrentPosition().x;
            float signX = transform.position.x;
            float directionToSign = Mathf.Sign(signX - playerX);

            if (Mathf.Approximately(directionToSign, 0f))
                directionToSign = 1f;

            float requiredDistance =
                Mathf.Max(0f, approachOffset) +
                Mathf.Max(0.01f, arrivalTolerance);

            if (Mathf.Abs(signX - playerX) > requiredDistance)
            {
                float targetX =
                    signX - directionToSign * Mathf.Max(0f, approachOffset);

                Vector3 destination = new Vector3(
                    targetX,
                    playerMovement.CurrentPosition().y,
                    0f);

                playerMovement.MoveTo(destination);
                playerMovement.SetMarkerAt(destination);

                float distance = Mathf.Abs(targetX - playerX);
                float moveSpeed = Mathf.Max(0.01f, playerMovement.moveSpeed);
                float timeout =
                    distance / moveSpeed + Mathf.Max(0.1f, timeoutPadding);

                float elapsed = 0f;

                while (elapsed < timeout)
                {
                    float distanceToTarget = Mathf.Abs(
                        playerMovement.CurrentPosition().x - targetX);

                    if (distanceToTarget <= arrivalTolerance)
                        break;

                    if (!playerMovement.IsMoving())
                        yield break;

                    elapsed += Time.deltaTime;
                    yield return null;
                }

                float finalDistance = Mathf.Abs(
                    playerMovement.CurrentPosition().x - targetX);

                if (finalDistance > arrivalTolerance)
                    yield break;
            }

            playerMovement.StopMovement();
            EnterScene();
        }
        finally
        {
            approachRoutine = null;

            if (!sceneLoading && lockedClickController != null)
                lockedClickController.enabled = true;

            lockedClickController = null;
        }
    }

    private void EnterScene()
    {
        sceneLoading = true;
        SceneTransitionState.SetNextSpawn(targetSpawnId);
        SceneManager.LoadScene(targetSceneName);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (targetSceneName != null)
            targetSceneName = targetSceneName.Trim();

        if (targetSpawnId != null)
            targetSpawnId = targetSpawnId.Trim();

        approachOffset = Mathf.Max(0f, approachOffset);
        arrivalTolerance = Mathf.Max(0.01f, arrivalTolerance);
        timeoutPadding = Mathf.Max(0.1f, timeoutPadding);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Vector3 leftPoint =
            transform.position + Vector3.left * approachOffset;
        Vector3 rightPoint =
            transform.position + Vector3.right * approachOffset;

        Gizmos.DrawWireSphere(leftPoint, arrivalTolerance);
        Gizmos.DrawWireSphere(rightPoint, arrivalTolerance);
        Gizmos.DrawLine(leftPoint, rightPoint);
    }
#endif
}
