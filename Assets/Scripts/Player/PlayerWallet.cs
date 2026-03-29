using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    [Header("Player Money")]
    [SerializeField] private double money = 0f;

    public double Money => money;

    private void Start()
    {
        // ?? odœwie¿ UI po starcie
        Invoke(nameof(DelayedUIUpdate), 0.1f);
    }

    void DelayedUIUpdate()
    {
        InventoryManager.Instance?.UpdateMoneyUI();
    }

    public void AddMoney(double amount)
    {
        if (amount <= 0)
            return;

        money += amount;
        InventoryManager.Instance?.UpdateMoneyUI();
        Debug.Log($"Player gained {amount:F2} banknotes. Total: {money:F2}");
    }

    public void SpendMoney(double amount)
    {
        if (amount <= 0)
            return;

        if (money >= amount)
        {
            money -= amount;
            InventoryManager.Instance?.UpdateMoneyUI();
            Debug.Log($"Player spent {amount:F2} banknotes. Remaining: {money:F2}");
        }
        else
        {
            Debug.LogWarning("Not enough money!");
        }
    }

    public void SetMoney(float newValue)
    {
        money = Mathf.Max(0f, newValue);
        InventoryManager.Instance?.UpdateMoneyUI();
    }
}
