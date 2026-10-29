using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlacementController : MonoBehaviour
{
    [SerializeField] GridManager grid;
    [SerializeField] Camera cam;
    [Tooltip("Every item in the game, in the order the hotbar shows them.")]
    [SerializeField] PlacedItem[] palette;

    PlacedItem selectedItem;
    Direction facing = Direction.Right;

    PlacedItem ghost;
    SpriteRenderer[] ghostRenderers;
    Color[] ghostColors;

    public PlacedItem[] Palette => palette;

    // The items you own at least one of, in palette order.
    public List<PlacedItem> HotbarItems
    {
        get
        {
            List<PlacedItem> list = new List<PlacedItem>();
            foreach (PlacedItem item in palette)
            {
                if (Inventory.Instance.Count(item) > 0) list.Add(item);
            }
            return list;
        }
    }

    void Start()
    {
        SelectSlot(0);
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (kb == null || mouse == null) return;

        // Number keys pick a hotbar slot.
        if (kb.digit1Key.wasPressedThisFrame) SelectSlot(0);
        if (kb.digit2Key.wasPressedThisFrame) SelectSlot(1);
        if (kb.digit3Key.wasPressedThisFrame) SelectSlot(2);
        if (kb.digit4Key.wasPressedThisFrame) SelectSlot(3);
        if (kb.digit5Key.wasPressedThisFrame) SelectSlot(4);
        if (kb.digit6Key.wasPressedThisFrame) SelectSlot(5);

        if (ghost == null) return; // nothing selected yet

        // Flip Left / Right with R.
        if (kb.rKey.wasPressedThisFrame)
        {
            facing = facing.Flip();
            ghost.PreviewFacing(facing);
        }

        // Pixels -> world position -> tile.
        Vector2 screen = mouse.position.ReadValue();
        Vector3 world = cam.ScreenToWorldPoint(screen);
        Vector2Int cell = grid.WorldToGrid(world);

        UpdateGhost(cell);

        // Ignore clicks on buttons and other UI.
        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        if (mouse.leftButton.wasPressedThisFrame && !overUI) TryPlace(cell);
        if (mouse.rightButton.wasPressedThisFrame && !overUI) TryPickUp(cell);
    }

    void TryPlace(Vector2Int cell)
    {
        if (!grid.CanPlace(cell)) return;                          // board says no
        if (!Inventory.Instance.TryTake(selectedItem)) return;     // none left
        grid.Place(selectedItem, cell, facing);
    }

    void TryPickUp(Vector2Int cell)
    {
        PlacedItem item = grid.GetItem(cell);
        if(item == null) {
            return;
        }

        Inventory.Instance.Add(item.SourcePrefab);
        grid.Remove(cell);
    }

    // Select whatever is in this hotbar slot, if anything.
    void SelectSlot(int slot)
    {
        List<PlacedItem> items = HotbarItems;
        if (slot < items.Count) Select(items[slot]);
    }

    // Switch to a different item and rebuild the ghost.
    public void Select(PlacedItem item)
    {
        selectedItem = item;

        if (ghost != null) Destroy(ghost.gameObject);

        ghost = Instantiate(item);
        ghost.enabled = false; // its script must never run

        foreach (Collider2D col in ghost.GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false; // its colliders must never touch Elements
        }

        ghost.PreviewFacing(facing);

        ghostRenderers = ghost.GetComponentsInChildren<SpriteRenderer>();
        ghostColors = new Color[ghostRenderers.Length];
        for (int i = 0; i < ghostRenderers.Length; i++)
        {
            ghostColors[i] = ghostRenderers[i].color;
        }
    }

    // Move the ghost to the hovered tile and tint it.
    void UpdateGhost(Vector2Int cell)
    {
        bool inside = grid.IsInside(cell);
        ghost.gameObject.SetActive(inside);
        if (!inside) return;

        ghost.transform.position = grid.GridToWorld(cell);

        bool owned = Inventory.Instance.Count(selectedItem) > 0;
        Color tint = grid.CanPlace(cell) && owned
            ? new Color(1f, 1f, 1f, 0.5f)      // allowed: see-through
            : new Color(1f, 0.3f, 0.3f, 0.5f); // not allowed: see-through red

        for (int i = 0; i < ghostRenderers.Length; i++)
        {
            ghostRenderers[i].color = ghostColors[i] * tint;
        }
    }
}