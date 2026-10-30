using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Press I (or the Inventory button) to open. Everything you own, with how many; click one and the
// window gets out of the way so you can place it. Opens on the right, where the shop does.
public class InventoryMenu : MonoBehaviour
{
    [SerializeField] PlacementController placement;
    [Tooltip("The window that shows and hides. Must be a child, not this object.")]
    [SerializeField] GameObject panel;
    [Tooltip("Where the item cards are created.")]
    [SerializeField] Transform cardParent;
    [SerializeField] ItemCard cardPrefab;
    [Tooltip("Closed when this opens: they share the right side of the screen.")]
    [SerializeField] ShopMenu shop;
    [Tooltip("Shown instead of the cards when you own nothing.")]
    [SerializeField] GameObject emptyText;

    public bool IsOpen => panel.activeSelf;

    void Start()
    {
        Inventory.Instance.Changed += Rebuild;
        Rebuild();
        panel.SetActive(false); // start closed
    }

    void OnDestroy()
    {
        if (Inventory.Instance != null) Inventory.Instance.Changed -= Rebuild;
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;
        if (kb.iKey.wasPressedThisFrame) Toggle();
        if (kb.escapeKey.wasPressedThisFrame) Close();
    }

    // Open the inventory if it's closed, close it if it's open.
    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void Open()
    {
        shop.Close();
        panel.SetActive(true);
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    // One card per item you own, in palette order.
    void Rebuild()
    {
        foreach (Transform child in cardParent)
        {
            Destroy(child.gameObject);
        }

        List<PlacedItem> items = placement.OwnedItems;
        foreach (PlacedItem item in items)
        {
            ItemCard card = Instantiate(cardPrefab, cardParent);
            card.Set(item);
            card.SetDetail("x" + Inventory.Instance.Count(item));
            card.Button.onClick.AddListener(() => Pick(item));
        }

        if (emptyText != null) emptyText.SetActive(items.Count == 0);
    }

    // Take the item to the board.
    void Pick(PlacedItem item)
    {
        placement.Select(item);
        Close();
    }
}