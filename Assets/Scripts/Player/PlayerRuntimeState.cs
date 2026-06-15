using UnityEngine;

public static class PlayerRuntimeState
{
    private static bool hasStats;
    private static bool hasWallet;

    private static int level;
    private static double currentXP;
    private static int unspentSkillPoints;
    private static int currentHealth;
    private static int currentMana;

    private static int strength;
    private static int dexterity;
    private static int intelligence;
    private static int vitality;
    private static int luck;

    private static double money;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRuntimeState()
    {
        hasStats = false;
        hasWallet = false;
    }

    public static bool TryRestoreStats(PlayerStats stats)
    {
        if (!hasStats || stats == null)
            return false;

        stats.level = level;
        stats.currentXP = currentXP;
        stats.unspentSkillPoints = unspentSkillPoints;

        if (stats.attributes == null)
            stats.attributes = new PlayerAttributes();

        stats.attributes.Strength = strength;
        stats.attributes.Dexterity = dexterity;
        stats.attributes.Intelligence = intelligence;
        stats.attributes.Vitality = vitality;
        stats.attributes.Luck = luck;

        stats.currentHealth = Mathf.Clamp(
            currentHealth,
            0,
            stats.MaxHP);

        stats.currentMana = Mathf.Clamp(
            currentMana,
            0,
            stats.MaxMana);

        return true;
    }

    public static void SaveStats(PlayerStats stats)
    {
        if (stats == null)
            return;

        level = stats.level;
        currentXP = stats.currentXP;
        unspentSkillPoints = stats.unspentSkillPoints;
        currentHealth = stats.currentHealth;
        currentMana = stats.currentMana;

        if (stats.attributes != null)
        {
            strength = stats.attributes.Strength;
            dexterity = stats.attributes.Dexterity;
            intelligence = stats.attributes.Intelligence;
            vitality = stats.attributes.Vitality;
            luck = stats.attributes.Luck;
        }

        hasStats = true;
    }

    public static bool TryRestoreMoney(out double savedMoney)
    {
        savedMoney = money;
        return hasWallet;
    }

    public static void SaveMoney(double currentMoney)
    {
        money = System.Math.Max(0d, currentMoney);
        hasWallet = true;
    }
}
