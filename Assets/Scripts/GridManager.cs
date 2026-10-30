using UnityEngine;

using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [SerializeField] int width = 9;
    [SerializeField] int height = 6;
    [SerializeField] float cellSize = 1f;

    [SerializeField] Vector2Int unlockedOrigin = new Vector2Int(2, 1);
    [SerializeField] Vector2Int unlockedSize = new Vector2Int(5, 4);
    readonly Dictionary<Vector2Int, PlacedItem> items = new Dictionary<Vector2Int, PlacedItem>();

    public float CellSize => cellSize;
    public int Width => width;
    public int Height => height;
    // Every item currently on the board.
    public IEnumerable<PlacedItem> AllItems => items.Values;

    // Tile coordinates -> the world position of that tile's center.
    public Vector3 GridToWorld(Vector2Int cell)
    {
        Vector3 offset = new Vector3((cell.x + 0.5f) * cellSize, (cell.y + 0.5f) * cellSize, 0f);
        return transform.position + offset;
    }

    // A world position (like a mouse click) -> the tile it falls in.
    public Vector2Int WorldToGrid(Vector3 world)
    {
        Vector3 local = world - transform.position;
        return new Vector2Int(Mathf.FloorToInt(local.x / cellSize), Mathf.FloorToInt(local.y / cellSize));
    }

    public bool IsInside(Vector2Int cell) =>
        cell.x >= 0 && cell.x < width && cell.y >= 0 && cell.y < height;

    public bool IsUnlocked(Vector2Int cell) =>
        cell.x >= unlockedOrigin.x && cell.x < unlockedOrigin.x + unlockedSize.x &&
        cell.y >= unlockedOrigin.y && cell.y < unlockedOrigin.y + unlockedSize.y;

    // What's on this tile? Returns null if nothing is.
    public PlacedItem GetItem(Vector2Int cell) =>
        items.TryGetValue(cell, out PlacedItem item) ? item : null;

    // Can something be placed here?
    public bool CanPlace(Vector2Int cell)
    {
        return (IsInside(cell) && IsUnlocked(cell) && !(items.ContainsKey(cell)));
    }

    // Create an item from a prefab and put it on a tile.
    public PlacedItem Place(PlacedItem prefab, Vector2Int cell, Direction facing)
    {
        if (!CanPlace(cell)) return null;

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
        PlacedItem item = GetItem(cell);   // 1. find the item...
        if (item == null) return;          //    ...and stop if there isn't one
        Destroy(item.gameObject);          // 2. destroy its GameObject
        items.Remove(cell);                // 3. forget the tile
        TellNeighbours(cell);              // 4. let the tiles either side know
    }

    // Remove every placed item from the board.
    public void ClearAll()
    {
        foreach(PlacedItem item in items.Values) {
            Destroy(item.gameObject);
        } 

        items.Clear();
    }

    // Let the items either side of a tile know it changed (conveyors use this to join up).
    void TellNeighbours(Vector2Int cell)
    {
        PlacedItem left = GetItem(cell + Vector2Int.left);
        if (left != null) left.OnNeighbourChanged();

        PlacedItem right = GetItem(cell + Vector2Int.right);
        if (right != null) right.OnNeighbourChanged();
    }

    // Editor-only drawing: gray outline for every tile, green fill for unlocked ones.
    void OnDrawGizmos()
    {
        Vector3 size = new Vector3(cellSize, cellSize, 0f);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                Vector3 center = GridToWorld(cell);

                if (IsUnlocked(cell))
                {
                    Gizmos.color = new Color(0f, 1f, 0.5f, 0.25f);
                    Gizmos.DrawCube(center, size * 0.95f);
                }
                Gizmos.color = Color.gray;
                Gizmos.DrawWireCube(center, size);
            }
        }
    }
}