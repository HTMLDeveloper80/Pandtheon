using System;
using System.Collections;
using UnityEngine;

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
    public double currentXP;
    public double baseXP = 10;
    public double power = 2.1;
    public double multiplier = 1.05;
    public double curve = 11.0;
    public int unspentSkillPoints;

    public double XPToNextLevel =>
        baseXP *
        Math.Pow(level, power) *
        Math.Pow(multiplier, level / curve);

    public float invulnerabilityTime = 3f;

    private bool isInvulnerable;
    private bool restoredRuntimeState;

    private void Awake()
    {
        restoredRuntimeState = PlayerRuntimeState.TryRestoreStats(this);
    }

    private void Start()
    {
        if (!restoredRuntimeState)
        {
            currentHealth = MaxHP;
            currentMana = MaxMana;
            SaveRuntimeState();
        }

        ApplyMovementSpeed();
        RefreshUI();
    }

    private void OnDestroy()
    {
        SaveRuntimeState();
    }

    public int MaxHP => baseHealth + attributes.GetBonusHP();
    public int TotalDamage => baseDamage + attributes.GetBonusDamage();
    public float TotalMoveSpeed =>
        baseMoveSpeed + attributes.GetBonusMoveSpeed();

    public float TotalAttackSpeed =>
        baseAttackSpeed + attributes.GetAttackSpeed();

    public int MaxMana => attributes.GetBonusMana();
    public float CritChance => attributes.GetCritChance();
    public float CritDamageMultiplier =>
        attributes.GetCritDamageMultiplier();

    public float HPRegen => attributes.GetHPRegen();
    public float DropRateBonus => attributes.GetDropRate();

    public void AddXP(double amount)
    {
        if (amount <= 0d)
            return;

        currentXP += amount;
        TryLevelUp();
        SaveRuntimeState();
        RefreshLevelUI();
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
        currentHealth = MaxHP;

        UIManager.Instance?.UpdatePlayerHP(currentHealth, MaxHP);
        UIManager.Instance?.ShowPickupMessage(
            $"LEVEL {level}! +1 Punkt umiejetnosci");
    }

    private int GetXPToNextLevel(int levelToCheck)
    {
        double raw =
            baseXP *
            Math.Pow(levelToCheck, power) *
            Math.Pow(multiplier, levelToCheck / curve);

        return Mathf.RoundToInt((float)raw);
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable)
            return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        SaveRuntimeState();
        UIManager.Instance?.UpdatePlayerHP(currentHealth, MaxHP);

        StartCoroutine(InvulnerabilityCooldown());

        if (currentHealth <= 0)
            Die();
    }

    private IEnumerator InvulnerabilityCooldown()
    {
        isInvulnerable = true;

        SpriteRenderer spriteRenderer =
            GetComponentInChildren<SpriteRenderer>();

        for (float time = 0f;
             time < invulnerabilityTime;
             time += 0.2f)
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = !spriteRenderer.enabled;

            yield return new WaitForSeconds(0.2f);
        }

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        isInvulnerable = false;
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(
            currentHealth + amount,
            0,
            MaxHP);

        SaveRuntimeState();
        UIManager.Instance?.UpdatePlayerHP(currentHealth, MaxHP);
    }

    public void SaveRuntimeState()
    {
        PlayerRuntimeState.SaveStats(this);
    }

    private void ApplyMovementSpeed()
    {
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.moveSpeed = TotalMoveSpeed;
    }

    private void RefreshUI()
    {
        UIManager.Instance?.UpdatePlayerHP(currentHealth, MaxHP);
        RefreshLevelUI();
    }

    private void RefreshLevelUI()
    {
        UIManager.Instance?.UpdatePlayerLevel(
            level,
            (float)currentXP,
            GetXPToNextLevel(level));
    }

    private void Die()
    {
        Debug.Log("Gracz zginal!");
    }
}
