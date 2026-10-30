using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Everything the mouse does on the board:
//   - empty tile: place the hotbar item (R flips it)
//   - an item: a white box shows around it; click it for its menu (Upgrade, Move, Stash)
//   - R moves the selected item, Z stashes it, right-click stashes whatever is under the mouse
public class PlacementController : MonoBehaviour
{
    [SerializeField] GridManager grid;
    [SerializeField] Camera cam;
    [Tooltip("Every item in the game, in the order the hotbar shows them.")]
    [SerializeField] PlacedItem[] palette;

    [Header("Items on the board")]
    [Tooltip("The white box drawn around the item under the mouse, or the selected one.")]
    [SerializeField] SpriteRenderer box;
    [Tooltip("The window with the Upgrade / Move / Stash buttons.")]
    [SerializeField] ItemMenu menu;

    PlacedItem selectedItem;   // the hotbar's pick (a prefab)
    Direction facing = Direction.Right;

    PlacedItem ghost;
    SpriteRenderer[] ghostRenderers;
    Color[] ghostColors;

    PlacedItem selected;       // the item on the board whose menu is open

    // An item picked up with Move. It goes wherever you click next, keeping its upgrades.
    PlacedItem carried;        // its prefab, or null when not moving anything
    int carriedLevel;
    Direction carriedFacing;
    Vector2Int carriedFrom;

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
        RebirthManager.Instance.Reborn += OnReborn;
    }

    void OnDestroy()
    {
        if (RebirthManager.Instance != null) RebirthManager.Instance.Reborn -= OnReborn;
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

        // Pixels -> world position -> tile.
        Vector2 screen = mouse.position.ReadValue();
        Vector3 world = cam.ScreenToWorldPoint(screen);
        Vector2Int cell = grid.WorldToGrid(world);

        // Ignore clicks on buttons and other UI.
        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        if (carried != null)
        {
            UpdateMove(kb, mouse, cell, overUI);
            return;
        }

        // The selected item is gone (a Rebirth clears the board): close its menu.
        if (menu.IsOpen && selected == null) CloseMenu();

        PlacedItem hovered = overUI ? null : grid.GetItem(cell);

        // R and Z work on the selected item. With nothing selected, R flips what you're placing.
        if (selected != null)
        {
            if (kb.rKey.wasPressedThisFrame) { Move(); return; }
            if (kb.zKey.wasPressedThisFrame) { Stash(); return; }
            if (kb.escapeKey.wasPressedThisFrame) CloseMenu();
        }
        else if (kb.rKey.wasPressedThisFrame && ghost != null)
        {
            facing = facing.Flip();
            ghost.PreviewFacing(facing);
        }

        // The box goes around the selected item, or else whatever is under the mouse.
        ShowBox(selected != null ? selected : hovered);

        // The ghost only shows over empty tiles, and not while a menu is open.
        if (ghost != null) UpdateGhost(cell, hovered == null && selected == null && !overUI);

        if (overUI) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            if (hovered != null) OpenMenu(hovered);   // click an item: its menu
            else if (selected != null) CloseMenu();   // click the floor: close the menu
            else TryPlace(cell);                      // otherwise place the hotbar item
        }

        if (mouse.rightButton.wasPressedThisFrame)
        {
            if (hovered != null) StashItem(hovered);
            else CloseMenu();
        }
    }

    void TryPlace(Vector2Int cell)
    {
        if (!grid.CanPlace(cell)) return;                          // board says no
        if (!Inventory.Instance.TryTake(selectedItem)) return;     // none left
        grid.Place(selectedItem, cell, facing);
    }

    // ---------------- The item menu ----------------

    void OpenMenu(PlacedItem item)
    {
        selected = item;
        menu.Show(item);
    }

    void CloseMenu()
    {
        selected = null;
        menu.Hide();
    }

    // Menu button: spend mana for the next stage.
    public void Upgrade()
    {
        if (selected != null) selected.TryUpgrade();
    }

    // Menu button (or Z): back into the inventory. Upgrades are refunded, so nothing is lost.
    public void Stash()
    {
        if (selected != null) StashItem(selected);
    }

    void StashItem(PlacedItem item)
    {
        if (item == selected) CloseMenu();
        ManaManager.Instance.Add(item.SpentOnUpgrades);
        Inventory.Instance.Add(item.SourcePrefab);
        grid.Remove(item.Cell);
    }

    // Menu button (or R): pick the item up. It follows the mouse until you click an empty tile.
    public void Move()
    {
        if (selected == null) return;

        carried = selected.SourcePrefab;
        carriedLevel = selected.Level;
        carriedFacing = selected.Facing;
        carriedFrom = selected.Cell;

        grid.Remove(selected.Cell);
        CloseMenu();
        MakeGhost(carried, carriedFacing, carriedLevel);
    }

    void UpdateMove(Keyboard kb, Mouse mouse, Vector2Int cell, bool overUI)
    {
        box.enabled = false;

        if (kb.rKey.wasPressedThisFrame)
        {
            carriedFacing = carriedFacing.Flip();
            ghost.PreviewFacing(carriedFacing);
        }

        // Escape or right-click puts it back where it was.
        if (kb.escapeKey.wasPressedThisFrame || (mouse.rightButton.wasPressedThisFrame && !overUI))
        {
            PutDown(carriedFrom);
            return;
        }

        UpdateGhost(cell, !overUI);

        if (mouse.leftButton.wasPressedThisFrame && !overUI && grid.CanPlace(cell))
        {
            PutDown(cell);
        }
    }

    void PutDown(Vector2Int cell)
    {
        PlacedItem item = grid.Place(carried, cell, carriedFacing);
        if (item != null) item.SetLevel(carriedLevel);
        carried = null;

        // Back to the hotbar item.
        if (selectedItem != null) MakeGhost(selectedItem, facing, 0);
        else ClearGhost();
    }

    // A Rebirth while carrying something: it goes the same way as the rest of the board.
    void OnReborn()
    {
        if (carried != null)
        {
            if (carried.RebirthTier) Inventory.Instance.Add(carried);
            carried = null;
            if (selectedItem != null) MakeGhost(selectedItem, facing, 0);
            else ClearGhost();
        }
        CloseMenu();
    }

    void ShowBox(PlacedItem item)
    {
        box.enabled = item != null;
        if (item == null) return;

        box.transform.position = grid.GridToWorld(item.Cell);
        // Solid on the selected item, a little softer on the one under the mouse.
        box.color = new Color(1f, 1f, 1f, item == selected ? 1f : 0.75f);
    }

    // ---------------- The hotbar and the ghost ----------------

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
        if (carried == null) MakeGhost(item, facing, 0);
    }

    void MakeGhost(PlacedItem item, Direction dir, int level)
    {
        ClearGhost();

        ghost = Instantiate(item);
        ghost.enabled = false; // its script must never run

        foreach (Collider2D col in ghost.GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false; // its colliders must never touch Elements
        }

        ghost.PreviewFacing(dir);
        ghost.SetLevel(level);

        ghostRenderers = ghost.GetComponentsInChildren<SpriteRenderer>();
        ghostColors = new Color[ghostRenderers.Length];
        for (int i = 0; i < ghostRenderers.Length; i++)
        {
            ghostColors[i] = ghostRenderers[i].color;
        }
    }

    void ClearGhost()
    {
        if (ghost != null) Destroy(ghost.gameObject);
        ghost = null;
    }

    // Move the ghost to the hovered tile and tint it.
    void UpdateGhost(Vector2Int cell, bool show)
    {
        bool inside = show && grid.IsInside(cell);
        ghost.gameObject.SetActive(inside);
        if (!inside) return;

        ghost.transform.position = grid.GridToWorld(cell);

        bool owned = carried != null || Inventory.Instance.Count(selectedItem) > 0;
        Color tint = grid.CanPlace(cell) && owned
            ? new Color(1f, 1f, 1f, 0.5f)      // allowed: see-through
            : new Color(1f, 0.3f, 0.3f, 0.5f); // not allowed: see-through red

        for (int i = 0; i < ghostRenderers.Length; i++)
        {
            ghostRenderers[i].color = ghostColors[i] * tint;
        }
    }
}