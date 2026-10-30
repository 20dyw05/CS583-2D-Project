using UnityEngine;

// A huge multiplier for Elements that have been through a lot of Catalysts (the Celestial Forge).
[CreateAssetMenu(menuName = "Elemental/Effects/Multiply If Many Tags")]
public class MultiplyIfManyTagsEffect : CatalystEffect
{
    [Tooltip("How many different tags the Element needs.")]
    public int minTags = 12;
    public double factor = 25;

    public override void Apply(Element element)
    {
        if (element.TagKinds >= minTags)
        {
            element.Value *= factor;
        }
    }

    public override string Describe()
    {
        return "x" + Num(factor) + " value if it has picked up " + minTags + " or more different upgraders and statuses.";
    }
}