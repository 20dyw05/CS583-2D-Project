using UnityEngine;

// The longer the Element has been around, the bigger the multiplier
// (the Chrono Gate rewards long, winding lines).
[CreateAssetMenu(menuName = "Elemental/Effects/Age Multiply")]
public class AgeMultiplyEffect : CatalystEffect
{
    [Tooltip("Added to the multiplier for every second since the Element was made.")]
    public double perSecond = 0.5;
    [Tooltip("The most it can multiply by.")]
    public double max = 6;

    public override void Apply(Element element)
    {
        double multiplier = 1 + perSecond * element.Age;
        if (multiplier > max)
        {
            multiplier = max;
        }
        element.Value *= multiplier;
    }

    public override string Describe()
    {
        return "x1, plus " + Num(perSecond) + " for every second it has existed (up to x" + Num(max) + ").";
    }
}