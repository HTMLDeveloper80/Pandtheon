using UnityEngine;
using TMPro;

public class StatsUI : MonoBehaviour
{
    [Header("Attributes UI")]
    public TMP_Text strengthText;
    public TMP_Text dexterityText;
    public TMP_Text intelligenceText;
    public TMP_Text vitalityText;
    public TMP_Text luckText;

    [Header("Final Stats UI")]
    public TMP_Text damageText;
    public TMP_Text critChanceText;
    public TMP_Text critDamageText;
    public TMP_Text hpText;
    public TMP_Text moveSpeedText;
    public TMP_Text attackSpeedText;

    private PlayerStats stats;

    private void Awake()
    {
        stats = FindFirstObjectByType<PlayerStats>();
    }

    private void OnEnable()
    {
        RefreshStats();
    }

    public void RefreshStats()
    {
        if (stats == null) return;

        strengthText.text = $"Strength: {stats.attributes.Strength}";
        dexterityText.text = $"Dexterity: {stats.attributes.Dexterity}";
        intelligenceText.text = $"Intelligence: {stats.attributes.Intelligence}";
        vitalityText.text = $"Vitality: {stats.attributes.Vitality}";
        luckText.text = $"Luck: {stats.attributes.Luck}";

        damageText.text = $"Damage: {stats.TotalDamage}";
        critChanceText.text = $"Crit Chance: {stats.CritChance:F1}%";
        critDamageText.text = $"Crit Damage: x{stats.CritDamageMultiplier:F2}";
        hpText.text = $"Max HP: {stats.MaxHP}";
        moveSpeedText.text = $"Move Speed: {stats.TotalMoveSpeed:F2}";
        attackSpeedText.text = $"Attack Speed: {stats.TotalAttackSpeed:F2}";
    }
}
