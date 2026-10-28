using UnityEngine;
using System.Collections.Generic;

public class Element : MonoBehaviour
{
    public double Value;

    GridManager grid;

    // How many times each Catalyst has been used on this Element.
    readonly Dictionary<CatalystData, int> uses = new Dictionary<CatalystData, int>();

    SpriteRenderer sr;
    Color baseColor;
    float flash;

    void Awake() {
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;
    }


    // Briefly lighten when passing through upgrade
    public void Flash() {
        flash = 1f;
    }

    // Permanent color change
    public void SetTint(Color tint) {
        baseColor = tint;
    }

    public int UsesOf(CatalystData data) =>
        uses.TryGetValue(data, out int count) ? count : 0;

    public void RecordUse(CatalystData data) {
        uses[data] = UsesOf(data) + 1;
    }

    // Colour shows value: pale blue when cheap, gold when valuable.
    public void Refresh() {
        float t = Mathf.Clamp01(Mathf.Log10((float)Value + 1f) / 2f);
        sr.color = Color.Lerp(new Color(0.45f, 0.75f, 1f), new Color(1f, 0.85f, 0.2f), t);
    }

    // Called once when the Element is created.
    public void Launch(GridManager grid, Vector3 position, double value) {
        this.grid = grid;
        Value = value;
        transform.position = position;
    }

    void Update() {
        Vector2Int cell = grid.WorldToGrid(transform.position);

        if(!grid.IsInside(cell)) {
            Dissolve();
        }

        flash = Mathf.MoveTowards(flash, 0f, Time.deltaTime * 4f);
        sr.color = Color.Lerp(baseColor, Color.white, flash * 0.6f);
    }

    public void Dissolve() {
        Destroy(gameObject);
    }
}