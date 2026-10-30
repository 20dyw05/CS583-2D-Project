using UnityEngine;

// Multiplies the Element's value (the Doubler, the Multipliers).
[CreateAssetMenu(menuName = "Elemental/Effects/Multiply")]
public class MultiplyEffect : CatalystEffect
{
    public double factor = 2;

    public override void Apply(Element element)
    {
        element.Value *= factor;
    }

    public override string Describe()
    {
        return "x" + Num(factor) + " value.";
    }
}