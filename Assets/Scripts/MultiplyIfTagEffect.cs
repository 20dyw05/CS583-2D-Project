using UnityEngine;

// Multiplies the value, but only if the Element has a certain tag (the Booster doubles Burning ones).
[CreateAssetMenu(menuName = "Elemental/Effects/Multiply If Tag")]
public class MultiplyIfTagEffect : CatalystEffect
{
    public ElementTag requiredTag;
    public double factor = 2;

    public override void Apply(Element element)
    {
        if (element.HasTag(requiredTag))
        {
            element.Value *= factor;
        }
    }

    public override string Describe()
    {
        return "x" + Num(factor) + " value if " + TagName(requiredTag) + ".";
    }
}