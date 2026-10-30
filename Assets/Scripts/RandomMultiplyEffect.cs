using UnityEngine;

// A roll of the dice: multiplies by a random amount between Min and Max (the Cyclone).
[CreateAssetMenu(menuName = "Elemental/Effects/Random Multiply")]
public class RandomMultiplyEffect : CatalystEffect
{
    public float min = 1f;
    public float max = 10f;

    public override void Apply(Element element)
    {
        element.Value *= Random.Range(min, max);
    }

    public override string Describe()
    {
        return "x" + Num(min) + " to x" + Num(max) + " value, at random.";
    }
}