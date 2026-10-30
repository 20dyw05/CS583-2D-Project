using UnityEngine;
using System.Collections.Generic;

public class Element : MonoBehaviour
{
    public double Value;

    GridManager grid;

    [Tooltip("A brighter frame shown for a moment when a Catalyst works on this Element.")]
    [SerializeField] Sprite flashSprite;

    SpriteRenderer sr;
    Color originalColor;
    Sprite normalSprite;
    float flash;

    void Awake() {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
        normalSprite = sr.sprite;
    }

    // How many times each tag has been stamped on this Element.
    readonly Dictionary<ElementTag, int> tags = new Dictionary<ElementTag, int>();

    public int TagCount(ElementTag tag) =>
        tag != null && tags.TryGetValue(tag, out int count) ? count : 0;

    public bool HasTag(ElementTag tag) => TagCount(tag) > 0;

    public void AddTag(ElementTag tag) {
        if (tag == null) return; // Catalysts without a tag add nothing
        tags[tag] = TagCount(tag) + 1;
    }

    public void RemoveTag(ElementTag tag) {
        if (tag != null) tags.Remove(tag);
    }

    // Remove every tag except permanent ones.
    public void ClearTags()
    {
        List<ElementTag> removable = new List<ElementTag>();
        foreach (ElementTag tag in tags.Keys)
        {
            if (!tag.permanent) removable.Add(tag);
        }

        foreach (ElementTag tag in removable)
        {
            tags.Remove(tag);
        }
    }

    // The colour to show: the tint of any colouring tag, otherwise the original colour.
    Color BaseColor() {
        foreach (ElementTag tag in tags.Keys)
        {
            if (tag.tintsElement) return tag.tint;
        }
        return originalColor;
    }

    // Briefly lighten when passing through upgrade
    public void Flash() {
        flash = 1f;
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
        sr.color = Color.Lerp(BaseColor(), Color.white, flash * 0.6f);

        // The sprite is already full colour, so the flash also swaps in the brighter frame.
        sr.sprite = (flash > 0.5f && flashSprite != null) ? flashSprite : normalSprite;
    }

    public void Dissolve() {
        Destroy(gameObject);
    }
}