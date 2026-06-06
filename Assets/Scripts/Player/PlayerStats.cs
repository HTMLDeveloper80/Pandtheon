using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Analytics;

public class PlayerStats : MonoBehaviour
{
    [Header("Basic stats")]
    public int baseHealth = 10;
    public int baseDamage = 3;
    public float baseMoveSpeed = 5f;
    public float baseAttackSpeed = 1f;

    [Header("Attributes")]
    public PlayerAttributes attributes = new PlayerAttributes();

    [Header("Current State")]
    public int currentHealth;
    public int currentMana;

    [Header("Levelling System")]
    public int level = 1;
    public double currentXP = 0;
    public double baseXP = 10;
    public double power = 2.1;
    public double multiplier = 1.05;
    public double curve = 11.0;
    public int unspentSkillPoints = 0;
    public double XPToNextLevel => baseXP * Math.Pow(level, power) * Math.Pow(multiplier, level / curve);

    private bool isInvulnerable = false;
    public float invulnerabilityTime = 3f;

    void Start()
    {
        currentHealth = MaxHP;
        currentMana = MaxMana;

        UIManager.Instance?.UpdatePlayerHP(currentHealth, MaxHP);
        UIManager.Instance?.UpdatePlayerLevel(level, (float)currentXP, (float)XPToNextLevel);

        var move = GetComponent<PlayerMovement>();
        if (move != null)
        {
            move.moveSpeed = TotalMoveSpeed;
        }
    }

    public int MaxHP => baseHealth + attributes.GetBonusHP();
    public int TotalDamage => baseDamage + attributes.GetBonusDamage();
    public float TotalMoveSpeed => baseMoveSpeed + attributes.GetBonusMoveSpeed();
    public float TotalAttackSpeed => baseAttackSpeed + attributes.GetAttackSpeed();
    public int MaxMana => attributes.GetBonusMana();
    public float CritChance => attributes.GetCritChance();
    public float CritDamageMultiplier => attributes.GetCritDamageMultiplier();
    public float HPRegen => attributes.GetHPRegen();
    public float DropRateBonus => attributes.GetDropRate();
    public void AddXP(double amount)
    {
        currentXP += amount;

        // log testowy – możesz zostawić
        Debug.Log($"XP = {currentXP:F6} / {GetXPToNextLevel(level):F6}");

        TryLevelUp();

        UIManager.Instance?.UpdatePlayerLevel(level, (float)currentXP, GetXPToNextLevel(level));
    }

    private void TryLevelUp()
    {
        int requiredXP = GetXPToNextLevel(level);

        while (currentXP >= requiredXP)
        {
            currentXP -= requiredXP;
            LevelUp();

            requiredXP = GetXPToNextLevel(level);
        }
    }


    private void LevelUp()
    {
        level++;
        unspentSkillPoints++;

        Debug.Log($"🎉 LEVEL UP! Teraz poziom {level}");

        currentHealth = MaxHP;

        UIManager.Instance?.UpdatePlayerHP(currentHealth, MaxHP);
        UIManager.Instance?.ShowPickupMessage($"LEVEL {level}! +1 Punkt umiejętności");
    }

    private int GetXPToNextLevel(int levelToCheck)
    {
        double raw = baseXP * Math.Pow(levelToCheck, power) * Math.Pow(multiplier, levelToCheck / curve);
        return Mathf.RoundToInt((float)raw); // 🔹 Unity'owy int z zaokrągleniem
    }


    public void TakeDamage(int dmg)
    {
        if (isInvulnerable)
            return;

        currentHealth -= dmg;
        if (currentHealth < 0)
            currentHealth = 0;

        UIManager.Instance?.UpdatePlayerHP(currentHealth, MaxHP);
        StartCoroutine(InvulnerabilityCooldown());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator InvulnerabilityCooldown()
    {
        isInvulnerable = true;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        for (float t = 0; t < invulnerabilityTime; t += 0.2f)
        {
            if (sr != null)
                sr.enabled = !sr.enabled;

            yield return new WaitForSeconds(0.2f);
        }

        if (sr != null)
            sr.enabled = true;

        isInvulnerable = false;
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > MaxHP)
            currentHealth = MaxHP;

        UIManager.Instance?.UpdatePlayerHP(currentHealth, MaxHP);
    }

    void Die()
    {
        Debug.Log("Gracz zginął!");
    }
}

public static class Mathd
{
    public static double Pow(double x, double y)
    {
        return System.Math.Pow(x, y);
    }
}

