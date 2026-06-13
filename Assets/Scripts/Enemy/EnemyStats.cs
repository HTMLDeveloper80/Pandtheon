using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public EnemyData Data { get; private set; }
    public double MoneyReward => Data != null ? Data.moneyReward : 0;
    public double XPReward => Data != null ? Data.xpReward : 0;
    public int Damage => Data != null ? Data.damage : 0;

    public void Initialize(EnemyData enemyData)
    {
        Data = enemyData;
    }
}
