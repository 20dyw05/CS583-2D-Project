using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Everything the mouse does on the board:
//   - an empty tile: place the item picked in the inventory (R turns it; Esc or right-click stops)
//   - an item: a white box shows around it; click it for its menu (Upgrade, Move, Stash)
//   - with a menu open, R moves the item and Z stashes it; right-click stashes whatever is under the mouse
public class PlacementController : MonoBehaviour
{
    [SerializeField] GridManager grid;
    [SerializeField] Camera cam;
    [Tooltip("Every item in the game, in the order the shop and inventory list them.")]
    [SerializeField] PlacedItem[] palette;

    [Header("Items on the board")]
    [Tooltip("The white box drawn around the item under the mouse, or the selected one.")]
    [SerializeField] SpriteRenderer box;
    [Tooltip("The window with the Upgrade / Move / Stash buttons.")]
    [SerializeField] ItemMenu menu;
    [Tooltip("The line in the bottom bar that says what you're placing.")]
    [SerializeField] TMP_Text placingText;

    // An item picked up with Move. It is taken off the board and put down again as a fresh copy
    // wherever you click next, so the copy needs to know everything about the original.
    struct CarriedItem
    {
        public PlacedItem prefab;
        public int level;
        public double manaSpent;
        public bool placedFree;
        public Direction facing;
        public Vector2Int from;
    }

    PlacedItem itemToPlace;              // picked in the inventory (a prefab), or null
    Direction facing = Direction.Right;  // which way new items will face

    bool isCarrying;                     // moving an item with Move?
    CarriedItem carried;

    PlacedItem selected;                 // the item on the board whose menu is open

    // The see-through preview that follows the mouse.
    PlacedItem ghost;
    SpriteRenderer[] ghostRenderers;
    Color[] ghostColors;

    public PlacedItem[] Palette
    {
        get { return palette; }
    }

    // The items the inventory lists: those you own at least one of, in palette order.
    // In God Mode that's every item in the game.
    public List<PlacedItem> OwnedItems
    {
        get
        {
            List<PlacedItem> list = new List<PlacedItem>();
            foreach (PlacedItem item in palette)
            {
                if (Inventory.Instance.CanTake(item))
                {
                    list.Add(item);
                }
            }
            return list;
        }
    }

    void Start()
    {
        RebirthManager.Instance.Reborn += OnReborn;
        Inventory.Instance.Changed += OnInventoryChanged;
        if (GodMode.Instance != null)
        {
            GodMode.Instance.Changed += OnInventoryChanged;
        }
        RefreshPlacingText();
    }

    void OnDestroy()
    {
        if (RebirthManager.Instance != null)
        {
            RebirthManager.Instance.Reborn -= OnReborn;
        }
        if (Inventory.Instance != null)
        {
            Inventory.Instance.Changed -= OnInventoryChanged;
        }
        if (GodMode.Instance != null)
        {
            GodMode.Instance.Changed -= OnInventoryChanged;
        }
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (keyboard == null || mouse == null)
        {
            return;
        }

        // Mouse pixels -> world position -> tile.
        Vector2 screen = mouse.position.ReadValue();
        Vector3 world = cam.ScreenToWorldPoint(screen);
        Vector2Int cell = grid.WorldToGrid(world);

        // Clicks on buttons and other UI are not clicks on the board.
        bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        if (isCarrying)
        {
            UpdateMove(keyboard, mouse, cell, overUI);
            return;
        }

        // The selected item is gone (a Rebirth clears the board): close its menu.
        if (menu.IsOpen && selected == null)
        {
            CloseMenu();
        }

        PlacedItem hovered = null;
        if (!overUI)
        {
            hovered = grid.GetItem(cell);
        }

        HandleKeys(keyboard);

        // The box goes around the selected item, or else whatever is under the mouse.
        if (selected != null)
        {
            ShowBox(selected);
        }
        else
        {
            ShowBox(hovered);
        }

        // The ghost only shows over empty tiles, and not while a menu is open.
        if (ghost != null)
        {
            bool showGhost = hovered == null && selected == null && !overUI;
            UpdateGhost(cell, showGhost);
        }

        if (overUI)
        {
            return;
        }

        if (mouse.leftButton.wasPressedThisFrame)
        {
            if (hovered != null)
            {
                OpenMenu(hovered);          // click an item: its menu
            }
            else if (selected != null)
            {
                CloseMenu();                // click the floor: close the menu
            }
            else
            {
                TryPlace(cell);             // otherwise place the picked item
            }
        }

        if (mouse.rightButton.wasPressedThisFrame)
        {
            if (hovered != null)
            {
                StashItem(hovered);
            }
            else if (selected != null)
            {
                CloseMenu();
            }
            else
            {
                StopPlacing();
            }
        }
    }

