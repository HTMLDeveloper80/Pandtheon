using UnityEngine;

public enum EnemyType
{
    Dwarf,
    Wolf,
    WildSundew,
    Alligator,
    Cannibal,
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game Data/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Statistics")]
    public EnemyType enemyType;
    public int maxHP;
    public int damage;
    public double moneyReward;
    public double xpReward;

    [Header("Loot")]
    public ItemData[] possibleDrops;
    public GameObject dropPrefab;
}
