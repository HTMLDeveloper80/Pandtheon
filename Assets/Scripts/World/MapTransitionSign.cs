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

    public bool CanEnter()
    {
        return !string.IsNullOrWhiteSpace(targetSceneName);
    }

    public void Interact()
    {
        if (!CanEnter())
        {
            Debug.LogWarning(
                $"[MapTransitionSign] Brak targetSceneName na obiekcie: {name}");
            return;
        }

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
    }
#endif
}
