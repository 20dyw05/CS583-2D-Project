using UnityEngine;

// Adds a set amount to the Element's value (the Adder).
[CreateAssetMenu(menuName = "Elemental/Effects/Add")]
public class AddEffect : CatalystEffect
{
    public double amount = 1;

    public override void Apply(Element element)
    {
        element.Value += amount;
    }

    public override string Describe()
    {
        return "+" + Num(amount) + " value.";
    }
}