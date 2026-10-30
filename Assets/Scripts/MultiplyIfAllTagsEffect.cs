using UnityEngine;

// Multiplies the value, but only if the Element has every one of the tags
// (the Thermal Shock wants Elements that are both Burning and Frozen).
[CreateAssetMenu(menuName = "Elemental/Effects/Multiply If All Tags")]
public class MultiplyIfAllTagsEffect : CatalystEffect
{
    public ElementTag[] tags;
    public double factor = 10;

    public override void Apply(Element element)
    {
        // If any one of the tags is missing, do nothing.
        foreach (ElementTag tag in tags)
        {
            if (!element.HasTag(tag))
            {
                return;
            }
        }
        element.Value *= factor;
    }

    public override string Describe()
    {
        return "x" + Num(factor) + " value if " + NameList(tags, " and ") + ".";
    }
}