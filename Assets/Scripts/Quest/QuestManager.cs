using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    private readonly Dictionary<string, QuestRuntime> quests = new Dictionary<string, QuestRuntime>();

    public event Action<QuestRuntime> OnQuestUpdated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool HasQuest(QuestData quest)
    {
        if (quest == null) return false;
        return quests.ContainsKey(quest.name);
    }

    public QuestState GetState(QuestData quest)
    {
        if (quest == null) return QuestState.NotStarted;
        if (!quests.TryGetValue(quest.name, out var runtime))
            return QuestState.NotStarted;

        return runtime.state;
    }

    public QuestRuntime GetRuntime(QuestData quest)
    {
        if (quest == null) return null;
        quests.TryGetValue(quest.name, out var runtime);
        return runtime;
    }

    public void AcceptQuest(QuestData quest)
    {
        if (quest == null) return;

        if (!quests.TryGetValue(quest.name, out var runtime))
        {
            runtime = new QuestRuntime(quest);
            quests.Add(quest.name, runtime);
        }

        runtime.state = QuestState.InProgress;
        runtime.currentAmount = 0;
        Notify(runtime);
    }

    public void AddKill(string enemyName)
    {
        if (string.IsNullOrWhiteSpace(enemyName)) return;

        foreach (var kv in quests)
        {
            var runtime = kv.Value;
            if (runtime == null || runtime.data == null) continue;
            if (runtime.state != QuestState.InProgress) continue;

            if (string.Equals(runtime.data.targetEnemyName, enemyName, StringComparison.OrdinalIgnoreCase))
            {
                runtime.currentAmount++;
                if (runtime.currentAmount >= runtime.data.requiredAmount)
                    runtime.state = QuestState.ReadyToTurnIn;

                Notify(runtime);
            }
        }
    }

    public bool TryTurnIn(QuestData quest, PlayerStats playerStats, PlayerWallet wallet)
    {
        if (quest == null || playerStats == null || wallet == null) return false;
        if (!quests.TryGetValue(quest.name, out var runtime)) return false;
        if (runtime.state != QuestState.ReadyToTurnIn) return false;

        runtime.state = QuestState.Completed;
        Notify(runtime);

        playerStats.AddXP(quest.xpReward);
        wallet.AddMoney(quest.moneyReward);

        return true;
    }

    private void Notify(QuestRuntime runtime)
    {
        OnQuestUpdated?.Invoke(runtime);
    }

    public IEnumerable<QuestRuntime> GetAllQuests()
    {
        return quests.Values;
    }
}