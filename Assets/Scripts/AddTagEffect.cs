using UnityEngine;

[CreateAssetMenu(menuName = "Elemental/Effects/Add Tag")]
public class AddTagEffect : CatalystEffect
{
    public ElementTag tag;

    public override void Apply(Element element) {
        element.AddTag(tag);
    }

    public override string Describe() => tag != null ? "Makes it " + tag.displayName + "." : "";
}