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
    [Header("Identity")]
    public EnemyType enemyType;

    [Header("Combat")]
    [Min(1)] public int maxHP = 1;
    [Min(0)] public int damage = 1;
    [Min(0.05f)] public float contactDamageCooldown = 1f;

    [Header("Rewards")]
    public double moneyReward;
    public double xpReward;

    [Header("Movement")]
    [Min(0f)] public float moveSpeed = 0.6f;
    [Min(0f)] public float chaseSpeedMultiplier = 1f;
    public bool startsMovingRight = true;

    [Header("Patrol")]
    public bool keepNearSpawn = true;
    [Min(0f)] public float patrolRadius = 1.5f;
    public Vector2 moveDurationRange = new Vector2(1f, 2.5f);
    public Vector2 idleDurationRange = new Vector2(2f, 4f);
    [Range(0f, 1f)] public float patrolMoveChance = 0.5f;
    [Range(0f, 1f)] public float directionChangeChance = 0.35f;

    [Header("Chase")]
    public bool chasePlayer = true;
    [Min(0f)] public float detectRange = 2f;
    [Min(0f)] public float loseRange = 3f;
    [Min(0f)] public float stopDistanceFromPlayer = 0.9f;

    [Header("Environment checks")]
    [Min(0.01f)] public float groundCheckDistance = 0.5f;
    [Min(0.01f)] public float wallCheckDistance = 0.2f;

    [Header("Lifecycle")]
    [Min(0f)] public float destroyDelay = 0.5f;
    [Min(0f)] public float respawnDelay = 30f;

    [Header("Loot")]
    public ItemData[] possibleDrops;
    public GameObject dropPrefab;

    private void OnValidate()
    {
        maxHP = Mathf.Max(1, maxHP);
        damage = Mathf.Max(0, damage);
        contactDamageCooldown = Mathf.Max(0.05f, contactDamageCooldown);
        moveSpeed = Mathf.Max(0f, moveSpeed);
        chaseSpeedMultiplier = Mathf.Max(0f, chaseSpeedMultiplier);
        patrolRadius = Mathf.Max(0f, patrolRadius);
        detectRange = Mathf.Max(0f, detectRange);
        loseRange = Mathf.Max(detectRange, loseRange);
        stopDistanceFromPlayer = Mathf.Max(0f, stopDistanceFromPlayer);
        groundCheckDistance = Mathf.Max(0.01f, groundCheckDistance);
        wallCheckDistance = Mathf.Max(0.01f, wallCheckDistance);
        destroyDelay = Mathf.Max(0f, destroyDelay);
        respawnDelay = Mathf.Max(0f, respawnDelay);

        moveDurationRange = NormalizeRange(moveDurationRange);
        idleDurationRange = NormalizeRange(idleDurationRange);
    }

    private static Vector2 NormalizeRange(Vector2 range)
    {
        float min = Mathf.Max(0.01f, Mathf.Min(range.x, range.y));
        float max = Mathf.Max(min, Mathf.Max(range.x, range.y));
        return new Vector2(min, max);
    }
}
