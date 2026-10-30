using UnityEngine;

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

    public bool RebirthTier => rebirthTier;
    public bool Buyable => buyable;
    public string DisplayName => displayName;
    public double Price => price;
    // The prefab this item was created from. Set by the grid when placed.
    public PlacedItem SourcePrefab { get; set; }

    public Vector2Int Cell { get; private set; }
    public Direction Facing => facing;

    protected GridManager grid;

        // Called once by the grid when the item is placed.
    public void Init(GridManager grid, Vector2Int cell, Direction facing) {
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

    // Each item sets itself up for its direction here. Default: nothing.
    protected virtual void OnPlaced() { }

    // What happens to an Element that touches this item. Default: nothing.
    public virtual void OnElementEnter(Element element) { }

    // Called by the grid when the tile to the left or right of this one changes. Default: nothing.
    public virtual void OnNeighbourChanged() { }

    // ---------------- Upgrades ----------------
    // 0 = as bought. Each upgrade adds one, up to StageCount - 1.
    public int Level { get; private set; }

    // How many stages this item has. Items that can be upgraded override this.
    public virtual int StageCount => 1;

    // Mana to upgrade into this stage. Items that can be upgraded override this.
    protected virtual double CostToReach(int stage) => 0;

    public bool CanUpgrade => Level < StageCount - 1;
    public double UpgradeCost => CanUpgrade ? CostToReach(Level + 1) : 0;

    // All the mana spent upgrading this item. Stashing it gives this back.
    public double SpentOnUpgrades
    {
        get
        {
            double total = 0;
            for (int stage = 1; stage <= Level; stage++)
            {
                total += CostToReach(stage);
            }
            return total;
        }
    }

    // The name with its stage, like "Adder III".
    public string StageName => StageCount > 1 ? displayName + " " + Numeral(Level + 1) : displayName;

    // Spend the mana and move up a stage. Returns whether it worked.
    public bool TryUpgrade()
    {
        if (!CanUpgrade) return false;
        if (!ManaManager.Instance.TrySpend(UpgradeCost)) return false;

        SetLevel(Level + 1);
        return true;
    }

    // Jump straight to a stage (used by Move, so a moved item keeps its upgrades).
    public void SetLevel(int level)
    {
        Level = Mathf.Clamp(level, 0, StageCount - 1);
        OnLevelChanged();
    }

    // Each item that can be upgraded swaps its art and gets stronger here. Default: nothing.
    protected virtual void OnLevelChanged() { }

    static string Numeral(int n)
    {
        string[] numerals = { "I", "II", "III", "IV", "V", "VI", "VII", "VIII" };
        return n >= 1 && n <= numerals.Length ? numerals[n - 1] : n.ToString();
    }

    // Unity calls this when something enters one of this item's trigger colliders.
    protected virtual void OnTriggerEnter2D(Collider2D other) {
        Element element = other.GetComponent<Element>();
        if (element != null) {
            OnElementEnter(element);
        }
    }
   
}