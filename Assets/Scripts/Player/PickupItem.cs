using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public ItemData itemData;
    public float pickupDelay = 0.2f;
    private bool collected = false;

    public void TryPickup()
    {
        if (collected)
            return;

        if (UIManager.Instance != null && UIManager.Instance.IsInventoryOpen())
            return;

        StartCoroutine(PickupRoutine());
    }

    private System.Collections.IEnumerator PickupRoutine()
    {
        yield return new WaitForSeconds(pickupDelay);

        if (collected)
            yield break;

        if (itemData == null)
        {
            Debug.LogError($"{name}: PickupItem nie ma przypisanego ItemData!");
            yield break;
        }

        InventoryManager inventory = InventoryManager.Instance;

        if (inventory == null)
        {
            Debug.LogError("Brak InventoryManager w aktualnej scenie!");
            yield break;
        }

        inventory.RefreshSlots();

        if (!inventory.HasFreeSlot())
        {
            Debug.Log("Inventory full - item stays on the ground.");
            yield break;
        }

        collected = true;
        inventory.AddItem(itemData);

        UIManager.Instance?.ShowPickupMessage(
            $"+{itemData.amount} {itemData.itemName}"
        );

        Destroy(gameObject);
    }
}
