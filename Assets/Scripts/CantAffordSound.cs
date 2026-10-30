using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Goes on a button that is greyed out while you can't afford what it does: the cards in the shop,
// Upgrade, and Rebirth. A greyed-out button ignores clicks, so this listens for them instead and
// plays the "can't afford" sound.
[RequireComponent(typeof(Button))]
public class CantAffordSound : MonoBehaviour, IPointerClickHandler
{
    Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        // A button that can be clicked does its own thing; only a greyed-out one says no.
        if (!button.interactable && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCantAfford();
        }
    }
}