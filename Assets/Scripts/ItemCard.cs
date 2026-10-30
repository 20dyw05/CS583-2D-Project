using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// One item in the shop or the inventory: its picture, its name, and a line or two under the name.
// Shows the item's tooltip while the mouse is over it, and tells its menu when it's clicked.
public class ItemCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Button button;
    [SerializeField] Image icon;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text detailText;

    PlacedItem item;
    Color normalNameColor;
    bool normalNameColorKnown;

    // Fired when the card is clicked, with the item it shows. The shop buys it, the inventory picks it.
    public event System.Action<PlacedItem> Clicked;

    public Button Button
    {
        get { return button; }
    }

    public PlacedItem Item
    {
        get { return item; }
    }

    void Awake()
    {
        button.onClick.AddListener(OnButtonClicked);
    }

    void OnButtonClicked()
    {
        if (Clicked != null)
        {
            Clicked(item);
        }
    }

    // Show this item on the card.
    public void Set(PlacedItem item)
    {
        this.item = item;
        icon.sprite = item.Icon;
        icon.enabled = item.Icon != null;
        nameText.text = item.DisplayName;

        // Rebirth rewards show their name in the colour of their rarity; everything else keeps the
        // colour the name has in the prefab.
        if (!normalNameColorKnown)
        {
            normalNameColor = nameText.color;
            normalNameColorKnown = true;
        }
        nameText.color = normalNameColor;

        RebirthTable table = null;
        if (RebirthManager.Instance != null)
        {
            table = RebirthManager.Instance.Table;
        }

        RebirthTable.Entry entry;
        if (table != null && table.TryFind(item, out entry))
        {
            nameText.color = table.ColorOf(entry.rarity);
        }
    }

    // The line under the name: the price in the shop, how many you have in the inventory.
    public void SetDetail(string text)
    {
        detailText.text = text;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Beside the whole list rather than this card, so it never covers the card next to it.
        // (On a Screen Space - Overlay canvas, world corners are screen pixels.)
        Vector3[] cardCorners = new Vector3[4];
        ((RectTransform)transform).GetWorldCorners(cardCorners);

        Vector3[] listCorners = cardCorners;
        ScrollRect scroll = GetComponentInParent<ScrollRect>();
        if (scroll != null)
        {
            listCorners = new Vector3[4];
            ((RectTransform)scroll.transform).GetWorldCorners(listCorners);
        }

        // Corners go bottom-left, top-left, top-right, bottom-right.
        float middleY = (cardCorners[0].y + cardCorners[1].y) / 2f;
        Vector2 leftEdge = new Vector2(listCorners[0].x, middleY);
        Vector2 rightEdge = new Vector2(listCorners[2].x, middleY);
        ItemTooltip.Instance.ShowHover(item, leftEdge, rightEdge);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemTooltip.Instance.HideHover();
    }

    // The window closed (or this card was removed) with the mouse still on it.
    void OnDisable()
    {
        if (ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.HideHover();
        }
    }
}