using UnityEngine;

[CreateAssetMenu(menuName = "Elemental/Effects/Multiply")]
public class MultiplyEffect : CatalystEffect
{
    public double factor = 2;

    public override void Apply(Element element)
    {
        element.Value *= factor;
    }
}