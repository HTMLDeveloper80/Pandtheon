using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Serializable]
    private class StoredSlot
    {
        public ItemData data;
        public int amount;

        public StoredSlot(ItemData data, int amount)
        {
            this.data = data;
            this.amount = amount;
        }
    }

    public static InventoryManager Instance { get; private set; }

    // Dane sa wspolne dla wszystkich scen, ale UI slotow pozostaje lokalne.
    private static readonly List<StoredSlot> storedSlots = new List<StoredSlot>();

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
        storedSlots.Clear();
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
        EnsureStoredSlotCount();

        foreach (StoredSlot slot in storedSlots)
        {
            if (slot.data == data)
                return true;
        }

        foreach (StoredSlot slot in storedSlots)
        {
            if (slot.data == null)
                return true;
        }

        return false;
    }

    public bool AddItem(ItemData data)
    {
        if (!CanAddItem(data))
        {
            Debug.LogWarning($"Inventory pelne. Nie dodano: {data?.itemName}");
            return false;
        }

        EnsureStoredSlotCount();

        foreach (StoredSlot slot in storedSlots)
        {
            if (slot.data != data)
                continue;

            slot.amount += Mathf.Max(1, data.amount);
            DisplayStoredItems();
            return true;
        }

        foreach (StoredSlot slot in storedSlots)
        {
            if (slot.data != null)
                continue;

            slot.data = data;
            slot.amount = Mathf.Max(1, data.amount);
            DisplayStoredItems();
            return true;
        }

        return false;
    }

    public void SaveCurrentSlotOrder()
    {
        RefreshSlots();
        storedSlots.Clear();

        foreach (InventorySlot slot in slots)
        {
            if (slot.HasItem && slot.ItemRef != null)
                storedSlots.Add(new StoredSlot(slot.ItemRef, slot.Amount));
            else
                storedSlots.Add(new StoredSlot(null, 0));
        }
    }

    private void DisplayStoredItems()
    {
        EnsureStoredSlotCount();

        foreach (InventorySlot slot in slots)
            slot.ClearSlot();

        int count = Mathf.Min(storedSlots.Count, slots.Count);
        for (int i = 0; i < count; i++)
        {
            StoredSlot storedSlot = storedSlots[i];
            if (storedSlot.data != null)
                slots[i].SetItem(storedSlot.data, storedSlot.amount);
        }
    }

    private void EnsureStoredSlotCount()
    {
        while (storedSlots.Count < slots.Count)
            storedSlots.Add(new StoredSlot(null, 0));

        if (storedSlots.Count > slots.Count)
            storedSlots.RemoveRange(slots.Count, storedSlots.Count - slots.Count);
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
        EnsureStoredSlotCount();

        foreach (StoredSlot slot in storedSlots)
        {
            if (slot.data == null)
                return true;
        }

        return false;
    }
}
