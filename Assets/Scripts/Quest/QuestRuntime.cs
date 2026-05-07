using System;

[Serializable]
public class QuestRuntime
{
    public string questId;
    public QuestData data;
    public QuestState state;
    public int currentAmount;

    public QuestRuntime(QuestData questData)
    {
        data = questData;
        questId = QuestManager.GetQuestId(questData);
        state = QuestState.NotStarted;
        currentAmount = 0;
    }
}