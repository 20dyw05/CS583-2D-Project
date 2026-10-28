using UnityEngine;

[CreateAssetMenu(menuName = "Elemental/Effects/Multiply If Tag")]
public class MultiplyIfTagEffect : CatalystEffect
{
    public ElementTag requiredTag;

    public double factor = 2;

    public override void Apply(Element element) {
        if(element.HasTag(requiredTag)) {
            element.Value *= factor;
        }
    }
}