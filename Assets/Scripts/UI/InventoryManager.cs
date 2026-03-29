using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Slots")]
    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();
    [SerializeField] private Transform slotContainer;

    [Header("Money Display")]
    [SerializeField] private TMP_Text moneyText;
    private PlayerWallet playerWallet;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log($"[InventoryManager] ✅ Awake() działa na obiekcie: {gameObject.name}");

        RefreshSlots();
        Debug.Log($"[InventoryManager] 🧩 Wykryto slotów: {slots.Count}");
    }


    private void Start()
    {
        StartCoroutine(InitWallet());

        RefreshSlots();

        Debug.Log($"[InventoryManager] 🔍 Liczba slotów wykryta: {slots.Count}");
        int i = 0;
        foreach (var s in slots)
        {
            Debug.Log($"[InventoryManager] Slot {i}: {s.name}, HasItem={s.HasItem}");
            i++;
        }
    }

    private System.Collections.IEnumerator InitWallet()
    {
        yield return null; // czeka 1 frame
        playerWallet = FindFirstObjectByType<PlayerWallet>();

        if (playerWallet == null)
        {
            Debug.LogError("PlayerWallet nadal nie znaleziony! Upewnij się, że gracz ma ten komponent!");
        }
        else
        {
            Debug.Log("PlayerWallet znaleziony, aktualizuję UI.");
        }

        UpdateMoneyUI();
    }

    public void RefreshSlots()
    {
        slots.Clear();

        if (slotContainer != null)
        {
            slots.AddRange(slotContainer.GetComponentsInChildren<InventorySlot>(true));
            Debug.Log($"[InventoryManager] 🧩 Znaleziono {slots.Count} slotów w {slotContainer.name}");
        }
        else
        {
            slots.AddRange(GetComponentsInChildren<InventorySlot>(true));
            Debug.Log($"[InventoryManager] ⚠️ slotContainer nie ustawiony — szukam lokalnie, znaleziono: {slots.Count}");
        }
    }


    public void AddItem(ItemData data)
    {
        if (data == null)
        {
            Debug.LogWarning("Próba dodania pustego itemData!");
            return;
        }

        // Upewnij się, że lista jest aktualna
        RefreshSlots();

        // Szukaj identycznego przedmiotu (po referencji)
        foreach (var slot in slots)
        {
            if (slot.HasItem && slot.ItemRef == data)
            {
                slot.AddAmount(data.amount);
                Debug.Log($"Zstackowano {data.amount}x {data.itemName}");
                UpdateMoneyUI();
                return;
            }
        }

        // Szukaj pustego slota
        foreach (var slot in slots)
        {
            if (!slot.HasItem)
            {
                slot.SetItem(data);
                Debug.Log($"Dodano nowy przedmiot: {data.itemName}");
                UpdateMoneyUI();
                return;
            }
        }

        Debug.LogWarning("❌ Inventory pełne! Nie udało się dodać " + data.itemName);
    }



    public void UpdateMoneyUI()
    {
        if (moneyText == null)
        {
            Debug.LogWarning("moneyText NIE jest przypisany w InventoryManager!");
            return;
        }

        if (playerWallet == null)
        {
            Debug.LogWarning("PlayerWallet jeszcze nie znaleziony — spróbuj ponownie.");
            return;
        }

        moneyText.text = $"{playerWallet.Money:F2} $";
        Debug.Log($"[InventoryManager] Zaktualizowano pieniądze: {playerWallet.Money:F2} $");
    }

    public bool HasFreeSlot()
    {
        foreach (var slot in slots)
        {
            if (!slot.HasItem)
                return true;    // istnieje wolne miejsce
        }
        return false;
    }

}
