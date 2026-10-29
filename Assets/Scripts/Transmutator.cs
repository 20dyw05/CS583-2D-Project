using UnityEngine;

public class Transmutator : PlacedItem
{
    public override void OnElementEnter(Element element) { 
        ManaManager.Instance.Add(element.Value * RebirthManager.Instance.IncomeMultiplier);        
        element.Dissolve();
    }

}