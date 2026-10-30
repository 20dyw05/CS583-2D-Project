using UnityEngine;

// A big multiplier that sometimes destroys the Element instead (the Overload Coil).
[CreateAssetMenu(menuName = "Elemental/Effects/Risky Multiply")]
public class RiskyMultiplyEffect : CatalystEffect
{
    public double factor = 12;
    [Range(0f, 1f)] public float breakChance = 0.3f;
    [Tooltip("Elements with this tag never break. Can be empty.")]
    public ElementTag safeTag;

    public override void Apply(Element element)
    {
        bool isSafe = safeTag != null && element.HasTag(safeTag);
        bool breaks = !isSafe && Random.value < breakChance;

        if (breaks)
        {
            element.Shatter();
        }
        else
        {
            element.Value *= factor;
        }
    }

    public override string Describe()
    {
        int percent = Mathf.RoundToInt(breakChance * 100f);
        string text = "x" + Num(factor) + " value, but a " + percent + "% chance to destroy it";
        if (safeTag != null)
        {
            text += " (" + safeTag.displayName + " ones are safe)";
        }
        return text + ".";
    }
}