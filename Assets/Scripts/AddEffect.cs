using UnityEngine;

[CreateAssetMenu(menuName = "Elemental/Effects/Add")]
public class AddEffect : CatalystEffect
{
    public double amount = 1;

    public override void Apply(Element element) {
        element.Value += amount;
    }

    public override string Describe() => "+" + amount.ToString("0.##") + " value.";
}