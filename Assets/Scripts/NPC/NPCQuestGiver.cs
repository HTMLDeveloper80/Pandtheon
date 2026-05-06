using UnityEngine;

public class NpcQuestGiver : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField] private QuestData questData;

    [Header("Dialog per state")]
    [SerializeField] private DialogData notStartedDialog;
    [SerializeField] private DialogData inProgressDialog;
    [SerializeField] private DialogData readyToTurnInDialog;
    [SerializeField] private DialogData completedDialog;

    [Header("UI over NPC")]
    [SerializeField] private NpcDialogBubbleUI bubbleUI;

    private int lineIndex = 0;

    public void Interact(PlayerStats playerStats, PlayerWallet playerWallet)
    {
        if (questData == null)
        {
            ShowFromDialog(notStartedDialog);
            return;
        }

        if (QuestManager.Instance == null)
        {
            ShowFromDialog(notStartedDialog);
            return;
        }

        QuestState state = QuestManager.Instance.GetState(questData);

        switch (state)
        {
            case QuestState.NotStarted:
                ShowFromDialog(notStartedDialog);
                QuestManager.Instance.AcceptQuest(questData);
                break;

            case QuestState.InProgress:
                {
                    var rt = QuestManager.Instance.GetRuntime(questData);
                    int cur = rt != null ? rt.currentAmount : 0;
                    ShowFromDialog(inProgressDialog, $"Postêp: {cur}/{questData.requiredAmount}");
                }
                break;

            case QuestState.ReadyToTurnIn:
                {
                    bool turnedIn = QuestManager.Instance.TryTurnIn(questData, playerStats, playerWallet);
                    if (turnedIn)
                        ShowFromDialog(readyToTurnInDialog);
                    else
                        Show("Nie mogê teraz oddaæ questa.");
                }
                break;

            case QuestState.Completed:
                ShowFromDialog(completedDialog);
                break;
        }
    }

    private void ShowFromDialog(DialogData data, string fallback = "")
    {
        if (data == null || data.lines == null || data.lines.Length == 0)
        {
            Show(string.IsNullOrEmpty(fallback) ? "..." : fallback);
            return;
        }

        if (lineIndex >= data.lines.Length)
            lineIndex = 0;

        string line = data.lines[lineIndex];
        lineIndex++;
        Show(line);
    }

    private void Show(string text)
    {
        if (bubbleUI != null)
            bubbleUI.ShowText(text);
    }
}