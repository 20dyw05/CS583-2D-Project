using UnityEngine;

// Something a Catalyst does to an Element, like "+1 value" or "x2 value if Burning".
// Each kind of effect is a subclass, and each effect is saved as an asset in Assets/Catalysts/Effects,
// so the same effect can be shared by several Catalysts.
public abstract class CatalystEffect : ScriptableObject
{
    // Change the Element (its value, its tags...).
    public abstract void Apply(Element element);

    // One line for the tooltip, like "+1 value." An empty string means there's nothing worth saying.
    public virtual string Describe()
    {
        return "";
    }

    // Writes a list of tag names for a tooltip: "Burning", "Burning and Frozen",
    // "Burning, Frozen and Charged". lastJoin is the word before the last one (" and " or " or ").
    protected static string NameList(ElementTag[] tags, string lastJoin)
    {
        string text = "";
        for (int i = 0; i < tags.Length; i++)
        {
            bool isFirst = i == 0;
            bool isLast = i == tags.Length - 1;
            if (!isFirst)
            {
                if (isLast)
                {
                    text += lastJoin;
                }
                else
                {
                    text += ", ";
                }
            }
            text += TagName(tags[i]);
        }
        return text;
    }

    // A tag's name for a tooltip, or "?" if the tag slot was left empty.
    protected static string TagName(ElementTag tag)
    {
        if (tag == null)
        {
            return "?";
        }
        return tag.displayName;
    }

    // A number for a tooltip, with at most two decimals: 2, 1.5, 0.25.
    protected static string Num(double value)
    {
        return value.ToString("0.##");
    }
}