using UnityEngine;

// Multiplies every Element, and Elements with a certain tag by more
// (the Glacier Heart: x1.5, or x6 if the Element is Frozen).
[CreateAssetMenu(menuName = "Elemental/Effects/Multiply More If Tag")]
public class MultiplyMoreIfTagEffect : CatalystEffect
{
    public double factor = 1.5;
    public ElementTag tag;
    [Tooltip("Used instead of Factor when the Element has the tag.")]
    public double tagFactor = 6;

    public override void Apply(Element element)
    {
        if (element.HasTag(tag))
        {
            element.Value *= tagFactor;
        }
        else
        {
            element.Value *= factor;
        }
    }

    public override string Describe()
    {
        return "x" + Num(factor) + " value, or x" + Num(tagFactor) + " if " + TagName(tag) + ".";
    }
}