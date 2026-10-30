using UnityEngine;

[CreateAssetMenu(menuName = "Elemental/Effects/Multiply")]
public class MultiplyEffect : CatalystEffect
{
    public double factor = 2;

    public override void Apply(Element element)
    {
        element.Value *= factor;
    }

    public override string Describe() => "x" + factor.ToString("0.##") + " value.";
}