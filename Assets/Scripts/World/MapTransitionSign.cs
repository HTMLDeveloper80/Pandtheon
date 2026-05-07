using UnityEngine;
using UnityEngine.SceneManagement;

public class MapTransitionSign : MonoBehaviour
{
    [Header("Scene target")]
    [Tooltip("Nazwa sceny docelowej dok³adnie jak w Build Settings.")]
    [SerializeField] private string targetSceneName;

    [Header("Optional spawn point")]
    [Tooltip("Jeœli u¿ywasz systemu spawn pointów miêdzy scenami, wpisz ID punktu wejœcia.")]
    [SerializeField] private string targetSpawnId;

    // Na ten moment zawsze true (brak wymagañ)
    public bool CanEnter()
    {
        return !string.IsNullOrWhiteSpace(targetSceneName);
    }

    public void Interact()
    {
        if (!CanEnter())
        {
            Debug.LogWarning($"[MapTransitionSign] Brak targetSceneName na obiekcie: {name}");
            return;
        }

        // Tutaj póŸniej mo¿esz zapisaæ targetSpawnId np. do GameManagera / static data
        // SceneTransitionState.NextSpawnId = targetSpawnId;

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