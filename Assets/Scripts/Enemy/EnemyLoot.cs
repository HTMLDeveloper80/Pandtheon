using UnityEngine;

public class EnemyLoot : MonoBehaviour
{
    private EnemyStats stats;

    private void Awake()
    {
        stats = GetComponent<EnemyStats>();
    }

    public void DropLoot()
    {
        if (stats == null || stats.data == null) return;

        var data = stats.data;
        if (data.dropPrefab == null)
        {
            Debug.LogWarning($"{name}: EnemyDrop.dropPrefab nie jest ustawiony!");
            return;
        }

        if (data.possibleDrops == null || data.possibleDrops.Length == 0)
            return;

        bool droppedSomething = false;

        // Szansa na drop
        foreach (var itemData in data.possibleDrops)
        {
            if (itemData == null)
                continue;

            if (Random.value <= itemData.dropChance)
            {
                Vector3 dropPos = transform.position + new Vector3(Random.Range(-0.3f, 0.3f), 0.2f, 0);

                GameObject drop = Instantiate(data.dropPrefab, dropPos, Quaternion.identity);

                var pickup = drop.GetComponent<PickupItem>();
                if (pickup == null)
                {
                    pickup = drop.AddComponent<PickupItem>();
                }
                pickup.itemData = itemData;

                droppedSomething = true;
            }
        }
    }
}
