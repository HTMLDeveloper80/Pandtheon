using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Player HP UI")]
    [SerializeField] private Slider playerHPSlider;
    [SerializeField] private TMP_Text playerHPText;

    [Header("Inventory UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private CanvasGroup inventoryCanvasGroup;
    private bool isInventoryOpen = false;

    [Header("Codex UI")]
    [SerializeField] private GameObject codexPanel;
    [SerializeField] private CanvasGroup codexCanvasGroup;
    private bool isCodexOpen = false;

    [Header("Pickup/Feedback UI")]
    [SerializeField] private TMP_Text pickupText;
    [SerializeField] private float pickupTextDuration = 2.5f;

    private Coroutine pickupRoutine;

    [Header("XP UI")]
    [SerializeField] private Slider xpSlider;
    [SerializeField] private TMP_Text xpText;
    [SerializeField] private TMP_Text levelText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitPanelClosed(inventoryCanvasGroup, inventoryPanel);
        InitPanelClosed(codexCanvasGroup, codexPanel);

        if (pickupText != null)
            pickupText.text = "";
    }

    private void InitPanelClosed(CanvasGroup cg, GameObject panel)
    {
        if (cg != null)
        {
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
        else if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private void SetPanelState(CanvasGroup cg, GameObject panel, bool isOpen)
    {
        if (cg != null)
        {
            cg.alpha = isOpen ? 1f : 0f;
            cg.interactable = isOpen;
            cg.blocksRaycasts = isOpen;
        }
        else if (panel != null)
        {
            panel.SetActive(isOpen);
        }
    }

    public void UpdatePlayerHP(int current, int max)
    {
        float value = max > 0 ? (float)current / max : 0f;

        if (playerHPSlider != null)
            playerHPSlider.value = value;

        if (playerHPText != null)
            playerHPText.text = $"{current} / {max}";
    }

    public void UpdatePlayerLevel(int level, float currentXP, float xpToNext)
    {
        if (xpSlider != null && xpToNext > 0f)
            xpSlider.value = Mathf.Clamp01(currentXP / xpToNext);

        if (xpText != null)
            xpText.text = $"{Mathf.RoundToInt(currentXP)} / {Mathf.RoundToInt(xpToNext)} XP";

        if (levelText != null)
            levelText.text = $"Lv. {level}";
    }

    // ---------- INVENTORY ----------
    public void ToggleInventory()
    {
        if (isInventoryOpen) CloseInventory();
        else OpenInventory();
    }

    public void OpenInventory()
    {
        isInventoryOpen = true;
        SetPanelState(inventoryCanvasGroup, inventoryPanel, true);

        // opcjonalnie: tylko jeden panel naraz
        if (isCodexOpen)
            CloseCodex();
    }

    public void CloseInventory()
    {
        isInventoryOpen = false;
        SetPanelState(inventoryCanvasGroup, inventoryPanel, false);
    }

    public bool IsInventoryOpen() => isInventoryOpen;

    // ---------- CODEX ----------
    public void ToggleCodex()
    {
        if (isCodexOpen) CloseCodex();
        else OpenCodex();
    }

    public void OpenCodex()
    {
        isCodexOpen = true;
        SetPanelState(codexCanvasGroup, codexPanel, true);

        // opcjonalnie: tylko jeden panel naraz
        if (isInventoryOpen)
            CloseInventory();
    }

    public void CloseCodex()
    {
        isCodexOpen = false;
        SetPanelState(codexCanvasGroup, codexPanel, false);
    }

    public bool IsCodexOpen() => isCodexOpen;

    // ---------- PICKUP MESSAGE ----------
    public void ShowPickupMessage(string message)
    {
        if (pickupText == null)
            return;

        if (pickupRoutine != null)
            StopCoroutine(pickupRoutine);

        pickupRoutine = StartCoroutine(ShowPickupCoroutine(message));
    }

    private IEnumerator ShowPickupCoroutine(string message)
    {
        pickupText.text = message;
        pickupText.alpha = 1f;

        yield return new WaitForSeconds(pickupTextDuration);

        float fadeTime = 0.5f;
        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            pickupText.alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
            yield return null;
        }

        pickupText.text = "";
    }
}