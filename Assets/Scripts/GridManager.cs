using System.Collections.Generic;
using UnityEngine;

// The board: a grid of tiles that items are placed on. It turns tiles into world positions and back,
// keeps track of which item is on which tile, and creates and removes the items.
public class GridManager : MonoBehaviour
{
    [SerializeField] int width = 9;
    [SerializeField] int height = 6;
    [SerializeField] float cellSize = 1f;

    [Tooltip("The bottom-left tile of the area the player can build in.")]
    [SerializeField] Vector2Int unlockedOrigin = new Vector2Int(2, 1);
    [Tooltip("How many tiles across and up the buildable area is.")]
    [SerializeField] Vector2Int unlockedSize = new Vector2Int(5, 4);

    // The item on each occupied tile.
    readonly Dictionary<Vector2Int, PlacedItem> items = new Dictionary<Vector2Int, PlacedItem>();

    public float CellSize { get { return cellSize; } }
    public int Width { get { return width; } }
    public int Height { get { return height; } }

    // Every item currently on the board.
    public IEnumerable<PlacedItem> AllItems
    {
        get { return items.Values; }
    }

    // Tile coordinates -> the world position of that tile's centre.
    public Vector3 GridToWorld(Vector2Int cell)
    {
        Vector3 offset = new Vector3((cell.x + 0.5f) * cellSize, (cell.y + 0.5f) * cellSize, 0f);
        return transform.position + offset;
    }

    // A world position (like where the mouse is) -> the tile it falls in.
    public Vector2Int WorldToGrid(Vector3 world)
    {
        Vector3 local = world - transform.position;
        int x = Mathf.FloorToInt(local.x / cellSize);
        int y = Mathf.FloorToInt(local.y / cellSize);
        return new Vector2Int(x, y);
    }

    // Is the tile on the board at all?
    public bool IsInside(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height;
    }

    // Is the tile in the area the player can build in?
    public bool IsUnlocked(Vector2Int cell)
    {
        bool insideX = cell.x >= unlockedOrigin.x && cell.x < unlockedOrigin.x + unlockedSize.x;
        bool insideY = cell.y >= unlockedOrigin.y && cell.y < unlockedOrigin.y + unlockedSize.y;
        return insideX && insideY;
    }

    // What's on this tile? Returns null if nothing is.
    public PlacedItem GetItem(Vector2Int cell)
    {
        PlacedItem item;
        if (items.TryGetValue(cell, out item))
        {
            return item;
        }
        return null;
    }

    // Can something be placed here? (On the board, unlocked, and empty.)
    public bool CanPlace(Vector2Int cell)
    {
        return IsInside(cell) && IsUnlocked(cell) && !items.ContainsKey(cell);
    }

    // Create an item from a prefab and put it on a tile. Returns the new item, or null if the tile
    // can't take it.
    public PlacedItem Place(PlacedItem prefab, Vector2Int cell, Direction facing)
    {
        if (!CanPlace(cell))
        {
            return null;
        }

        PlacedItem item = Instantiate(prefab, transform);
        item.Init(this, cell, facing);
        item.SourcePrefab = prefab;
        items[cell] = item;
        TellNeighbours(cell);
        return item;
    }

    // Remove whatever is on this tile.
    public void Remove(Vector2Int cell)
    {
        PlacedItem item = GetItem(cell);
        if (item == null)
        {
            return;
        }

        Destroy(item.gameObject);
        items.Remove(cell);
        TellNeighbours(cell);
    }

    // Remove every item from the board (used by Rebirth).
    public void ClearAll()
    {
        foreach (PlacedItem item in items.Values)
        {
            Destroy(item.gameObject);
        }
        items.Clear();
    }

    // Let the items either side of a tile know it changed (conveyors use this to join up).
    void TellNeighbours(Vector2Int cell)
    {
        PlacedItem left = GetItem(cell + Vector2Int.left);
        if (left != null)
        {
            left.OnNeighbourChanged();
        }

        PlacedItem right = GetItem(cell + Vector2Int.right);
        if (right != null)
        {
            right.OnNeighbourChanged();
        }
    }

    // Editor-only drawing in the Scene view: a grey outline for every tile, green for unlocked ones.
    void OnDrawGizmos()
    {
        Vector3 size = new Vector3(cellSize, cellSize, 0f);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                Vector3 centre = GridToWorld(cell);

                if (IsUnlocked(cell))
                {
                    Gizmos.color = new Color(0f, 1f, 0.5f, 0.25f);
                    Gizmos.DrawCube(centre, size * 0.95f);
                }
                Gizmos.color = Color.gray;
                Gizmos.DrawWireCube(centre, size);
            }
        }
    }
}