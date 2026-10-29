using UnityEngine;

public abstract class PlacedItem : MonoBehaviour
{
    [SerializeField] Direction facing = Direction.Right;

    [Header("Shop")]
    [SerializeField] string displayName = "Item";
    [SerializeField] double price = 10;

    public string DisplayName => displayName;
    public double Price => price;
    // The prefab this item was created from. Set by the grid when placed.
    public PlacedItem SourcePrefab { get; set; }

    public Vector2Int Cell { get; private set; }
    public Direction Facing => facing;

    protected GridManager grid;

        // Called once by the grid when the item is placed.
    public void Init(GridManager grid, Vector2Int cell, Direction facing) {
        this.grid = grid;
        Cell = cell;
        this.facing = facing;
        transform.position = grid.GridToWorld(cell);
        OnPlaced();
    }

    // Used by the placement preview: show a direction without being placed.
    public void PreviewFacing(Direction facing)
    {
        this.facing = facing;
        OnPlaced();
    }

    // Each item sets itself up for its direction here. Default: nothing.
    protected virtual void OnPlaced() { }

    // What happens to an Element that touches this item. Default: nothing.
    public virtual void OnElementEnter(Element element) { }

    // Unity calls this when something enters one of this item's trigger colliders.
    protected virtual void OnTriggerEnter2D(Collider2D other) {
        Element element = other.GetComponent<Element>();
        if (element != null) {
            OnElementEnter(element);
        }
    }
   
}