using System.Collections;
using TMPro;
using UnityEngine;

public class NpcDialogBubbleUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text dialogText;

    [Header("Timing")]
    [SerializeField] private float autoHideAfter = 3f;

    private Coroutine hideRoutine;

    private void Awake()
    {
        HideImmediate();
    }

    public void ShowText(string text)
    {
        if (dialogText != null)
            dialogText.text = text ?? string.Empty;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        hideRoutine = StartCoroutine(AutoHide());
    }

    public void HideImmediate()
    {
        if (hideRoutine != null)
        {
            StopCoroutine(hideRoutine);
            hideRoutine = null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (dialogText != null)
            dialogText.text = string.Empty;
    }

    private IEnumerator AutoHide()
    {
        yield return new WaitForSeconds(autoHideAfter);

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }
}