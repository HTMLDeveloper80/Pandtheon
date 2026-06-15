using UnityEngine;

public enum MainUIPanel
{
    None,
    Inventory,
    Codex
}

public enum InventoryTab
{
    Default,
    Stats
}

public enum CodexTab
{
    Quests,
    Bestiary,
    Artifacts,
    Friends
}

public static class UIRuntimeState
{
    public static MainUIPanel OpenPanel { get; set; } = MainUIPanel.None;
    public static InventoryTab SelectedInventoryTab { get; set; } =
        InventoryTab.Default;

    public static CodexTab SelectedCodexTab { get; set; } =
        CodexTab.Quests;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRuntimeState()
    {
        OpenPanel = MainUIPanel.None;
        SelectedInventoryTab = InventoryTab.Default;
        SelectedCodexTab = CodexTab.Quests;
    }
}
