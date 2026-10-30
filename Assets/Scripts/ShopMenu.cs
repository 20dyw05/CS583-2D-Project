using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// The shop window. Press P (or the Shop button) to open it. Buy items with mana; they go into the
// inventory. It opens on the right side of the screen, where the inventory does, so it closes the
// inventory when it opens.
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

    public bool IsOpen
    {
        get { return panel.activeSelf; }
    }

    void Start()
    {
        // A card for every item that can be bought, in palette order.
        foreach (PlacedItem item in placement.Palette)
        {
            if (!item.Buyable)
            {
                continue;
            }

            ItemCard card = Instantiate(cardPrefab, buttonParent);
            card.Set(item);
            card.Clicked += Buy;
            cards.Add(card);
        }

        Inventory.Instance.Changed += RefreshLabels;
        if (GodMode.Instance != null)
        {
            GodMode.Instance.Changed += RefreshLabels;
        }
        RefreshLabels();

        panel.SetActive(false); // start closed
    }

    void OnDestroy()
    {
        if (Inventory.Instance != null)
        {
            Inventory.Instance.Changed -= RefreshLabels;
        }
        if (GodMode.Instance != null)
        {
            GodMode.Instance.Changed -= RefreshLabels;
        }
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.pKey.wasPressedThisFrame)
            {
                Toggle();
            }
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                Close();
            }
        }

        if (!IsOpen)
        {
            return;
        }

        // Grey out anything you can't afford right now.
        foreach (ItemCard card in cards)
        {
            card.Button.interactable = ManaManager.Instance.CanAfford(card.Item.Price);
        }
    }

    // Open the shop if it's closed, close it if it's open. (The Shop button calls this.)
    public void Toggle()
    {
        if (IsOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
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

    // (A card you can't afford is greyed out, so its click never gets here. Its CantAffordSound
    // plays the "can't afford" sound instead.)
    void Buy(PlacedItem item)
    {
        bool bought = ManaManager.Instance.TrySpend(item.Price);
        if (bought)
        {
            Inventory.Instance.Add(item);
        }

        if (AudioManager.Instance != null)
        {
            if (bought)
            {
                AudioManager.Instance.PlayBuy();
            }
            else
            {
                AudioManager.Instance.PlayCantAfford();
            }
        }
    }

    // Under each name: the price (or "Free" in God Mode), and how many you already have.
    void RefreshLabels()
    {
        foreach (ItemCard card in cards)
        {
            string price;
            if (GodMode.IsOn)
            {
                price = "Free";
            }
            else
            {
                price = Format.Price(card.Item.Price);
            }
            card.SetDetail(price + "\nOwned: " + Inventory.Instance.Count(card.Item));
        }
    }
}