using UnityEngine;

// Sometimes makes a second Element just like this one (the Echo Mirror).
[CreateAssetMenu(menuName = "Elemental/Effects/Copy")]
public class CopyEffect : CatalystEffect
{
    [Range(0f, 1f)] public float chance = 0.3f;

    public override void Apply(Element element)
    {
        if (Random.value < chance)
        {
            element.RequestCopy();
        }
    }

    public override string Describe()
    {
        int percent = Mathf.RoundToInt(chance * 100f);
        return percent + "% chance to copy it. The copy keeps its value and tags.";
    }
}