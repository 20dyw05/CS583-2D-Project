using UnityEngine;

public class Element : MonoBehaviour
{
    public double Value;

    GridManager grid;

    // Called once when the Element is created.
    public void Launch(GridManager grid, Vector3 position, double value)
    {
        this.grid = grid;
        Value = value;
        transform.position = position;
    }

    void Update()
    {
        // YOU WRITE THIS: if the Element is no longer on the board, Dissolve().
        Vector2Int cell = grid.WorldToGrid(transform.position);

        if(!grid.IsInside(cell)) {
            Dissolve();
        }
    }

    public void Dissolve()
    {
        Destroy(gameObject);
    }
}