using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quests/Quest")]
public class QuestData : ScriptableObject
{
    public string questName;
    [TextArea] public string description;

    [Header("Cel")]
    [FormerlySerializedAs("targetID")]
    public string targetEnemyName;
    public int requiredAmount;

    [Header("Nagrody")]
    public int xpReward;
    public double moneyReward;
}
