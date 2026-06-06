using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Data source")]
    public EnemyData data;

    private EnemyHealth health;

    public EnemyData Data => data;
    public double MoneyReward => data != null ? data.moneyReward : 0;
    public double XPReward => data != null ? data.xpReward : 0;
    public int Damage => data != null ? data.damage : 1;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        ApplyToHealth();
    }

    public void ApplyData(EnemyData newData)
    {
        data = newData;
        ApplyToHealth();
    }

    private void ApplyToHealth()
    {
        if (health == null)
            health = GetComponent<EnemyHealth>();

        if (health != null && data != null)
            health.maxHealth = data.maxHP;
    }
}
