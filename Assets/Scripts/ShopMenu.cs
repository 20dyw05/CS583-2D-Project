using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Press P (or the Shop button) to open. Buy items with mana; they go into the inventory.
// Opens on the right side of the screen, where the inventory does, so it closes the inventory.
public class ShopMenu : MonoBehaviour
{
    [SerializeField] PlacementController placement;
    [Tooltip("The window that shows and hides. Must be a child, not this object.")]
    [SerializeField] GameObject panel;
    [Tooltip("Where the item cards are created.")]
    [SerializeField] Transform buttonParent;
    [SerializeField] ItemCard cardPrefab;
    [Tooltip("Closed when this opens: they share the right side of the screen.")]
    [SerializeField] InventoryMenu inventory;

    // One card per item for sale.
    readonly List<ItemCard> cards = new List<ItemCard>();

    public bool IsOpen => panel.activeSelf;

    void Start()
    {
        foreach (PlacedItem item in placement.Palette)
        {
            if (!item.Buyable) continue;

            ItemCard card = Instantiate(cardPrefab, buttonParent);
            card.Set(item);
            card.Button.onClick.AddListener(() => Buy(item));
            cards.Add(card);
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
        if (kb != null && kb.pKey.wasPressedThisFrame) Toggle();
        if (kb != null && kb.escapeKey.wasPressedThisFrame) Close();

        if (!IsOpen) return;

        // Grey out anything you can't afford right now.
        foreach (ItemCard card in cards)
        {
            card.Button.interactable = ManaManager.Instance.Mana >= card.Item.Price;
        }
    }

    // Open the shop if it's closed, close it if it's open.
    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void Open()
    {
        inventory.Close();
        panel.SetActive(true);
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    void Buy(PlacedItem item)
    {
        if(ManaManager.Instance.TrySpend(item.Price)) {
            Inventory.Instance.Add(item);
        }
    }

    // The price, and how many you already have.
    void RefreshLabels()
    {
        foreach (ItemCard card in cards)
        {
            card.SetDetail(Format.Price(card.Item.Price) + "\nOwned: " + Inventory.Instance.Count(card.Item));
        }
    }
}