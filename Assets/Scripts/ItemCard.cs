using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// One item in the shop, the inventory or the Rebirth rewards: its picture, its name, and a line or
// two under the name. Shows the tooltip while the mouse is over it.
public class ItemCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Button button;
    [SerializeField] Image icon;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text detailText;

    PlacedItem item;

    public Button Button => button;
    public PlacedItem Item => item;

    public void Set(PlacedItem item)
    {
        this.item = item;
        icon.sprite = item.Icon;
        icon.enabled = item.Icon != null;
        nameText.text = item.DisplayName;
    }

    public void SetDetail(string text)
    {
        detailText.text = text;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Beside the whole list rather than this card, so it never covers the card next to it.
        // (On a Screen Space - Overlay canvas, world corners are screen pixels.)
        Vector3[] card = new Vector3[4];
        ((RectTransform)transform).GetWorldCorners(card);
        Vector3[] list = card;
        ScrollRect scroll = GetComponentInParent<ScrollRect>();
        if (scroll != null)
        {
            list = new Vector3[4];
            ((RectTransform)scroll.transform).GetWorldCorners(list);
        }

        float y = (card[0].y + card[1].y) / 2f;
        ItemTooltip.Instance.ShowHover(item, new Vector2(list[0].x, y), new Vector2(list[2].x, y));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemTooltip.Instance.HideHover();
    }

    // The window closed (or this card was removed) with the mouse still on it.
    void OnDisable()
    {
        if (ItemTooltip.Instance != null) ItemTooltip.Instance.HideHover();
    }
}