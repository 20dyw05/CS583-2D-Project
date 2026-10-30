// Which way an item faces. On the side-view board, conveyors only ever run Left or Right.
public enum Direction { Up, Right, Down, Left }

public static class DirectionExtensions
{
    // Right becomes Left and Left becomes Right (R turns an item around while placing it).
    public static Direction Flip(this Direction direction)
    {
        if (direction == Direction.Right)
        {
            return Direction.Left;
        }
        return Direction.Right;
    }
}