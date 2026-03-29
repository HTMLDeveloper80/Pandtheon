using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Data source")]
    public EnemyData data;

    private EnemyHealth health;

    public EnemyData Data => data;
    public double MoneyReward => data != null ? data.moneyReward : 0;
    public double XPReward => data != null ? data.xpReward : 0;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();

        if (data == null)
        {
            Debug.LogError($"{name}: EnemyData is not assigned on EnemyStats!");
            return;
        }

        if (health != null)
        {
            health.maxHealth = data.maxHP;
        }
    }
}
