using UnityEngine;

// Multiplies the value, but only if nothing has touched the Element yet: it has no tags at all
// (the Standing Stones go first in a line).
[CreateAssetMenu(menuName = "Elemental/Effects/Multiply If Untouched")]
public class MultiplyIfUntouchedEffect : CatalystEffect
{
    public double factor = 8;

    public override void Apply(Element element)
    {
        if (element.TagKinds == 0)
        {
            element.Value *= factor;
        }
    }

    public override string Describe()
    {
        return "x" + Num(factor) + " value if it has no tags yet.";
    }
}