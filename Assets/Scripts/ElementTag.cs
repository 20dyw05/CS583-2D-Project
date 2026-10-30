using UnityEngine;

// A label a Catalyst can put on an Element, like "Doubled" or "Burning", saved as an asset
// (Assets/Catalysts/Tags). Catalysts read tags to decide what to do, and some tags change how the
// Element looks.
[CreateAssetMenu(menuName = "Elemental/Tag")]
public class ElementTag : ScriptableObject
{
    public string displayName;

    [Tooltip("Resetters can't remove this tag.")]
    public bool permanent = false;

    [Header("Optional: recolour Elements carrying this tag")]
    public bool tintsElement = false;
    public Color tint = Color.white;

    [Header("Optional: a whole new look for Elements carrying this tag")]
    [Tooltip("Shown instead of the plain mana cube. If an Element has several, the newest one shows.")]
    public Sprite look;
    [Tooltip("The brighter frame of the look, for the moment a Catalyst works on it.")]
    public Sprite lookFlash;
}