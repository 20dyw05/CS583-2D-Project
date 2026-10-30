using System.Collections.Generic;
using UnityEngine;

// A mana cube: the thing Condensers drop, Catalysts upgrade and Transmutators turn into mana
// (Miner's Haven's "ore"). It's a physics object that rides the belts. It carries a value and a set of
// tags that Catalysts read and write, and its newest status tag decides how it looks.
public class Element : MonoBehaviour
{
    // What it's worth. Catalysts change this; a Transmutator pays it out.
    public double Value;

    [Tooltip("A brighter frame shown for a moment when a Catalyst works on this Element.")]
    [SerializeField] Sprite flashSprite;

    // Every Element in play. The value labels read this.
    public static readonly List<Element> All = new List<Element>();

    GridManager grid;
    SpriteRenderer spriteRenderer;
    Color originalColor;
    Sprite normalSprite;
    float flash;    // 1 right after a Catalyst works on it, fading back to 0

    // How many times each tag has been stamped on it, and the order the tags first arrived in.
    readonly Dictionary<ElementTag, int> tags = new Dictionary<ElementTag, int>();
    readonly List<ElementTag> tagOrder = new List<ElementTag>();

    // When it was made. The Chrono Gate pays more for Elements that have been around a while.
    float bornAt;

    // Pulled into a Transmutator: sliding into its pool and shrinking away.
    public bool Sinking { get; private set; }
    Vector3 sinkFrom;
    Vector3 sinkTo;
    float sinkTime;
    const float SinkDuration = 0.15f;

    // Broken by a Catalyst (an Overload Coil, an Event Horizon): it shrinks away where it is, for nothing.
    bool destroyed;

    // An Echo Mirror asked for a copy. It's made at the end of the frame, once the mirror has stamped
    // its tag on this one, so the copy can't be copied again by the same mirror.
    bool copyRequested;

    // How long ago it was made, in seconds.
    public float Age
    {
        get { return Time.time - bornAt; }
    }

    // How many different tags it carries.
    public int TagKinds
    {
        get { return tags.Count; }
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        normalSprite = spriteRenderer.sprite;
        bornAt = Time.time;
    }

    void OnEnable()
    {
        All.Add(this);
    }

    void OnDisable()
    {
        All.Remove(this);
    }

    // Called once when the Element is created.
    public void Launch(GridManager grid, Vector3 position, double value)
    {
        this.grid = grid;
        Value = value;
        transform.position = position;
    }

    // ---------------- Tags ----------------

    // How many times this tag has been stamped on it (0 if never).
    public int TagCount(ElementTag tag)
    {
        if (tag == null)
        {
            return 0;
        }

        int count;
        if (tags.TryGetValue(tag, out count))
        {
            return count;
        }
        return 0;
    }

    public bool HasTag(ElementTag tag)
    {
        return TagCount(tag) > 0;
    }

    public void AddTag(ElementTag tag)
    {
        if (tag == null)
        {
            return; // Catalysts without a tag add nothing
        }

        if (!tags.ContainsKey(tag))
        {
            tagOrder.Add(tag);
        }
        tags[tag] = TagCount(tag) + 1;
    }

    public void RemoveTag(ElementTag tag)
    {
        if (tag == null)
        {
            return;
        }
        tags.Remove(tag);
        tagOrder.Remove(tag);
    }

    // Remove every tag except permanent ones (the Resetter).
    public void ClearTags()
    {
        // Collect them first: a dictionary can't be changed while looping over it.
        List<ElementTag> removable = new List<ElementTag>();
        foreach (ElementTag tag in tags.Keys)
        {
            if (!tag.permanent)
            {
                removable.Add(tag);
            }
        }

        foreach (ElementTag tag in removable)
        {
            RemoveTag(tag);
        }
    }

    // ---------------- What Catalysts and Transmutators do to it ----------------

    // Briefly light up (a Catalyst just worked on it).
    public void Flash()
    {
        flash = 1f;
    }

