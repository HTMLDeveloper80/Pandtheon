using UnityEngine;

public class SceneSpawnPoint : MonoBehaviour
{
    [SerializeField] private string spawnId;

    private void Start()
    {
        if (string.IsNullOrWhiteSpace(SceneTransitionState.NextSpawnId))
            return;

        if (!string.Equals(
                spawnId,
                SceneTransitionState.NextSpawnId,
                System.StringComparison.Ordinal))
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError(
                $"[SceneSpawnPoint] Nie znaleziono Playera dla punktu {spawnId}.");
            return;
        }

        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null)
            movement.StopMovement();

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.position = transform.position;
        }
        else
        {
            player.transform.position = transform.position;
        }

        Physics2D.SyncTransforms();
        SceneTransitionState.ClearNextSpawn();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (spawnId != null)
            spawnId = spawnId.Trim();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
        Gizmos.DrawLine(
            transform.position,
            transform.position + Vector3.up * 0.6f);
    }
#endif
}
