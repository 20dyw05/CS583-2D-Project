using UnityEngine;

// A belt that carries Elements left or right. Elements are physics objects: the belt's
// SurfaceEffector2D pushes whatever rests on top of it. Conveyors next to each other join up and
// are drawn as one long belt.
public class Conveyor : PlacedItem
{
    [SerializeField] float speed = 2f;
    [SerializeField] SurfaceEffector2D effector;
    [SerializeField] Transform arrow;

    [Header("Belt")]
    [SerializeField] SpriteRenderer belt;
    [Tooltip("A conveyor on its own, with an end block on both sides.")]
    [SerializeField] Sprite single;
    [Tooltip("The left end of a longer belt (open on the right).")]
    [SerializeField] Sprite leftEnd;
    [Tooltip("Inside a longer belt (open on both sides).")]
    [SerializeField] Sprite middle;
    [Tooltip("The right end of a longer belt (open on the left).")]
    [SerializeField] Sprite rightEnd;

    protected override void OnPlaced()
    {
        // The effector pushes to the right for a positive speed, to the left for a negative one.
        if (Facing == Direction.Right)
        {
            effector.speed = speed;
        }
        else
        {
            effector.speed = -speed;
        }

        // The arrow plate is drawn pointing right; mirror it for belts that run left.
        arrow.GetComponent<SpriteRenderer>().flipX = Facing == Direction.Left;

        RefreshBelt();
    }

    // Something was placed or picked up next to this conveyor.
    public override void OnNeighbourChanged()
    {
        RefreshBelt();
    }

    // Join up with the conveyors either side, so a row of them reads as one long belt.
    void RefreshBelt()
    {
        bool joinsLeft = JoinsWith(Cell + Vector2Int.left);
        bool joinsRight = JoinsWith(Cell + Vector2Int.right);

        if (joinsLeft && joinsRight)
        {
            belt.sprite = middle;
        }
        else if (joinsRight)
        {
            belt.sprite = leftEnd;
        }
        else if (joinsLeft)
        {
            belt.sprite = rightEnd;
        }
        else
        {
            belt.sprite = single;
        }
    }

    // Is there a conveyor on that tile running the same way as this one? A Transmutator counts too:
    // its slab is the same size as a belt, so the belt runs straight into it.
    bool JoinsWith(Vector2Int cell)
    {
        if (grid == null)
        {
            return false; // the placement preview isn't on the grid
        }

        PlacedItem item = grid.GetItem(cell);
        if (item is Transmutator)
        {
            return true;
        }

        Conveyor other = item as Conveyor;
        return other != null && other.Facing == Facing;
    }

    public override string Describe(int level)
    {
        return "Carries Elements along. Press R while placing it to turn it around.";
    }
}