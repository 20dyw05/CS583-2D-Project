using UnityEngine;

// Turns Elements into mana (Miner's Haven's "furnace"). A slab the size of a conveyor with a pool of
// mana on top: whatever touches it, riding in from either side or falling from above, is pulled into
// the pool. Each stage pays out more, and looks grander.
public class Transmutator : PlacedItem
{
    // One stage of the Transmutator: what it costs, how much it pays, and how it looks.
    [System.Serializable]
    public struct Stage
    {
        [Tooltip("Mana to upgrade to this stage. Ignored for the first stage.")]
        public double cost;
        [Tooltip("Mana for each point of an Element's value. 1 = the value as it is.")]
        public double payout;
        [Tooltip("The slab.")]
        public Sprite body;
        [Tooltip("The pool on top, played in a loop.")]
        public Sprite[] portal;
        public Sprite glow;
    }

    [SerializeField] Stage[] stages;

    [Header("Art")]
    [SerializeField] SpriteRenderer body;
    [SerializeField] SpriteRenderer portal;
    [SerializeField] SpriteRenderer glow;
    [SerializeField] float portalFps = 10f;
    [Tooltip("The middle of the pool, in pixels from the tile's centre. Elements are pulled in here.")]
    [SerializeField] Vector2 poolCentre = new Vector2(-4f, -10f);

    [Header("Bonus")]
    [Tooltip("Elements with this tag pay extra (an Ember Kiln pays more for Burning ones). Can be empty.")]
    [SerializeField] ElementTag bonusTag;
    [Tooltip("How many times more a bonus Element pays.")]
    [SerializeField] double bonusMultiplier = 2;

    float flash;        // 1 right after an Element goes in, fading back to 0
    float timeOffset;   // so two Transmutators don't swirl in step

    public override int StageCount
    {
        get { return stages.Length; }
    }

    protected override double CostToReach(int stage)
    {
        return stages[stage].cost;
    }

    void Awake()
    {
        timeOffset = Random.value * 10f;
    }

    void Start()
    {
        OnLevelChanged();
    }

    // Swap to this stage's art. The flash makes an upgrade easy to see.
    protected override void OnLevelChanged()
    {
        body.sprite = stages[Level].body;
        portal.sprite = stages[Level].portal[0];
        glow.sprite = stages[Level].glow;
        flash = 1f;
    }

    void Update()
    {
        // The pool loops through its frames.
        Sprite[] frames = stages[Level].portal;
        int frame = (int)((Time.time + timeOffset) * portalFps) % frames.Length;
        portal.sprite = frames[frame];

        // The glow breathes, and flares when an Element goes in.
        flash = Mathf.MoveTowards(flash, 0f, Time.deltaTime * 3f);
        float breathe = 0.7f + 0.15f * Mathf.Sin((Time.time + timeOffset) * 2f);
        glow.color = new Color(1f, 1f, 1f, Mathf.Clamp01(breathe + flash * 0.4f));
    }

    public override void OnElementEnter(Element element)
    {
        if (element.Sinking)
        {
            return; // already on its way in
        }

        // What it's worth: its value, times this stage's payout (more for bonus Elements),
        // times the Rebirth income bonus.
        double payout = stages[Level].payout;
        if (bonusTag != null && element.HasTag(bonusTag))
        {
            payout *= bonusMultiplier;
        }
        double mana = element.Value * payout * RebirthManager.Instance.IncomeMultiplier;
        ManaManager.Instance.Add(mana);

        // Into the pool it goes, and what it was worth floats up out of it.
        Vector3 pool = transform.position + (Vector3)(poolCentre / 50f) * grid.CellSize;
        element.SinkInto(pool);
        if (ManaPopups.Instance != null)
        {
            ManaPopups.Instance.Show(pool, mana);
        }
        flash = 1f;
    }

    public override string Describe(int level)
    {
        double payout = stages[level].payout;

        string text;
        if (payout == 1)
        {
            text = "Turns Elements into mana.";
        }
        else
        {
            text = "Turns Elements into mana, x" + payout.ToString("0.##") + " their value.";
        }

        if (bonusTag != null)
        {
            double bonusPayout = payout * bonusMultiplier;
            text += "\n" + bonusTag.displayName + " Elements: x" + bonusPayout.ToString("0.##") + ".";
        }
        return text;
    }
}