using UnityEngine;
using UnityEngine.InputSystem;

public class PlacementController : MonoBehaviour
{
    [SerializeField] GridManager grid;
    [SerializeField] Camera cam;
    [SerializeField] PlacedItem[] palette; // key 1 = first item, key 2 = second, ...

    int selected = 0;
    Direction facing = Direction.Right;

    PlacedItem ghost;
    SpriteRenderer[] ghostRenderers;
    Color[] ghostColors;

    void Start()
    {
        Select(0);
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        Mouse mouse = Mouse.current;
        if (kb == null || mouse == null) return;

        // Pick an item with the number keys.
        if (kb.digit1Key.wasPressedThisFrame) Select(0);
        if (kb.digit2Key.wasPressedThisFrame) Select(1);
        if (kb.digit3Key.wasPressedThisFrame) Select(2);

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

        if (mouse.leftButton.wasPressedThisFrame)
        {
            grid.Place(palette[selected], cell, facing);
        }

        if (mouse.rightButton.wasPressedThisFrame)
        {
            grid.Remove(cell);
        }
    }

    // Switch to a different item and rebuild the ghost.
    void Select(int index)
    {
        if (index >= palette.Length) return;
        selected = index;

        if (ghost != null) Destroy(ghost.gameObject);

        ghost = Instantiate(palette[selected]);
        ghost.enabled = false; // its script must never run

        // Its colliders must never touch Elements.
        foreach (Collider2D col in ghost.GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
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

        Color tint = grid.CanPlace(cell)
            ? new Color(1f, 1f, 1f, 0.5f)      // allowed: see-through
            : new Color(1f, 0.3f, 0.3f, 0.5f); // not allowed: see-through red

        for (int i = 0; i < ghostRenderers.Length; i++)
        {
            ghostRenderers[i].color = ghostColors[i] * tint;
        }
    }
}