using UnityEngine;

// The more different tags the Element already has, the bigger the multiplier
// (the Elder Root rewards long lines of Catalysts).
[CreateAssetMenu(menuName = "Elemental/Effects/Grow Per Tag")]
public class GrowPerTagEffect : CatalystEffect
{
    [Tooltip("Added to the multiplier for every different tag on the Element.")]
    public double perTag = 0.5;
    [Tooltip("The most it can multiply by.")]
    public double max = 10;

    public override void Apply(Element element)
    {
        double multiplier = 1 + perTag * element.TagKinds;
        if (multiplier > max)
        {
            multiplier = max;
        }
        element.Value *= multiplier;
    }

    public override string Describe()
    {
        return "x1, plus " + Num(perTag) + " for every different tag it has (up to x" + Num(max) + ").";
    }
}