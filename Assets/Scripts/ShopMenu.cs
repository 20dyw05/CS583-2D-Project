using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Press P to open. Buy items with mana; they go into the inventory.
public class ShopMenu : MonoBehaviour
{
    [SerializeField] PlacementController placement;
    [Tooltip("The window that shows and hides. Must be a child, not this object.")]
    [SerializeField] GameObject panel;
    [Tooltip("Where the item buttons are created.")]
    [SerializeField] Transform buttonParent;
    [SerializeField] Button buttonPrefab;

    // Each button, and the item it buys, at the same position in both lists.
    readonly List<Button> buttons = new List<Button>();
    readonly List<PlacedItem> buttonItems = new List<PlacedItem>();

    void Start()
    {
        foreach (PlacedItem item in placement.Palette)
        {
            if (!item.Buyable) continue;

            Button button = Instantiate(buttonPrefab, buttonParent);
            button.onClick.AddListener(() => Buy(item));
            buttons.Add(button);
            buttonItems.Add(item);
        }

        Inventory.Instance.Changed += RefreshLabels;
        RefreshLabels();
        panel.SetActive(false); // start closed
    }

    void OnDestroy()
    {
        if (Inventory.Instance != null) Inventory.Instance.Changed -= RefreshLabels;
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.pKey.wasPressedThisFrame) panel.SetActive(!panel.activeSelf);
        if (kb != null && kb.escapeKey.wasPressedThisFrame) panel.SetActive(false);

        if (!panel.activeSelf) return;

        // Grey out anything you can't afford right now.
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].interactable = ManaManager.Instance.Mana >= buttonItems[i].Price;
        }
    }

    void Buy(PlacedItem item)
    {
        if(ManaManager.Instance.TrySpend(item.Price)) {
            Inventory.Instance.Add(item);
        }
    }

    void RefreshLabels()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            PlacedItem item = buttonItems[i];
            buttons[i].GetComponentInChildren<TMP_Text>().text =
                item.DisplayName + "\n" +
                item.Price.ToString("0.##") + " mana\n" +
                "Owned: " + Inventory.Instance.Count(item);
        }
    }
}