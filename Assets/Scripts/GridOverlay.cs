using UnityEngine;
using UnityEngine.InputSystem;

// Faint grid lines around the mouse: clearest right under it, so you can see exactly which tile
// you're pointing at, and fading out further away so the board stays clean. The fading is done by
// the GridReveal shader; this script stretches it over the board and tells it where the mouse is.
public class GridOverlay : MonoBehaviour
{
    [SerializeField] GridManager grid;
    [SerializeField] Camera cam;
    [Tooltip("A white square using the GridReveal material. It gets stretched over the whole board.")]
    [SerializeField] SpriteRenderer lines;

    Material material;

    void Start()
    {
        // Cover the board, plus one pixel so the right and top edges get a line too.
        Vector2 size = new Vector2(grid.Width, grid.Height) * grid.CellSize;
        Vector2 onePixel = Vector2.one * grid.CellSize / 50f;
        lines.transform.position = grid.transform.position + (Vector3)((size + onePixel) / 2f);
        lines.transform.localScale = new Vector3(size.x + onePixel.x, size.y + onePixel.y, 1f);

        material = lines.material; // this renderer's own copy of the material
        material.SetVector("_Origin", grid.transform.position);
        material.SetFloat("_CellSize", grid.CellSize);
    }

    void Update()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        // Mouse pixels -> world position, the same way placing works.
        Vector3 world = cam.ScreenToWorldPoint(mouse.position.ReadValue());
        material.SetVector("_Mouse", world);
    }
}