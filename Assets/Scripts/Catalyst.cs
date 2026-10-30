using System.Collections.Generic;
using UnityEngine;

// A conveyor that also changes the Elements it carries: an "upgrader". Each upgrade stage has its
// own settings (a CatalystData asset) and its own arch art.
public class Catalyst : Conveyor
{
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
        [Tooltip("Optional: the field as an animation (fire, sparks, snow...), played in a loop. Empty = the still field.")]
        public Sprite[] fieldFrames;
    }

    [SerializeField] Stage[] stages;

    [Header("Arch")]
    [SerializeField] SpriteRenderer archBack;
    [SerializeField] SpriteRenderer archFront;
    [SerializeField] SpriteRenderer field;
    [Tooltip("Frames per second for an animated field.")]
    [SerializeField] float fieldFps = 10f;

    float flash;        // 1 right after it works on an Element, fading back to 0
    float timeOffset;   // so a row of Catalysts doesn't shimmer in step

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

    // Swap to this stage's arch. The flash makes an upgrade easy to see.
    protected override void OnLevelChanged()
    {
        archBack.sprite = stages[Level].back;
        archFront.sprite = stages[Level].front;
        field.sprite = stages[Level].field;
        flash = 1f;
    }

    void Update()
    {
        // An animated field plays its frames in a loop.
        Sprite[] frames = stages[Level].fieldFrames;
        bool animated = frames != null && frames.Length > 0;
        if (animated)
        {
            int frame = (int)((Time.time + timeOffset) * fieldFps) % frames.Length;
            field.sprite = frames[frame];
        }

        // The field shimmers, and lights up when it works on an Element. An animated field's frames
        // already have their own see-through parts, so it stays nearly solid.
        flash = Mathf.MoveTowards(flash, 0f, Time.deltaTime * 4f);
        float wave = Mathf.Sin((Time.time + timeOffset) * 3f);
        float shimmer;
        if (animated)
        {
            shimmer = 0.88f + 0.08f * wave;
        }
        else
        {
            shimmer = 0.65f + 0.15f * wave;
        }
        field.color = new Color(1f, 1f, 1f, Mathf.Clamp01(shimmer + flash * 0.5f));
    }

    public override void OnElementEnter(Element element)
    {
        if (element.Sinking)
        {
            return; // destroyed, or already going into a Transmutator
        }

        CatalystData data = stages[Level].data;
        if (!WillWorkOn(element, data))
        {
            return;
        }

        // Do what this stage does, in order...
        foreach (CatalystEffect effect in data.effects)
        {
            effect.Apply(element);
        }

        // ...take away the tags it removes...
        if (data.removesAllTags)
        {
            element.ClearTags();
        }
        else
        {
            foreach (ElementTag tag in data.removesTags)
            {
                element.RemoveTag(tag);
            }
        }

        // ...and leave its mark, so limits like "once per Element" work.
        element.AddTag(data.addsTag);

        element.Flash();
        flash = 1f;
    }

    // The rules that stop a Catalyst working: a value cap, a limit on how often, and blocking tags.
    bool WillWorkOn(Element element, CatalystData data)
    {
        if (data.valueCap > 0 && element.Value >= data.valueCap)
        {
            return false;
        }

        if (data.tagLimit > 0 && element.TagCount(data.addsTag) >= data.tagLimit)
        {
            return false;
        }

        foreach (ElementTag tag in data.blockedBy)
        {
            if (element.HasTag(tag))
            {
                return false;
            }
        }

        return true;
    }

    // For the tooltip: what it does at a stage, one line per effect.
    public override string Describe(int level)
    {
        CatalystData data = stages[level].data;
        List<string> lines = new List<string>();

        foreach (CatalystEffect effect in data.effects)
        {
            string line = effect.Describe();
            if (line != "")
            {
                lines.Add(line);
            }
        }

        if (data.removesAllTags)
        {
            lines.Add("Clears its tags, except permanent ones.");
        }
        else
        {
            foreach (ElementTag tag in data.removesTags)
            {
                lines.Add("Removes " + tag.displayName + ".");
            }
        }

        return string.Join("\n", lines);
    }

    // For the tooltip: the rules that stop it working.
    public override string Limits(int level)
    {
        CatalystData data = stages[level].data;
        List<string> lines = new List<string>();

        if (data.valueCap > 0)
        {
            lines.Add("Only works on Elements worth less than " + Format.Number(data.valueCap) + ".");
        }

        if (data.tagLimit > 0 && data.addsTag != null)
        {
            if (data.tagLimit == 1)
            {
                lines.Add("Works once per Element.");
            }
            else
            {
                lines.Add("Works up to " + data.tagLimit + " times per Element.");
            }
        }

        foreach (ElementTag tag in data.blockedBy)
        {
            lines.Add("Skips " + tag.displayName + " Elements.");
        }

        return string.Join("\n", lines);
    }
}