using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
public class EnemyLoot : MonoBehaviour
{
    private EnemyStats stats;

    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
    }

    public void DropLoot()
    {
        if (stats == null)
            stats = GetComponent<EnemyStats>();

        EnemyData data = stats != null ? stats.Data : null;
        if (data == null)
        {
            Debug.LogWarning($"{name}: EnemyLoot cannot drop items without EnemyData.");
            return;
        }

        if (data.possibleDrops == null || data.possibleDrops.Length == 0)
            return;

        if (data.dropPrefab == null)
        {
            Debug.LogWarning($"{name}: Drop Prefab is not assigned in {data.name}.");
            return;
        }

        foreach (ItemData itemData in data.possibleDrops)
        {
            if (itemData == null)
                continue;

            float dropChance = Mathf.Clamp01(itemData.dropChance);
            if (Random.value > dropChance)
                continue;

            Vector3 dropPosition = transform.position +
                new Vector3(Random.Range(-0.3f, 0.3f), 0.2f, 0f);

            GameObject drop = Instantiate(
                data.dropPrefab,
                dropPosition,
                Quaternion.identity
            );

            PickupItem pickup = drop.GetComponent<PickupItem>();
            if (pickup == null)
                pickup = drop.AddComponent<PickupItem>();

            pickup.itemData = itemData;
        }
    }
}
