using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestJournalUI : MonoBehaviour
{
    [SerializeField] private Transform contentRoot;
    [SerializeField] private GameObject questRowPrefab;

    private readonly List<GameObject> spawnedRows = new();

    private void OnEnable()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestUpdated += OnQuestUpdated;

        Refresh();
    }

    private void OnDisable()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestUpdated -= OnQuestUpdated;
    }

    private void OnQuestUpdated(QuestRuntime _)
    {
        Refresh();
    }

    public void Refresh()
    {
        ClearRows();

        if (QuestManager.Instance == null) return;

        foreach (var q in QuestManager.Instance.GetAllQuests())
        {
            if (q == null || q.data == null) continue;
            if (q.state != QuestState.InProgress && q.state != QuestState.ReadyToTurnIn) continue;

            var row = Instantiate(questRowPrefab, contentRoot);
            spawnedRows.Add(row);

            var text = row.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                string suffix = q.state == QuestState.ReadyToTurnIn ? " (Gotowy do oddania)" : "";
                text.text = $"{q.data.questName}\n{q.currentAmount}/{q.data.requiredAmount}{suffix}";
            }
        }
    }

    private void ClearRows()
    {
        for (int i = 0; i < spawnedRows.Count; i++)
        {
            if (spawnedRows[i] != null)
                Destroy(spawnedRows[i]);
        }

        spawnedRows.Clear();
    }
}