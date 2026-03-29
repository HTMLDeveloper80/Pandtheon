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
    [SerializeField] private CanvasGroup inventoryCanvasgroup;
    private bool isInventoryOpen = false;

    [Header("Pickup/Feedback UI")]
    [SerializeField] private TMP_Text pickupText;
    [SerializeField] private float pickupTextDuration = 2.5f;

    private Coroutine pickupRoutine;

    [Header("XP UI")]
    [SerializeField] private UnityEngine.UI.Slider xpSlider;
    [SerializeField] private TMPro.TMP_Text xpText;
    [SerializeField] private TMPro.TMP_Text levelText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (inventoryCanvasgroup != null)
        {
            inventoryCanvasgroup.alpha = 0f;
            inventoryCanvasgroup.interactable = false;
            inventoryCanvasgroup.blocksRaycasts = false;
        }
        else if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        if (pickupText != null)
        {
            pickupText.text = "";
        }
    }

    public void UpdatePlayerHP(int current, int max)
    {
        float value = max > 0 ? (float)current / max : 0f;
        playerHPSlider.value = value;
        playerHPText.text = $"{current} / {max}";
    }

    public void UpdatePlayerLevel(int level, float currentXP, float xpToNext)
    {
        if (xpSlider != null)
            xpSlider.value = Mathf.Clamp01(currentXP / xpToNext);

        if (xpText != null)
            xpText.text = $"{Mathf.RoundToInt(currentXP)} / {Mathf.RoundToInt(xpToNext)} XP";

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

        if (inventoryCanvasgroup != null)
        {
            inventoryCanvasgroup.alpha = 1f;
            inventoryCanvasgroup.interactable = true;
            inventoryCanvasgroup.blocksRaycasts = true;
        }
        else if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }
    }

    public void CloseInventory()
    {
        isInventoryOpen = false;

        if (inventoryCanvasgroup != null)
        {
            inventoryCanvasgroup.alpha = 0f;
            inventoryCanvasgroup.interactable = false;
            inventoryCanvasgroup.blocksRaycasts = false;
        }
        else if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }

    public bool IsInventoryOpen()
    {
        return isInventoryOpen;
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
