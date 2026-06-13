using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Serializable]
    private class StoredItem
    {
        public ItemData data;
        public int amount;

        public StoredItem(ItemData data, int amount)
        {
            this.data = data;
            this.amount = amount;
        }
    }

    public static InventoryManager Instance { get; private set; }

    // Dane sa wspolne dla wszystkich scen, ale UI slotow pozostaje lokalne.
    private static readonly List<StoredItem> storedItems = new List<StoredItem>();

    [Header("Slots")]
    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();
    [SerializeField] private Transform slotContainer;

    [Header("Money Display")]
    [SerializeField] private TMP_Text moneyText;

    private PlayerWallet playerWallet;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRuntimeState()
    {
        Instance = null;
        storedItems.Clear();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        RefreshSlots();
        DisplayStoredItems();
    }

    private void Start()
    {
        StartCoroutine(InitWallet());
        RefreshSlots();
        DisplayStoredItems();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private IEnumerator InitWallet()
    {
        yield return null;
        playerWallet = FindFirstObjectByType<PlayerWallet>();
        UpdateMoneyUI();
    }

    public void RefreshSlots()
    {
        slots.Clear();

        if (slotContainer != null)
            slots.AddRange(slotContainer.GetComponentsInChildren<InventorySlot>(true));
        else
            slots.AddRange(FindObjectsByType<InventorySlot>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.InstanceID));
    }

    public bool CanAddItem(ItemData data)
    {
        if (data == null)
            return false;

        RefreshSlots();

        foreach (StoredItem item in storedItems)
        {
            if (item.data == data)
                return true;
        }

        return storedItems.Count < slots.Count;
    }

    public bool AddItem(ItemData data)
    {
        if (!CanAddItem(data))
        {
            Debug.LogWarning($"Inventory pelne. Nie dodano: {data?.itemName}");
            return false;
        }

        foreach (StoredItem item in storedItems)
        {
            if (item.data != data)
                continue;

            item.amount += Mathf.Max(1, data.amount);
            DisplayStoredItems();
            return true;
        }

        storedItems.Add(new StoredItem(data, Mathf.Max(1, data.amount)));
        DisplayStoredItems();
        return true;
    }

    public void SaveCurrentSlotOrder()
    {
        RefreshSlots();
        storedItems.Clear();

        foreach (InventorySlot slot in slots)
        {
            if (slot.HasItem && slot.ItemRef != null)
                storedItems.Add(new StoredItem(slot.ItemRef, slot.Amount));
        }
    }

    private void DisplayStoredItems()
    {
        foreach (InventorySlot slot in slots)
            slot.ClearSlot();

        int count = Mathf.Min(storedItems.Count, slots.Count);
        for (int i = 0; i < count; i++)
            slots[i].SetItem(storedItems[i].data, storedItems[i].amount);
    }

    public void UpdateMoneyUI()
    {
        if (moneyText == null || playerWallet == null)
            return;

        moneyText.text = $"{playerWallet.Money:F2} $";
    }

    public bool HasFreeSlot()
    {
        RefreshSlots();
        return storedItems.Count < slots.Count;
    }
}
