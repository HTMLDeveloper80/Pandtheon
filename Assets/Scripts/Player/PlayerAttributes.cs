using UnityEngine;

[System.Serializable]
public class PlayerAttributes
{
    public int Strength = 5;
    public int Dexterity = 5;
    public int Intelligence = 5;
    public int Vitality = 5;
    public int Luck = 5;

    // Bonuses from stats
    public int GetBonusDamage() => Strength * 1;
    public float GetCritDamageMultiplier() => 1.5f + (Strength * 0.005f);

    public float GetBonusMoveSpeed() => Dexterity * 0.02f;
    public float GetAttackSpeed() => Dexterity * 0.01f;

    public int GetBonusMana() => Intelligence * 1;
    public int GetMagicDamage() => Intelligence * 1;

    public int GetBonusHP() => Vitality * 1;
    public float GetHPRegen() => Vitality * 0.1f;

    public float GetCritChance() => Luck * 0.01f;
    public float GetDropRate() => Luck * 0.01f;
}
