using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The box that explains an item: what it does, its limits, and its price.
// It sits to the left of what it describes: the selected item on the board, or the card under the
// mouse in the shop, the inventory or the Rebirth window. (If there's no room on the left, it goes on
// the right.)
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

    // Colours for parts of the text.
    const string LimitColor = "<color=#E8CE98>";   // parchment, a shade darker than the description
    const string GoldColor = "<color=#FADE88>";
    const string EndColor = "</color>";

    // One request for the box: what to describe, and the screen points just left and right of it.
    struct Request
    {
        public PlacedItem item;
        public bool onBoard;    // a placed item (its stage, its next upgrade), or a card in a list
        public Vector2 left;
        public Vector2 right;
    }

    // Two things can ask for the box at once. The card under the mouse wins over the selected item.
    Request hover;
    Request selected;
    bool hasHover;
    bool hasSelected;

    // What the box is showing right now, so the text is only rewritten when it changes.
    PlacedItem shownItem;
    int shownLevel = -1;
    bool shownOnBoard;

    Canvas canvas;

    void Awake()
    {
        Instance = this;
        canvas = GetComponentInParent<Canvas>();
        panel.gameObject.SetActive(false);
    }

    // A card in the shop, the inventory or the Rebirth window is under the mouse.
    public void ShowHover(PlacedItem item, Vector2 left, Vector2 right)
    {
        hover = MakeRequest(item, false, left, right);
        hasHover = true;
    }

    public void HideHover()
    {
        hasHover = false;
    }

    // An item on the board is selected (its menu is open).
    public void ShowSelected(PlacedItem item, Vector2 left, Vector2 right)
    {
        selected = MakeRequest(item, true, left, right);
        hasSelected = true;
    }

    public void HideSelected()
    {
        hasSelected = false;
    }

    static Request MakeRequest(PlacedItem item, bool onBoard, Vector2 left, Vector2 right)
    {
        Request request = new Request();
        request.item = item;
        request.onBoard = onBoard;
        request.left = left;
        request.right = right;
        return request;
    }

    void LateUpdate()
    {
        Request request;
        if (hasHover)
        {
            request = hover;
        }
        else
        {
            request = selected;
        }

        bool nothingToShow = (!hasHover && !hasSelected) || request.item == null;
        if (nothingToShow)
        {
            panel.gameObject.SetActive(false);
            shownItem = null;
            return;
        }

        panel.gameObject.SetActive(true);
        Refresh(request);
        Place(request);
    }

    // Write the text, but only when it would change.
    void Refresh(Request request)
    {
        PlacedItem item = request.item;

        // A card describes the item as bought; a placed item describes its current stage.
        int level = 0;
        if (request.onBoard)
        {
            level = item.Level;
        }

        if (item == shownItem && level == shownLevel && request.onBoard == shownOnBoard)
        {
            return;
        }
        shownItem = item;
        shownLevel = level;
        shownOnBoard = request.onBoard;

        if (request.onBoard)
        {
            title.text = item.StageName;
        }
        else
        {
            title.text = item.DisplayName;
        }

        // What it does, then its limits.
        string text = item.Describe(level);
        string limits = item.Limits(level);
        if (limits != "")
        {
            text += "\n" + LimitColor + limits + EndColor;
        }

        // Where it comes from: its shop price, or which Rebirth gives it.
        text += "\n";
        if (item.Buyable)
        {
            text += "\n" + GoldColor + "Price " + Format.Price(item.Price) + EndColor;
        }
        else
        {
            text += "\n" + RewardLine(item);
        }

        // Upgrades: how far it goes (on a card), or the price of the next stage (on the board).
        if (!request.onBoard)
        {
            if (item.StageCount > 1)
            {
                text += "\nUpgrades up to " + item.NameAt(item.StageCount - 1) + ".";
            }
        }
        else if (item.CanUpgrade)
        {
            text += "\n" + GoldColor + "Upgrade " + Format.Price(item.UpgradeCost) + EndColor;
        }
        else if (item.StageCount > 1)
        {
            text += "\nFully upgraded.";
        }

        body.text = text.Trim('\n');
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
    }

    // "Rare Rebirth reward (from Rebirth 5)", with the rarity in its colour.
    string RewardLine(PlacedItem item)
    {
        // Something on the board is a copy of its prefab; the table lists the prefab.
        if (item.SourcePrefab != null)
        {
            item = item.SourcePrefab;
        }

        RebirthTable table = null;
        if (RebirthManager.Instance != null)
        {
            table = RebirthManager.Instance.Table;
        }

        RebirthTable.Entry entry;
        if (table == null || !table.TryFind(item, out entry))
        {
            return GoldColor + "Rebirth reward" + EndColor;
        }

        string rarityColor = "<color=#" + ColorUtility.ToHtmlStringRGB(table.ColorOf(entry.rarity)) + ">";
        return rarityColor + entry.rarity + EndColor + " " + GoldColor + "Rebirth reward (from Rebirth " + entry.fromRebirth + ")" + EndColor;
    }

    // Put the box beside what it describes, on screen and above the bottom bar.
    void Place(Request request)
    {
        float scale = canvas.scaleFactor;
        Vector2 size = panel.rect.size * scale;

        float lowest = bottomBarHeight * scale + size.y / 2f + 4f;
        float highest = Screen.height - size.y / 2f - 4f;
        float y = Mathf.Clamp(request.left.y, lowest, highest);

        bool roomOnLeft = request.left.x - gap * scale - size.x >= 0f;
        if (roomOnLeft)
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