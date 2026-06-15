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
        ApplySavedTab();
    }

    public void ShowQuests()
    {
        UIRuntimeState.SelectedCodexTab = CodexTab.Quests;
        SetOnlyActive(questsPanel);

        if (questJournalUI != null)
            questJournalUI.Refresh();
    }

    public void ShowBestiary()
    {
        UIRuntimeState.SelectedCodexTab = CodexTab.Bestiary;
        SetOnlyActive(bestiaryPanel);
    }

    public void ShowArtifacts()
    {
        UIRuntimeState.SelectedCodexTab = CodexTab.Artifacts;
        SetOnlyActive(artifactsPanel);
    }

    public void ShowFriends()
    {
        UIRuntimeState.SelectedCodexTab = CodexTab.Friends;
        SetOnlyActive(friendsPanel);
    }

    private void ApplySavedTab()
    {
        switch (UIRuntimeState.SelectedCodexTab)
        {
            case CodexTab.Bestiary:
                ShowBestiary();
                break;

            case CodexTab.Artifacts:
                ShowArtifacts();
                break;

            case CodexTab.Friends:
                ShowFriends();
                break;

            default:
                ShowQuests();
                break;
        }
    }

    private void SetOnlyActive(GameObject activePanel)
    {
        if (questsPanel != null)
            questsPanel.SetActive(activePanel == questsPanel);

        if (bestiaryPanel != null)
            bestiaryPanel.SetActive(activePanel == bestiaryPanel);

        if (artifactsPanel != null)
            artifactsPanel.SetActive(activePanel == artifactsPanel);

        if (friendsPanel != null)
            friendsPanel.SetActive(activePanel == friendsPanel);
    }
}
