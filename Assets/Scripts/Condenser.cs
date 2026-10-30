using UnityEngine;

// Drops a new Element every few seconds (Miner's Haven's "dropper"). The crystal floats and shimmers
// through its Animator; each upgrade stage adds pieces around it and makes its Elements worth more.
public class Condenser : PlacedItem
{
    // One stage of the Condenser: what it costs, what its Elements are worth, and how it looks.
    // The crystal itself is animated and stays the same; each stage adds pieces around it.
    [System.Serializable]
    public struct Stage
    {
        [Tooltip("Mana to upgrade to this stage. Ignored for the first stage.")]
        public double cost;
        [Tooltip("What each Element it drops is worth.")]
        public double elementValue;
        [Tooltip("Pieces behind the crystal. Empty for the first stage.")]
        public Sprite back;
        [Tooltip("Pieces in front of the crystal. Empty for the first stage.")]
        public Sprite front;
        public Sprite glow;
    }

    [SerializeField] Element elementPrefab;
    [SerializeField] Stage[] stages;
    [Tooltip("Seconds between drops.")]
    [SerializeField] float interval = 1f;
    [Tooltip("Where a new Element appears, in pixels from the Condenser's centre (just under the crystal tip).")]
    [SerializeField] Vector2 dropPoint = new Vector2(1f, -16f);
    [Tooltip("Elements it drops start with this tag (an Ember Condenser's are Burning). Can be empty.")]
    [SerializeField] ElementTag dropTag;

    [Header("Art")]
    [SerializeField] SpriteRenderer upgradeBack;
    [SerializeField] SpriteRenderer upgradeFront;
    [SerializeField] SpriteRenderer glow;

    float timer;
    Animator animator;

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
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        // Start the float at a random point so a row of Condensers doesn't bob in step.
        animator.Play("Idle", 0, Random.value);
        OnLevelChanged();
    }

    // Swap in this stage's pieces around the crystal.
    protected override void OnLevelChanged()
    {
        upgradeBack.sprite = stages[Level].back;
        upgradeFront.sprite = stages[Level].front;
        glow.sprite = stages[Level].glow;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer -= interval;
            Drop();
        }
    }

    // Create a new Element that drips out of the crystal tip.
    void Drop()
    {
        // The art is 50 pixels per tile, so pixels / 50 = tiles.
        Vector3 offset = (Vector3)(dropPoint / 50f) * grid.CellSize;
        Vector3 spawn = transform.position + offset;

        Element element = Instantiate(elementPrefab);
        element.Launch(grid, spawn, stages[Level].elementValue);
        element.AddTag(dropTag);

        animator.SetTrigger("Drop"); // plays the shimmer
    }

    public override string Describe(int level)
    {
        string what;
        if (dropTag != null)
        {
            what = "a " + dropTag.displayName + " Element";
        }
        else
        {
            what = "an Element";
        }

        string value = Format.Number(stages[level].elementValue);
        string seconds = interval.ToString("0.##");
        return "Drops " + what + " worth " + value + " every " + seconds + "s.";
    }
}