    // R and Z work on the selected item. With nothing selected, R turns what you're placing.
    void HandleKeys(Keyboard keyboard)
    {
        if (selected != null)
        {
            if (keyboard.rKey.wasPressedThisFrame)
            {
                Move();
            }
            else if (keyboard.zKey.wasPressedThisFrame)
            {
                Stash();
            }
            else if (keyboard.escapeKey.wasPressedThisFrame)
            {
                CloseMenu();
            }
        }
        else if (keyboard.rKey.wasPressedThisFrame && ghost != null)
        {
            facing = facing.Flip();
            ghost.PreviewFacing(facing);
        }
        else if (keyboard.escapeKey.wasPressedThisFrame)
        {
            StopPlacing();
        }
    }

    void TryPlace(Vector2Int cell)
    {
        // Hold on to it: taking the last one out of the inventory clears itemToPlace.
        PlacedItem item = itemToPlace;
        if (item == null || !grid.CanPlace(cell))
        {
            return;
        }

        bool free = GodMode.IsOn;
        if (!Inventory.Instance.TryTake(item))
        {
            return; // none left
        }

        PlacedItem placed = grid.Place(item, cell, facing);
        placed.PlacedFree = free;
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
        if (selected != null)
        {
            selected.TryUpgrade();
        }
    }

    // Menu button (or Z): back into the inventory. The mana paid for upgrades comes back too.
    public void Stash()
    {
        if (selected != null)
        {
            StashItem(selected);
        }
    }

    void StashItem(PlacedItem item)
    {
        if (item == selected)
        {
            CloseMenu();
        }

        ManaManager.Instance.Add(item.ManaSpentOnUpgrades);

        // Something placed for free in God Mode never came out of the inventory.
        if (!item.PlacedFree)
        {
            Inventory.Instance.Add(item.SourcePrefab);
        }
        grid.Remove(item.Cell);
    }

    // Menu button (or R): pick the item up. It follows the mouse until you click an empty tile.
    public void Move()
    {
        if (selected == null)
        {
            return;
        }

        carried.prefab = selected.SourcePrefab;
        carried.level = selected.Level;
        carried.manaSpent = selected.ManaSpentOnUpgrades;
        carried.placedFree = selected.PlacedFree;
        carried.facing = selected.Facing;
        carried.from = selected.Cell;
        isCarrying = true;

        grid.Remove(selected.Cell);
        CloseMenu();
        MakeGhost(carried.prefab, carried.facing, carried.level);
        RefreshPlacingText();
    }

    void UpdateMove(Keyboard keyboard, Mouse mouse, Vector2Int cell, bool overUI)
    {
        box.enabled = false;

        if (keyboard.rKey.wasPressedThisFrame)
        {
            carried.facing = carried.facing.Flip();
            ghost.PreviewFacing(carried.facing);
        }

        // Escape or right-click puts it back where it was.
        bool cancel = keyboard.escapeKey.wasPressedThisFrame || (mouse.rightButton.wasPressedThisFrame && !overUI);
        if (cancel)
        {
            PutDown(carried.from);
            return;
        }

        UpdateGhost(cell, !overUI);

        if (mouse.leftButton.wasPressedThisFrame && !overUI && grid.CanPlace(cell))
        {
            PutDown(cell);
        }
    }

    // Put the carried item down as a fresh copy with the same stage and history.
    void PutDown(Vector2Int cell)
    {
        PlacedItem item = grid.Place(carried.prefab, cell, carried.facing);
        if (item != null)
        {
            item.SetLevel(carried.level);
            item.ManaSpentOnUpgrades = carried.manaSpent;
            item.PlacedFree = carried.placedFree;
        }
        isCarrying = false;

        // Back to whatever was picked in the inventory.
        RestoreGhost();
        RefreshPlacingText();
    }

