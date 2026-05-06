using UnityEngine;

public class CodexTabController : MonoBehaviour
{
    [Header("Tabs")]
    [SerializeField] private GameObject questsPanel;
    [SerializeField] private GameObject bestiaryPanel;
    [SerializeField] private GameObject artifactsPanel;
    [SerializeField] private GameObject friendsPanel;

    [Header("Optional")]
    [SerializeField] private QuestJournalUI questJournalUI;

    private void Start()
    {
        ShowQuests(); // domyœlnie otwórz zak³adkê questów
    }

    public void ShowQuests()
    {
        SetOnlyActive(questsPanel);

        if (questJournalUI != null)
            questJournalUI.Refresh();
    }

    public void ShowBestiary()
    {
        SetOnlyActive(bestiaryPanel);
    }

    public void ShowArtifacts()
    {
        SetOnlyActive(artifactsPanel);
    }

    public void ShowFriends()
    {
        SetOnlyActive(friendsPanel);
    }

    private void SetOnlyActive(GameObject activePanel)
    {
        if (questsPanel != null) questsPanel.SetActive(activePanel == questsPanel);
        if (bestiaryPanel != null) bestiaryPanel.SetActive(activePanel == bestiaryPanel);
        if (artifactsPanel != null) artifactsPanel.SetActive(activePanel == artifactsPanel);
        if (friendsPanel != null) friendsPanel.SetActive(activePanel == friendsPanel);
    }
}