using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text amountText;

    public string ItemName { get; private set; }
    public int Amount { get; private set; }
    public Sprite Icon { get; private set; }
    public bool HasItem { get; private set; }
    public ItemData ItemRef { get; private set; }

    private Canvas canvas;
    private GameObject dragGhost;
    private RectTransform dragRect;
    private Transform draggableLayer;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        draggableLayer = canvas.transform.Find("DraggableLayer");
        if (draggableLayer == null)
        {
            Debug.LogError("❌ Brak obiektu 'DraggableLayer' w Canvas!");
        }
    }

    public void SetItem(ItemData data)
    {
        if (data == null) return;

        ItemRef = data;
        ItemName = data.itemName;
        Amount = data.amount;
        Icon = data.icon;
        HasItem = true;

        iconImage.sprite = Icon;
        iconImage.enabled = true;
        amountText.text = Amount.ToString();
    }

    public void AddAmount(int add)
    {
        Amount += add;
        amountText.text = Amount.ToString();
    }

    public void ClearSlot()
    {
        ItemRef = null;
        ItemName = "";
        Amount = 0;
        Icon = null;
        HasItem = false;

        iconImage.enabled = false;
        amountText.text = "";
    }

    // ---------------- DRAG & DROP ----------------

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!HasItem) return;

        dragGhost = new GameObject("DragGhost");
        dragGhost.transform.SetParent(draggableLayer, false);
        dragRect = dragGhost.AddComponent<RectTransform>();
        dragRect.sizeDelta = iconImage.rectTransform.sizeDelta;

        var ghostImage = dragGhost.AddComponent<Image>();
        ghostImage.sprite = iconImage.sprite;
        ghostImage.raycastTarget = false;
        ghostImage.color = new Color(1, 1, 1, 0.7f);

        var ghostTextObj = new GameObject("GhostText");
        ghostTextObj.transform.SetParent(dragGhost.transform, false);
        var ghostText = ghostTextObj.AddComponent<TextMeshProUGUI>();
        ghostText.text = amountText.text;
        ghostText.font = amountText.font;
        ghostText.fontSize = amountText.fontSize;
        ghostText.alignment = TextAlignmentOptions.Center;
        ghostText.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragRect == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out Vector2 localPos);

        dragRect.localPosition = localPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragGhost != null)
            Destroy(dragGhost);
    }

    public void OnDrop(PointerEventData eventData)
    {
        var dragged = eventData.pointerDrag?.GetComponent<InventorySlot>();
        if (dragged == null || dragged == this) return;
        if (!dragged.HasItem) return;

        SwapItems(dragged);
        InventoryManager.Instance?.RefreshSlots();
    }

    private void SwapItems(InventorySlot other)
    {
        ItemData tempRef = ItemRef;
        string tempName = ItemName;
        int tempAmount = Amount;
        Sprite tempIcon = Icon;
        bool tempHas = HasItem;

        if (other.HasItem)
        {
            SetItem(other.ItemRef);
            Amount = other.Amount;
            amountText.text = Amount.ToString();
        }
        else
        {
            ClearSlot();
        }

        if (tempHas)
        {
            other.SetItem(tempRef);
            other.Amount = tempAmount;
            other.amountText.text = tempAmount.ToString();
        }
        else
        {
            other.ClearSlot();
        }
    }
}
