using UnityEngine;

public class Condenser : PlacedItem
{
    [SerializeField] Element elementPrefab;
    [SerializeField] double elementValue = 1;
    [SerializeField] float interval = 1f; // seconds between drops

    float timer;

    void Update() {
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer -= interval;
            Drop();
        }
    }

    // Create a new Element on the tile this Condenser faces.
    void Drop() {
        // Just below the Condenser, so the Element falls out of it.
        Vector3 spawn = transform.position + Vector3.down * grid.CellSize * 0.6f;
        Element e = Instantiate(elementPrefab);
        e.Launch(grid, spawn, elementValue);
    }
}