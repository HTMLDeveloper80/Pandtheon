using UnityEngine;

public class InventoryTabController : MonoBehaviour
{
    public GameObject defaultPanel;
    public GameObject statsPanel;

    private void Start()
    {
        ApplySavedTab();
    }

    public void ShowDefault()
    {
        UIRuntimeState.SelectedInventoryTab = InventoryTab.Default;
        SetPanels(showDefault: true);
    }

    public void ShowStats()
    {
        UIRuntimeState.SelectedInventoryTab = InventoryTab.Stats;
        SetPanels(showDefault: false);
    }

    private void ApplySavedTab()
    {
        SetPanels(
            UIRuntimeState.SelectedInventoryTab == InventoryTab.Default);
    }

    private void SetPanels(bool showDefault)
    {
        if (defaultPanel != null)
            defaultPanel.SetActive(showDefault);

        if (statsPanel != null)
            statsPanel.SetActive(!showDefault);
    }
}