    // Stop being a physics object and slip into the pool at this point, then disappear.
    public void SinkInto(Vector3 point)
    {
        if (Sinking)
        {
            return;
        }

        Sinking = true;
        sinkFrom = transform.position;
        sinkTo = point;
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().simulated = false;
        Flash();
    }

    // Broken: it shrinks away where it is and pays nothing.
    public void Shatter()
    {
        if (Sinking)
        {
            return;
        }

        destroyed = true;
        SinkInto(transform.position);
    }

    // Ask for a copy of this Element. It appears at the end of the frame.
    public void RequestCopy()
    {
        copyRequested = true;
    }

    public void Dissolve()
    {
        Destroy(gameObject);
    }

    // ---------------- Every frame ----------------

    void Update()
    {
        if (Sinking)
        {
            // Slide into the pool, shrinking, then disappear.
            sinkTime += Time.deltaTime / SinkDuration;
            transform.position = Vector3.Lerp(sinkFrom, sinkTo, sinkTime);
            transform.localScale = Vector3.one * Mathf.Max(0f, 1f - sinkTime);
            if (sinkTime >= 1f)
            {
                Dissolve();
                return;
            }
        }

        // An Element that falls off the board is gone.
        Vector2Int cell = grid.WorldToGrid(transform.position);
        if (!grid.IsInside(cell))
        {
            Dissolve();
            return;
        }

        UpdateLook();
    }

    // The plain cube, or the look (or tint) of its newest status, brightened for a moment after a
    // Catalyst works on it.
    void UpdateLook()
    {
        Color baseColor = originalColor;
        Sprite baseSprite = normalSprite;
        Sprite flashFrame = flashSprite;

        ElementTag lookTag = NewestLookTag();
        if (lookTag != null)
        {
            if (lookTag.look != null)
            {
                baseSprite = lookTag.look;
                flashFrame = lookTag.lookFlash;
            }
            else
            {
                baseColor = lookTag.tint;
            }
        }

        flash = Mathf.MoveTowards(flash, 0f, Time.deltaTime * 4f);
        spriteRenderer.color = Color.Lerp(baseColor, Color.white, flash * 0.6f);
        if (destroyed)
        {
            spriteRenderer.color = new Color(1f, 0.35f, 0.3f, 1f); // a red flash as it breaks
        }

        // The sprite is already full colour, so the flash also swaps in the brighter frame.
        if (flash > 0.5f && flashFrame != null)
        {
            spriteRenderer.sprite = flashFrame;
        }
        else
        {
            spriteRenderer.sprite = baseSprite;
        }
    }

    // The tag that decides how it looks: the newest one with a look of its own or a tint.
    ElementTag NewestLookTag()
    {
        for (int i = tagOrder.Count - 1; i >= 0; i--)
        {
            ElementTag tag = tagOrder[i];
            if (tag.look != null || tag.tintsElement)
            {
                return tag;
            }
        }
        return null;
    }

    void LateUpdate()
    {
        if (!copyRequested)
        {
            return;
        }
        copyRequested = false;

        if (Sinking)
        {
            return;
        }

        // A little above and behind the original, so it drops onto the belt next to it.
        Vector3 position = transform.position + new Vector3(-0.2f, 0.3f, 0f);
        Element copy = Instantiate(this, position, Quaternion.identity);
        copy.CopyFrom(this);
    }

    // Become a copy of another Element: its value, its tags, its age and how it was moving.
    void CopyFrom(Element other)
    {
        grid = other.grid;
        Value = other.Value;
        originalColor = other.originalColor;
        normalSprite = other.normalSprite;
        bornAt = other.bornAt;

        foreach (ElementTag tag in other.tagOrder)
        {
            tags[tag] = other.tags[tag];
            tagOrder.Add(tag);
        }

        Rigidbody2D body = GetComponent<Rigidbody2D>();
        body.linearVelocity = other.GetComponent<Rigidbody2D>().linearVelocity;
        Flash();
    }
}