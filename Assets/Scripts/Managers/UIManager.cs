using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Player HP UI")]
    [SerializeField] private Slider playerHPSlider;
    [SerializeField] private TMP_Text playerHPText;

    [Header("Inventory UI")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private CanvasGroup inventoryCanvasGroup;
    private bool isInventoryOpen;

    [Header("Codex UI")]
    [SerializeField] private GameObject codexPanel;
    [SerializeField] private CanvasGroup codexCanvasGroup;
    private bool isCodexOpen;

    [Header("Pickup/Feedback UI")]
    [SerializeField] private TMP_Text pickupText;
    [SerializeField] private float pickupTextDuration = 2.5f;

    [Header("XP UI")]
    [SerializeField] private Slider xpSlider;
    [SerializeField] private TMP_Text xpText;
    [SerializeField] private TMP_Text levelText;

    private Coroutine pickupRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        RestorePanelState();

        if (pickupText != null)
            pickupText.text = "";
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void RestorePanelState()
    {
        isInventoryOpen = UIRuntimeState.OpenPanel == MainUIPanel.Inventory;
        isCodexOpen = UIRuntimeState.OpenPanel == MainUIPanel.Codex;

        SetPanelState(
            inventoryCanvasGroup,
            inventoryPanel,
            isInventoryOpen);

        SetPanelState(
            codexCanvasGroup,
            codexPanel,
            isCodexOpen);
    }

    private void SetPanelState(
        CanvasGroup canvasGroup,
        GameObject panel,
        bool isOpen)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = isOpen ? 1f : 0f;
            canvasGroup.interactable = isOpen;
            canvasGroup.blocksRaycasts = isOpen;
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
        {
            xpText.text =
                $"{Mathf.RoundToInt(currentXP)} / {Mathf.RoundToInt(xpToNext)} XP";
        }

        if (levelText != null)
            levelText.text = $"Lv. {level}";
    }

    public void ToggleInventory()
    {
        if (isInventoryOpen)
            CloseInventory();
        else
            OpenInventory();
    }

    public void OpenInventory()
    {
        isInventoryOpen = true;
        SetPanelState(inventoryCanvasGroup, inventoryPanel, true);

        if (isCodexOpen)
            CloseCodex(saveClosedState: false);

        UIRuntimeState.OpenPanel = MainUIPanel.Inventory;
    }

    public void CloseInventory()
    {
        CloseInventory(saveClosedState: true);
    }

    private void CloseInventory(bool saveClosedState)
    {
        isInventoryOpen = false;
        SetPanelState(inventoryCanvasGroup, inventoryPanel, false);

        if (saveClosedState)
            UIRuntimeState.OpenPanel = MainUIPanel.None;
    }

    public bool IsInventoryOpen()
    {
        return isInventoryOpen;
    }

    public void ToggleCodex()
    {
        if (isCodexOpen)
            CloseCodex();
        else
            OpenCodex();
    }

    public void OpenCodex()
    {
        isCodexOpen = true;
        SetPanelState(codexCanvasGroup, codexPanel, true);

        if (isInventoryOpen)
            CloseInventory(saveClosedState: false);

        UIRuntimeState.OpenPanel = MainUIPanel.Codex;
    }

    public void CloseCodex()
    {
        CloseCodex(saveClosedState: true);
    }

    private void CloseCodex(bool saveClosedState)
    {
        isCodexOpen = false;
        SetPanelState(codexCanvasGroup, codexPanel, false);

        if (saveClosedState)
            UIRuntimeState.OpenPanel = MainUIPanel.None;
    }

    public bool IsCodexOpen()
    {
        return isCodexOpen;
    }

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

        const float fadeTime = 0.5f;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            pickupText.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            yield return null;
        }

        pickupText.text = "";
    }
}
