using UnityEngine;

// A conveyor that also changes the Elements it carries.
public class Catalyst : Conveyor {
    [SerializeField] CatalystData data;

    public override void OnElementEnter(Element element) {
        if(data.valueCap > 0 && element.Value >= data.valueCap) {
            return;
        } 
        
        if(data.useLimit > 0 && element.UsesOf(data) >= data.useLimit) {
            return;
        }

        if(data.effect == CatalystEffect.Add) {
            element.Value += data.amount;
        } else {
            element.Value *= data.amount;
        }

        element.RecordUse(data);

        element.Flash();
        if(data.tintsElement) {
            element.SetTint(data.tint);
        }
    }
}