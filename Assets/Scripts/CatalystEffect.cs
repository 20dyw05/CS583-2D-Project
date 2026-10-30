using UnityEngine;

// Something a Catalyst does to an Element. Each kind of effect is a subclass.
public abstract class CatalystEffect : ScriptableObject
{
    public abstract void Apply(Element element);

    // One line for the tooltip, like "+1 value." Empty = nothing worth saying.
    public virtual string Describe() => "";
}