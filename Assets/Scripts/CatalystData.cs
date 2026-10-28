using UnityEngine;

// One Catalyst's settings, stored as a file in the Project panel.
[CreateAssetMenu(menuName = "Elemental/Catalyst")]
public class CatalystData : ScriptableObject
{
    public string displayName = "New Catalyst";
    [Tooltip("Applied in order when this Catalyst works. Empty = changes no value.")]
    public CatalystEffect[] effects;

    [Tooltip("Only works on Elements worth less than this. 0 = no cap.")]
    public double valueCap = 0;

    [Header("Tags")]
    [Tooltip("Stamped on the Element when this Catalyst works. Can be empty.")]
    public ElementTag addsTag;

    [Tooltip("Refuse Elements already carrying Adds Tag this many times. 0 = no limit.")]
    public int tagLimit = 0;

    [Tooltip("Refuse Elements carrying any of these tags.")]
    public ElementTag[] blockedBy;

    [Tooltip("Cleared from the Element when this Catalyst works.")]
    public ElementTag[] removesTags;

    [Tooltip("Clear every tag (the Resetter).")]
    public bool removesAllTags = false;

}