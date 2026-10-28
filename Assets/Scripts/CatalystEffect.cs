using UnityEngine;

// Something a Catalyst does to an Element. Each kind of effect is a subclass.
public abstract class CatalystEffect : ScriptableObject
{
    public abstract void Apply(Element element);
}