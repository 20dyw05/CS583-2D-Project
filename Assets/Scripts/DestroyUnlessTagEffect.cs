using UnityEngine;

// Destroys any Element that doesn't have the tag (the Event Horizon only spares Voided ones).
[CreateAssetMenu(menuName = "Elemental/Effects/Destroy Unless Tag")]
public class DestroyUnlessTagEffect : CatalystEffect
{
    public ElementTag requiredTag;

    public override void Apply(Element element)
    {
        if (!element.HasTag(requiredTag))
        {
            element.Shatter();
        }
    }

    public override string Describe()
    {
        return "Destroys it unless it's " + TagName(requiredTag) + ".";
    }
}