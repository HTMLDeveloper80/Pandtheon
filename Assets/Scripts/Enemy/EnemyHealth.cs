using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
public class EnemyHealth : MonoBehaviour
{
    private int currentHealth;
    private bool isDead;

    private EnemyStats stats;
    private EnemyLoot loot;
    [HideInInspector] public EnemyRespawner respawner;

    public bool IsDead => isDead;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => stats != null && stats.Data != null ? stats.Data.maxHP : 0;

    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
        loot = GetComponent<EnemyLoot>();

        if (respawner == null)
            respawner = GetComponent<EnemyRespawner>();
    }

    private void Start()
    {
        ResetHealthFromData();
    }

    public void Initialize(EnemyData enemyData, EnemyRespawner owner)
    {
        respawner = owner;

        if (stats == null)
            stats = GetComponent<EnemyStats>();

        if (stats != null)
            stats.Initialize(enemyData);

        ResetHealthFromData();
    }

    public void TakeDamage(int damage)
    {
        if (isDead || stats == null || stats.Data == null)
            return;

        currentHealth -= Mathf.Max(0, damage);
        Debug.Log($"{name} took {damage} damage. HP: {currentHealth}/{MaxHealth}");

        if (currentHealth <= 0)
            Die();
    }

    private void ResetHealthFromData()
    {
        if (stats == null || stats.Data == null)
        {
            currentHealth = 0;
            Debug.LogError($"{name}: EnemyData was not provided during initialization.");
            return;
        }

        currentHealth = stats.Data.maxHP;
        isDead = false;
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (QuestManager.Instance != null && stats != null && stats.Data != null)
            QuestManager.Instance.AddKill(stats.Data.enemyType.ToString());

        if (loot != null)
            loot.DropLoot();

        GiveRewards();
        DisableAfterDeath();

        if (respawner != null)
            respawner.OnEnemyKilled();

        float destroyDelay = stats != null && stats.Data != null
            ? stats.Data.destroyDelay
            : 0f;

        Destroy(gameObject, destroyDelay);
    }

    private void GiveRewards()
    {
        PlayerStats player = FindFirstObjectByType<PlayerStats>();
        if (player == null || stats == null || stats.Data == null)
            return;

        player.AddXP(stats.XPReward);

        PlayerWallet wallet = player.GetComponent<PlayerWallet>();
        if (wallet != null)
            wallet.AddMoney(stats.MoneyReward);

        Debug.Log($"Player received {stats.MoneyReward:F2} money for defeating {stats.Data.enemyType}");
    }

    private void DisableAfterDeath()
    {
        EnemyMovement movement = GetComponent<EnemyMovement>();
        if (movement != null)
            movement.enabled = false;

        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
}