    // A Rebirth while carrying something: it goes the same way as the rest of the board
    // (back to the inventory if it's a Rebirth reward, gone otherwise).
    void OnReborn()
    {
        if (isCarrying)
        {
            if (carried.prefab.RebirthTier && !carried.placedFree)
            {
                Inventory.Instance.Add(carried.prefab);
            }
            isCarrying = false;
            RestoreGhost();
        }
        CloseMenu();
        RefreshPlacingText();
    }

    void ShowBox(PlacedItem item)
    {
        box.enabled = item != null;
        if (item == null)
        {
            return;
        }

        box.transform.position = grid.GridToWorld(item.Cell);

        // Solid around the selected item, a little softer around the one under the mouse.
        float alpha = 0.75f;
        if (item == selected)
        {
            alpha = 1f;
        }
        box.color = new Color(1f, 1f, 1f, alpha);
    }

    // ---------------- Placing and the ghost ----------------

    // Pick an item to place (the inventory calls this) and rebuild the ghost.
    public void Select(PlacedItem item)
    {
        itemToPlace = item;
        if (!isCarrying)
        {
            MakeGhost(item, facing, 0);
        }
        RefreshPlacingText();
    }

    // Stop placing.
    public void StopPlacing()
    {
        itemToPlace = null;
        if (!isCarrying)
        {
            ClearGhost();
        }
        RefreshPlacingText();
    }

    // The inventory changed (or God Mode was switched): if none of the picked item are left, stop placing.
    void OnInventoryChanged()
    {
        if (itemToPlace != null && !Inventory.Instance.CanTake(itemToPlace))
        {
            StopPlacing();
        }
        else
        {
            RefreshPlacingText();
        }
    }

    void RefreshPlacingText()
    {
        if (placingText == null)
        {
            return;
        }

        if (isCarrying)
        {
            placingText.text = "Moving " + carried.prefab.DisplayName + "\nR: turn   Esc: put back";
        }
        else if (itemToPlace != null)
        {
            string howMany;
            if (GodMode.IsOn)
            {
                howMany = "free";
            }
            else
            {
                howMany = Inventory.Instance.Count(itemToPlace) + " left";
            }
            placingText.text = "Placing " + itemToPlace.DisplayName + " (" + howMany + ")\nR: turn   Esc: stop";
        }
        else
        {
            placingText.text = "";
        }
    }

    // After a move: the ghost goes back to the item picked in the inventory, if there is one.
    void RestoreGhost()
    {
        if (itemToPlace != null)
        {
            MakeGhost(itemToPlace, facing, 0);
        }
        else
        {
            ClearGhost();
        }
    }

    // A copy of the item that does nothing but follow the mouse, see-through.
    void MakeGhost(PlacedItem item, Direction direction, int level)
    {
        ClearGhost();

        ghost = Instantiate(item);
        ghost.enabled = false; // its script must never run

        foreach (Collider2D col in ghost.GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false; // its colliders must never touch Elements
        }

        ghost.PreviewFacing(direction);
        ghost.SetLevel(level);

        // Remember each renderer's own colour, so the tint can be laid on top of it.
        ghostRenderers = ghost.GetComponentsInChildren<SpriteRenderer>();
        ghostColors = new Color[ghostRenderers.Length];
        for (int i = 0; i < ghostRenderers.Length; i++)
        {
            ghostColors[i] = ghostRenderers[i].color;
        }
    }

    void ClearGhost()
    {
        if (ghost != null)
        {
            Destroy(ghost.gameObject);
        }
        ghost = null;
    }

    // Move the ghost to the hovered tile and tint it: see-through white if it can go there, red if not.
    void UpdateGhost(Vector2Int cell, bool show)
    {
        bool visible = show && grid.IsInside(cell);
        ghost.gameObject.SetActive(visible);
        if (!visible)
        {
            return;
        }

        ghost.transform.position = grid.GridToWorld(cell);

        bool haveOne = isCarrying || Inventory.Instance.CanTake(itemToPlace);
        Color tint;
        if (grid.CanPlace(cell) && haveOne)
        {
            tint = new Color(1f, 1f, 1f, 0.5f);
        }
        else
        {
            tint = new Color(1f, 0.3f, 0.3f, 0.5f);
        }

        for (int i = 0; i < ghostRenderers.Length; i++)
        {
            ghostRenderers[i].color = ghostColors[i] * tint;
        }
    }
}