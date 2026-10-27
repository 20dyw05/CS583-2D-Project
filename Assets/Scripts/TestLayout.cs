using UnityEngine;

public class TestLayout : MonoBehaviour
{
    [SerializeField] GridManager grid;
    [SerializeField] PlacedItem conveyorPrefab;
    [SerializeField] PlacedItem condenserPrefab;
    [SerializeField] PlacedItem transmutatorPrefab;

    void Start()
    {
        // Top floor, running right.
        grid.Place(condenserPrefab, new Vector2Int(2, 4), Direction.Right);
        grid.Place(conveyorPrefab, new Vector2Int(2, 3), Direction.Right);
        grid.Place(conveyorPrefab, new Vector2Int(3, 3), Direction.Right);
        grid.Place(conveyorPrefab, new Vector2Int(4, 3), Direction.Right);
        grid.Place(conveyorPrefab, new Vector2Int(5, 3), Direction.Right);

        // Bottom floor, running left. Elements fall onto it from the end of the top floor.
        grid.Place(conveyorPrefab, new Vector2Int(6, 1), Direction.Left);
        grid.Place(conveyorPrefab, new Vector2Int(5, 1), Direction.Left);
        grid.Place(conveyorPrefab, new Vector2Int(4, 1), Direction.Left);
        grid.Place(transmutatorPrefab, new Vector2Int(3, 1), Direction.Right);
    }
}