using UnityEngine;

// Multiplies again for each of the listed tags the Element has
// (the Prism Gate: x1.5 for every status on it).
[CreateAssetMenu(menuName = "Elemental/Effects/Multiply Per Tag")]
public class MultiplyPerTagEffect : CatalystEffect
{
    public ElementTag[] tags;
    public double factorEach = 1.5;

    public override void Apply(Element element)
    {
        foreach (ElementTag tag in tags)
        {
            if (element.HasTag(tag))
            {
                element.Value *= factorEach;
            }
        }
    }

    public override string Describe()
    {
        return "x" + Num(factorEach) + " value for each of these it is: " + NameList(tags, " or ") + ".";
    }
}