using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The box that explains an item: what it does, its limits, and its price.
// It sits to the left of what it describes: the selected item on the board, or the card under the
// mouse in the shop and inventory. (If there's no room on the left, it goes on the right.)
public class ItemTooltip : MonoBehaviour
{
    public static ItemTooltip Instance { get; private set; }

    [Tooltip("The box that shows and hides. Must be a child, not this object.")]
    [SerializeField] RectTransform panel;
    [SerializeField] TMP_Text title;
    [SerializeField] TMP_Text body;
    [Tooltip("Space between the box and what it describes (in canvas units).")]
    [SerializeField] float gap = 12f;
    [Tooltip("Keeps the box above the bottom bar (in canvas units).")]
    [SerializeField] float bottomBarHeight = 120f;

    // What to describe, and the screen points just left and right of it.
    struct Request
    {
        public PlacedItem item;
        public bool onBoard;    // a placed item (its stage, its next upgrade), or a card in a list
        public Vector2 left;
        public Vector2 right;
    }

    // Two things ask for the box. The card under the mouse wins over the selected item.
    Request hover;
    Request selected;
    bool hasHover;
    bool hasSelected;

    PlacedItem shownItem;
    int shownLevel = -1;
    bool shownOnBoard;
    Canvas canvas;

    const string Limit = "<color=#E8CE98>";   // parchment, a shade darker than the description
    const string Gold = "<color=#FADE88>";

    void Awake()
    {
        Instance = this;
        canvas = GetComponentInParent<Canvas>();
        panel.gameObject.SetActive(false);
    }

    // A card in the shop, the inventory or the Rebirth rewards is under the mouse.
    public void ShowHover(PlacedItem item, Vector2 left, Vector2 right)
    {
        hover = new Request { item = item, onBoard = false, left = left, right = right };
        hasHover = true;
    }

    public void HideHover()
    {
        hasHover = false;
    }

    // An item on the board is selected (its menu is open).
    public void ShowSelected(PlacedItem item, Vector2 left, Vector2 right)
    {
        selected = new Request { item = item, onBoard = true, left = left, right = right };
        hasSelected = true;
    }

    public void HideSelected()
    {
        hasSelected = false;
    }

    void LateUpdate()
    {
        Request request = hasHover ? hover : selected;
        if ((!hasHover && !hasSelected) || request.item == null)
        {
            panel.gameObject.SetActive(false);
            shownItem = null;
            return;
        }

        panel.gameObject.SetActive(true);
        Refresh(request);
        Place(request);
    }

    // Write the text again only when it would change.
    void Refresh(Request request)
    {
        int level = request.onBoard ? request.item.Level : 0;
        if (request.item == shownItem && level == shownLevel && request.onBoard == shownOnBoard) return;

        shownItem = request.item;
        shownLevel = level;
        shownOnBoard = request.onBoard;

        PlacedItem item = request.item;
        title.text = request.onBoard ? item.StageName : item.DisplayName;

        string text = item.Describe(level);
        string limits = item.Limits(level);
        if (limits != "") text += "\n" + Limit + limits + "</color>";

        text += "\n";
        text += item.Buyable ? "\n" + Gold + "Price " + Format.Price(item.Price) + "</color>" : "\n" + Gold + "Rebirth reward</color>";

        if (!request.onBoard)
        {
            if (item.StageCount > 1) text += "\nUpgrades up to " + item.NameAt(item.StageCount - 1) + ".";
        }
        else if (item.CanUpgrade)
        {
            text += "\n" + Gold + "Upgrade " + Format.Price(item.UpgradeCost) + "</color>";
        }
        else if (item.StageCount > 1)
        {
            text += "\nFully upgraded.";
        }

        body.text = text.Trim('\n');
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
    }

    // Beside what it describes, on screen and above the bottom bar.
    void Place(Request request)
    {
        float scale = canvas.scaleFactor;
        Vector2 size = panel.rect.size * scale;
        float y = Mathf.Clamp(request.left.y, bottomBarHeight * scale + size.y / 2f + 4f, Screen.height - size.y / 2f - 4f);

        if (request.left.x - gap * scale - size.x >= 0f)
        {
            panel.pivot = new Vector2(1f, 0.5f);
            panel.position = new Vector3(request.left.x - gap * scale, y, 0f);
        }
        else
        {
            panel.pivot = new Vector2(0f, 0.5f);
            panel.position = new Vector3(request.right.x + gap * scale, y, 0f);
        }
    }
}