using UnityEngine;

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
    [SerializeField] float interval = 1f; // seconds between drops
    [Tooltip("Where a new Element appears, in pixels from the Condenser's centre (just under the crystal tip).")]
    [SerializeField] Vector2 dropPoint = new Vector2(1f, -16f);

    [Header("Art")]
    [SerializeField] SpriteRenderer upgradeBack;
    [SerializeField] SpriteRenderer upgradeFront;
    [SerializeField] SpriteRenderer glow;

    float timer;
    Animator animator;

    public override int StageCount => stages.Length;
    protected override double CostToReach(int stage) => stages[stage].cost;

    void Awake() {
        animator = GetComponent<Animator>();
    }

    void Start() {
        // Start the float at a random point so a row of Condensers doesn't bob in step.
        animator.Play("Idle", 0, Random.value);
        OnLevelChanged();
    }

    // Swap in this stage's pieces around the crystal.
    protected override void OnLevelChanged() {
        upgradeBack.sprite = stages[Level].back;
        upgradeFront.sprite = stages[Level].front;
        glow.sprite = stages[Level].glow;
    }

    void Update() {
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer -= interval;
            Drop();
        }
    }

    // Create a new Element that drips out of the crystal tip.
    void Drop() {
        // 50 pixels = 1 tile.
        Vector3 spawn = transform.position + (Vector3)(dropPoint / 50f) * grid.CellSize;
        Element e = Instantiate(elementPrefab);
        e.Launch(grid, spawn, stages[Level].elementValue);
        animator.SetTrigger("Drop"); // plays the shimmer
    }

    public override string Describe(int level) =>
        "Drops an Element worth " + Format.Number(stages[level].elementValue) +
        " every " + interval.ToString("0.##") + "s.";
}