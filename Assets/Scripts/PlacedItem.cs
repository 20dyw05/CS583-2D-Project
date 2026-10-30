using UnityEngine;

// Anything that can be placed on the board: Conveyors, Catalysts, Condensers and Transmutators.
// It knows its tile and which way it faces, its shop details, and its upgrade stage.
public abstract class PlacedItem : MonoBehaviour
{
    [SerializeField] Direction facing = Direction.Right;

    [Header("Shop")]
    [SerializeField] string displayName = "Item";
    [SerializeField] double price = 10;
    [Tooltip("Can this be bought in the shop? Rebirth rewards can't.")]
    [SerializeField] bool buyable = true;
    [Tooltip("Kept through Rebirth. Rebirth rewards are Rebirth Tier.")]
    [SerializeField] bool rebirthTier = false;
    [Tooltip("Its picture in the shop and the inventory.")]
    [SerializeField] Sprite icon;

    // The board this item is on. Null for the see-through preview that follows the mouse.
    protected GridManager grid;

    public string DisplayName { get { return displayName; } }
    public double Price { get { return price; } }
    public bool Buyable { get { return buyable; } }
    public bool RebirthTier { get { return rebirthTier; } }
    public Sprite Icon { get { return icon; } }
    public Direction Facing { get { return facing; } }

    // The tile it's on.
    public Vector2Int Cell { get; private set; }

    // The prefab this item was made from. The grid sets it when the item is placed.
    public PlacedItem SourcePrefab { get; set; }

    // Placed while God Mode was on, so it didn't come out of the inventory.
    // Stashing it (or a Rebirth) won't put it back there.
    public bool PlacedFree { get; set; }

    // The mana actually paid for this item's upgrades. Stashing it gives this back.
    public double ManaSpentOnUpgrades { get; set; }

    // Called once by the grid when the item is placed.
    public void Init(GridManager grid, Vector2Int cell, Direction facing)
    {
        this.grid = grid;
        Cell = cell;
        this.facing = facing;
        transform.position = grid.GridToWorld(cell);
        OnPlaced();
    }

    // Used by the placement preview: show a direction without being placed.
    public void PreviewFacing(Direction facing)
    {
        this.facing = facing;
        OnPlaced();
    }

    // Each kind of item sets itself up for its direction here. By default there's nothing to do.
    protected virtual void OnPlaced()
    {
    }

    // What happens to an Element that touches this item. By default nothing.
    public virtual void OnElementEnter(Element element)
    {
    }

    // Called by the grid when the tile to the left or right of this one changes. By default nothing.
    public virtual void OnNeighbourChanged()
    {
    }

    // Unity calls this when something enters one of this item's trigger colliders.
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        Element element = other.GetComponent<Element>();
        if (element != null)
        {
            OnElementEnter(element);
        }
    }

    // ---------------- Upgrades ----------------

    // 0 = as bought. Each upgrade adds one, up to StageCount - 1.
    public int Level { get; private set; }

    // How many stages this item has. Items that can be upgraded override this.
    public virtual int StageCount
    {
        get { return 1; }
    }

    // Mana to upgrade into this stage. Items that can be upgraded override this.
    protected virtual double CostToReach(int stage)
    {
        return 0;
    }

    public bool CanUpgrade
    {
        get { return Level < StageCount - 1; }
    }

    // The price of the next stage, or 0 if it's fully upgraded.
    public double UpgradeCost
    {
        get
        {
            if (!CanUpgrade)
            {
                return 0;
            }
            return CostToReach(Level + 1);
        }
    }

    // Spend the mana and move up a stage. Returns whether it worked.
    public bool TryUpgrade()
    {
        if (!CanUpgrade)
        {
            return false;
        }

        double cost = UpgradeCost;
        bool free = GodMode.IsOn;
        if (!ManaManager.Instance.TrySpend(cost))
        {
            return false;
        }

        // Only what was really paid comes back when it's stashed.
        if (!free)
        {
            ManaSpentOnUpgrades += cost;
        }

        SetLevel(Level + 1);
        return true;
    }

    // Jump straight to a stage (used by Move, so a moved item keeps its upgrades).
    public void SetLevel(int level)
    {
        Level = Mathf.Clamp(level, 0, StageCount - 1);
        OnLevelChanged();
    }

    // Each item that can be upgraded swaps its art and gets stronger here. By default nothing.
    protected virtual void OnLevelChanged()
    {
    }

    // ---------------- Names and the tooltip ----------------

    // The name with its stage, like "Adder III".
    public string StageName
    {
        get { return NameAt(Level); }
    }

    // The name of any stage of this item. Items with only one stage just use their name.
    public string NameAt(int level)
    {
        if (StageCount > 1)
        {
            return displayName + " " + Numeral(level + 1);
        }
        return displayName;
    }

    // What this item does at a stage, a line or two. Each kind of item describes itself.
    public virtual string Describe(int level)
    {
        return "";
    }

    // What stops it working, one rule per line. An empty string means no limits.
    public virtual string Limits(int level)
    {
        return "";
    }

    // 1 -> "I", 2 -> "II", ... for stage names.
    static string Numeral(int number)
    {
        string[] numerals = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII" };
        if (number >= 1 && number <= numerals.Length)
        {
            return numerals[number - 1];
        }
        return number.ToString();
    }
}