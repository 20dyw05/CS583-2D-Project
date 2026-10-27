using UnityEngine;

public enum Direction { Up, Right, Down, Left }

public static class DirectionExtensions
{
    // Direction -> one-tile step on the grid.
    public static Vector2Int ToVector(this Direction d) => d switch
    {
        Direction.Up => Vector2Int.up,
        Direction.Right => Vector2Int.right,
        Direction.Down => Vector2Int.down,
        _ => Vector2Int.left,
    };

    // Direction -> rotation angle, for art that points right by default.
    public static float ToAngle(this Direction d) => d switch
    {
        Direction.Up => 90f,
        Direction.Right => 0f,
        Direction.Down => -90f,
        _ => 180f,
    };

    // Right <-> Left, for side-view items.
    public static Direction Flip(this Direction d) {
        return d == Direction.Right ? Direction.Left : Direction.Right;
    }
}