using UnityEngine;

public class InventoryTabController : MonoBehaviour
{
    public GameObject defaultPanel;
    public GameObject statsPanel;

    public void ShowDefault()
    {
        defaultPanel.SetActive(true);
        statsPanel.SetActive(false);
    }

    public void ShowStats()
    {
        defaultPanel.SetActive(false);
        statsPanel.SetActive(true);
    }
}
