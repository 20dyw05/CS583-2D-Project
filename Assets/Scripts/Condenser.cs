using UnityEngine;

public class Condenser : PlacedItem
{
    [SerializeField] Element elementPrefab;
    [SerializeField] double elementValue = 1;
    [SerializeField] float interval = 1f; // seconds between drops
    [Tooltip("Where a new Element appears, in pixels from the Condenser's centre (just under the crystal tip).")]
    [SerializeField] Vector2 dropPoint = new Vector2(1f, -18f);

    float timer;
    Animator animator;

    void Awake() {
        animator = GetComponent<Animator>();
    }

    void Start() {
        // Start the float at a random point so a row of Condensers doesn't bob in step.
        animator.Play("Idle", 0, Random.value);
    }

    void Update() {
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer -= interval;
            Drop();
        }
    }

    // Create a new Element that drips out of the crystal tip.
    void Drop() {
        // 50 pixels = 1 tile.
        Vector3 spawn = transform.position + (Vector3)(dropPoint / 50f) * grid.CellSize;
        Element e = Instantiate(elementPrefab);
        e.Launch(grid, spawn, elementValue);
        animator.SetTrigger("Drop"); // plays the shimmer
    }
}