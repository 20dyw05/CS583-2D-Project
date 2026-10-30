using UnityEngine;

// Puts a tag on the Element, like a status: Burning, Frozen, Charged...
[CreateAssetMenu(menuName = "Elemental/Effects/Add Tag")]
public class AddTagEffect : CatalystEffect
{
    public ElementTag tag;

    public override void Apply(Element element)
    {
        element.AddTag(tag);
    }

    public override string Describe()
    {
        if (tag == null)
        {
            return "";
        }
        return "Makes it " + tag.displayName + ".";
    }
}