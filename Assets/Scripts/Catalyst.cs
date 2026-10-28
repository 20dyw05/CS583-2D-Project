using UnityEngine;

// A conveyor that also changes the Elements it carries.
public class Catalyst : Conveyor {
    [SerializeField] CatalystData data;

    public override void OnElementEnter(Element element) {

        // If value cap is hit, upgrader shouldn't work
        if(data.valueCap > 0 && element.Value >= data.valueCap) {
            return;
        } 
        
        if (data.tagLimit > 0 && element.TagCount(data.addsTag) >= data.tagLimit) {
            return;
        }
        
        foreach(ElementTag tag in data.blockedBy) {
            if(element.HasTag(tag)) {
                return;
            }
        }

        foreach(CatalystEffect effect in data.effects) {
            effect.Apply(element);
        }

        if(data.removesAllTags) {
            element.ClearTags();
        } else {
            foreach(ElementTag tag in data.removesTags) {
                element.RemoveTag(tag);
            }
        }

        element.AddTag(data.addsTag);

        element.Flash();
    }
}