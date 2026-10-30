using System.Collections.Generic;
using UnityEngine;

// A conveyor that also changes the Elements it carries.
// Each upgrade stage has its own settings and its own arch.
public class Catalyst : Conveyor {

    // One stage of this Catalyst: what it costs, what it does, and how it looks.
    [System.Serializable]
    public struct Stage
    {
        [Tooltip("Mana to upgrade to this stage. Ignored for the first stage.")]
        public double cost;
        [Tooltip("What this Catalyst does at this stage.")]
        public CatalystData data;
        [Tooltip("The arch behind the belt.")]
        public Sprite back;
        [Tooltip("The arch in front of the Element.")]
        public Sprite front;
        [Tooltip("The coloured field the Element rides through.")]
        public Sprite field;
    }

    [SerializeField] Stage[] stages;

    [Header("Arch")]
    [SerializeField] SpriteRenderer archBack;
    [SerializeField] SpriteRenderer archFront;
    [SerializeField] SpriteRenderer field;

    float flash;        // 1 right after it works on an Element, fading back to 0
    float timeOffset;   // so a row of Catalysts doesn't shimmer in step

    public override int StageCount => stages.Length;
    protected override double CostToReach(int stage) => stages[stage].cost;

    void Awake() {
        timeOffset = Random.value * 10f;
    }

    void Start() {
        OnLevelChanged();
    }

    // Swap to this stage's arch. The flash makes an upgrade easy to see.
    protected override void OnLevelChanged() {
        archBack.sprite = stages[Level].back;
        archFront.sprite = stages[Level].front;
        field.sprite = stages[Level].field;
        flash = 1f;
    }

    void Update() {
        // The field shimmers, and lights up when it works on an Element.
        flash = Mathf.MoveTowards(flash, 0f, Time.deltaTime * 4f);
        float shimmer = 0.65f + 0.15f * Mathf.Sin((Time.time + timeOffset) * 3f);
        field.color = new Color(1f, 1f, 1f, Mathf.Clamp01(shimmer + flash * 0.5f));
    }

    public override void OnElementEnter(Element element) {
        CatalystData data = stages[Level].data;

        // If value cap is hit, upgrader shouldn't work
        if(data.valueCap > 0 && element.Value >= data.valueCap) {
            return;
        }

        if (data.tagLimit > 0 && element.TagCount(data.addsTag) >= data.tagLimit) {
            return;
        }

        foreach(ElementTag tag in data.blockedBy) {
            if(element.HasTag(tag)) {
                return;
            }
        }

        foreach(CatalystEffect effect in data.effects) {
            effect.Apply(element);
        }

        if(data.removesAllTags) {
            element.ClearTags();
        } else {
            foreach(ElementTag tag in data.removesTags) {
                element.RemoveTag(tag);
            }
        }

        element.AddTag(data.addsTag);

        element.Flash();
        flash = 1f;
    }

    // For the tooltip: what it does at a stage, one line per effect.
    public override string Describe(int level) {
        CatalystData data = stages[level].data;
        List<string> lines = new List<string>();

        foreach(CatalystEffect effect in data.effects) {
            string line = effect.Describe();
            if(line != "") lines.Add(line);
        }

        if(data.removesAllTags) {
            lines.Add("Clears its tags, except permanent ones.");
        } else {
            foreach(ElementTag tag in data.removesTags) {
                lines.Add("Removes " + tag.displayName + ".");
            }
        }

        return string.Join("\n", lines);
    }

    // For the tooltip: the rules that stop it working.
    public override string Limits(int level) {
        CatalystData data = stages[level].data;
        List<string> lines = new List<string>();

        if(data.valueCap > 0) {
            lines.Add("Only works on Elements worth less than " + Format.Number(data.valueCap) + ".");
        }

        if(data.tagLimit > 0 && data.addsTag != null) {
            lines.Add(data.tagLimit == 1 ? "Works once per Element." : "Works up to " + data.tagLimit + " times per Element.");
        }

        foreach(ElementTag tag in data.blockedBy) {
            lines.Add("Skips " + tag.displayName + " Elements.");
        }

        return string.Join("\n", lines);
    }
}