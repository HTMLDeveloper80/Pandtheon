using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quests/Quest")]
public class QuestData : ScriptableObject
{
    public string questName;
    [TextArea] public string description;

    [Header("Cel")]
    public string targetEnemyName;
    public int requiredAmount;

    [Header("Nagrody")]
    public int xpReward;
    public double moneyReward;
}