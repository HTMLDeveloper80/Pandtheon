using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    [Header("Player Money")]
    [SerializeField] private double money;

    public double Money => money;

    private void Awake()
    {
        if (PlayerRuntimeState.TryRestoreMoney(out double savedMoney))
            money = savedMoney;
        else
            PlayerRuntimeState.SaveMoney(money);
    }

    private void Start()
    {
        InventoryManager.Instance?.UpdateMoneyUI();
    }

    private void OnDestroy()
    {
        PlayerRuntimeState.SaveMoney(money);
    }

    public void AddMoney(double amount)
    {
        if (amount <= 0d)
            return;

        money += amount;
        SaveAndRefresh();
    }

    public void SpendMoney(double amount)
    {
        if (amount <= 0d)
            return;

        if (money < amount)
        {
            Debug.LogWarning("Not enough money!");
            return;
        }

        money -= amount;
        SaveAndRefresh();
    }

    public void SetMoney(float newValue)
    {
        money = Mathf.Max(0f, newValue);
        SaveAndRefresh();
    }

    private void SaveAndRefresh()
    {
        PlayerRuntimeState.SaveMoney(money);
        InventoryManager.Instance?.UpdateMoneyUI();
    }
}
