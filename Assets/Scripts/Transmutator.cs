using UnityEngine;

// Turns Elements into mana. Each stage pays out more, and looks grander.
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
        public Sprite body;
        [Tooltip("The fire in the window, played in a loop.")]
        public Sprite[] fire;
        public Sprite glow;
    }

    [SerializeField] Stage[] stages;

    [Header("Art")]
    [SerializeField] SpriteRenderer body;
    [SerializeField] SpriteRenderer fire;
    [SerializeField] SpriteRenderer glow;
    [SerializeField] float fireFps = 8f;

    float flash;        // 1 right after an Element goes in, fading back to 0
    float timeOffset;   // so two Transmutators don't flicker in step

    public override int StageCount => stages.Length;
    protected override double CostToReach(int stage) => stages[stage].cost;

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
        fire.sprite = stages[Level].fire[0];
        glow.sprite = stages[Level].glow;
        flash = 1f;
    }

    void Update()
    {
        // The fire loops through its frames.
        Sprite[] frames = stages[Level].fire;
        int frame = (int)((Time.time + timeOffset) * fireFps) % frames.Length;
        fire.sprite = frames[frame];

        // The glow breathes, and flares when an Element goes in.
        flash = Mathf.MoveTowards(flash, 0f, Time.deltaTime * 3f);
        float breathe = 0.7f + 0.15f * Mathf.Sin((Time.time + timeOffset) * 2f);
        glow.color = new Color(1f, 1f, 1f, Mathf.Clamp01(breathe + flash * 0.4f));
    }

    public override void OnElementEnter(Element element) {
        double payout = stages[Level].payout;
        ManaManager.Instance.Add(element.Value * payout * RebirthManager.Instance.IncomeMultiplier);
        element.Dissolve();
        flash = 1f;
    }

}