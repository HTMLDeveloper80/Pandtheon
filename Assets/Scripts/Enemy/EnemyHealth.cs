using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth;
    private int currentHealth;

    private bool isDead = false;

    private EnemyStats stats;
    private EnemyLoot loot;
    [HideInInspector]public EnemyRespawner respawner;

    [SerializeField] private float destroyDelay = 0.5f;

    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
        loot = GetComponent<EnemyLoot>();
        respawner = GetComponent<EnemyRespawner>();
    }

    void Start()
    {
        if (stats != null && stats.data != null)
            maxHealth = stats.data.maxHP;

        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;
        Debug.Log($"{damage} tyle obra¿eñ dosta³ enemy!");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (loot != null)
            loot.DropLoot();

        PlayerStats player = FindFirstObjectByType<PlayerStats>();
        if (player != null && stats != null && stats.data != null)
        {
            double xpGain = stats.data.xpReward;
            player.AddXP(xpGain);

            PlayerWallet wallet = player.GetComponent<PlayerWallet>();
            if (wallet != null)
            {
                wallet.AddMoney(stats.MoneyReward);
            }

            Debug.Log($"Gracz dosta³ {stats.MoneyReward:F2} banknotów za pokonanie {stats.data.enemyType}");
        }

        if (respawner != null)
            respawner.OnEnemyKilled();

        Destroy(gameObject, destroyDelay);
    }

    public bool IsDead => isDead;
}
