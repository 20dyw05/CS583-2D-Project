using UnityEngine;

// A label a Catalyst can put on an Element, like "Doubled" or "Burning".
[CreateAssetMenu(menuName = "Elemental/Tag")]
public class ElementTag : ScriptableObject
{
    public string displayName;

    [Tooltip("Resetters can't remove this tag.")]
    public bool permanent = false;

    [Header("Optional: recolour Elements carrying this tag")]
    public bool tintsElement = false;
    public Color tint = Color.white;
}