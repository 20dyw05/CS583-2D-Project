using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// One reward in the Rebirth window. It flicks through item pictures, slower and slower, and lands on
// the item it gave. Then the frame's rim takes the colour of its rarity, light and rays bloom behind
// it, and the rarer the item, the brighter it shines and the more sparks fly. Once it has landed, the
// mouse over it shows the item's tooltip.
public class RewardSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Picture")]
    [Tooltip("The frame and everything in it. It bumps as the reel turns and pops when the reward lands.")]
    [SerializeField] RectTransform picture;
    [SerializeField] Image icon;
    [Tooltip("The frame's metal rim, drawn in white so it can take the colour of the rarity.")]
    [SerializeField] Image rim;
    [Tooltip("Light inside the frame, behind the picture.")]
    [SerializeField] Image wellGlow;
    [SerializeField] Image gem;
    [Tooltip("The rarity gems, Common to Legendary.")]
    [SerializeField] Sprite[] gems;

    [Header("Light behind the frame")]
    [Tooltip("Holds the glow and the rays. The window moves it behind all the slots, so rays never cover a neighbour.")]
    [SerializeField] RectTransform lights;
    [SerializeField] Image glow;
    [SerializeField] Image rays;

    [Header("Landing")]
    [SerializeField] Image burst;
    [SerializeField] RectTransform newRibbon;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text rarityText;
    [Tooltip("A little four-point star. Rarer rewards throw out more of them.")]
    [SerializeField] Sprite spark;
    [Tooltip("How many sparks fly out, Common to Legendary.")]
    [SerializeField] int[] sparkCounts = { 0, 4, 8, 14, 24 };

    // A spark in flight.
    struct Spark
    {
        public RectTransform rectTransform;
        public Image image;
        public Vector2 velocity;
        public float age;
        public float life;
        public float size;
    }

    static readonly Color SpinningTint = new Color(0.72f, 0.72f, 0.78f, 1f); // dimmer until it lands

    const float TickTime = 0.12f;     // how long the bump lasts when the reel turns
    const float BurstTime = 0.6f;     // how long the burst takes to grow and fade

    PlacedItem item;
    bool landed;
    float timeSinceLanding;
    float tick;            // 1 just after the reel turns, falling to 0
    Color color = Color.white;
    float strength;        // 0 for Common up to 1 for Legendary
    readonly List<Spark> sparks = new List<Spark>();

    void Awake()
    {
        // Nothing shows but the frame and the reel until the reward lands.
        rim.enabled = false;
        wellGlow.enabled = false;
        glow.enabled = false;
        rays.enabled = false;
        burst.enabled = false;
        gem.enabled = false;
        newRibbon.gameObject.SetActive(false);
        nameText.text = "";
        rarityText.text = "";
    }

    // Move the glow and the rays into a layer behind every slot. They follow the picture from there.
    public void PutLightsIn(Transform layer)
    {
        lights.SetParent(layer, true);
    }

    // Flick through the reel, slowing down, then land on the result and tell the window its rarity.
    public IEnumerator Spin(List<Sprite> reel, PlacedItem result, bool isNew, float duration, System.Action<Rarity> onLanded)
    {
        icon.color = SpinningTint;

        float time = 0f;
        float nextTurn = 0f;
        int index = Random.Range(0, Mathf.Max(1, reel.Count));
        while (time < duration)
        {
            if (time >= nextTurn && reel.Count > 0)
            {
                icon.sprite = reel[index % reel.Count];
                index++;
                tick = 1f;
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayReelTick();
                }

                // The gap between pictures grows from 0.05 to 0.3 seconds as the spin goes on.
                float progress = time / duration;
                nextTurn = time + Mathf.Lerp(0.05f, 0.3f, progress * progress);
            }
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        Rarity rarity = Land(result, isNew);
        if (onLanded != null)
        {
            onLanded(rarity);
        }
    }

    // Show the result: its picture, name, rarity, and everything that celebrates it.
    Rarity Land(PlacedItem result, bool isNew)
    {
        item = result;
        landed = true;
        timeSinceLanding = 0f;
        icon.sprite = result.Icon;
        icon.color = Color.white;

        RebirthTable table = RebirthManager.Instance.Table;
        Rarity rarity = Rarity.Common;
        RebirthTable.Entry entry;
        if (table.TryFind(result, out entry))
        {
            rarity = entry.rarity;
        }
        color = table.ColorOf(rarity);
        strength = (int)rarity / 4f;

        nameText.text = result.DisplayName;
        rarityText.text = rarity.ToString().ToUpper();
        if ((int)rarity < gems.Length)
        {
            gem.sprite = gems[(int)rarity];
            gem.enabled = true;
        }

        rim.enabled = true;
        wellGlow.enabled = true;
        glow.enabled = true;
        rays.enabled = true;
        burst.enabled = true;
        newRibbon.gameObject.SetActive(isNew);

        if ((int)rarity < sparkCounts.Length)
        {
            ThrowSparks(sparkCounts[(int)rarity]);
        }
        Animate(0f);
        return rarity;
    }

    void Update()
    {
        float deltaTime = Time.unscaledDeltaTime; // the game is paused while the window is open

        if (!landed)
        {
            // A small bump each time the reel turns.
            tick = Mathf.MoveTowards(tick, 0f, deltaTime / TickTime);
            picture.localScale = Vector3.one * (1f + 0.06f * tick);
            return;
        }

        timeSinceLanding += deltaTime;
        Animate(timeSinceLanding);
        MoveSparks(deltaTime);
    }

    void LateUpdate()
    {
        // The light sits behind the picture, wherever the window's layout has put it.
        if (lights.parent != transform)
        {
            lights.position = picture.position;
        }
    }

    // Everything that moves after landing, t seconds after it.
    void Animate(float t)
    {
        // The picture pops up and settles with a wobble.
        float pop = 1f + 0.28f * Mathf.Exp(-9f * t) * Mathf.Cos(18f * t);
        picture.localScale = Vector3.one * pop;

        // The rim takes the rarity's colour at once; the light comes up a moment later and breathes.
        rim.color = WithAlpha(color, Mathf.Clamp01(t / 0.15f));

        float light = Mathf.Clamp01(t / 0.3f) * (0.88f + 0.12f * Mathf.Sin(t * 4f));
        glow.color = WithAlpha(color, light * Mathf.Lerp(0.35f, 0.8f, strength));
        wellGlow.color = WithAlpha(color, light * Mathf.Lerp(0.2f, 0.55f, strength));
        rays.color = WithAlpha(color, light * Mathf.Lerp(0.3f, 0.85f, strength));

        float turnSpeed = Mathf.Lerp(10f, 28f, strength); // degrees per second; rarer turns faster
        rays.rectTransform.localEulerAngles = new Vector3(0f, 0f, -t * turnSpeed);

        // The burst grows and fades once; rarer rewards burst bigger.
        float burstProgress = t / BurstTime;
        if (burstProgress < 1f)
        {
            float biggest = Mathf.Lerp(1.6f, 2.6f, strength);
            burst.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.4f, biggest, burstProgress);
            burst.color = WithAlpha(Color.Lerp(color, Color.white, 0.5f), 1f - burstProgress);
        }
        else if (burst.enabled)
        {
            burst.enabled = false;
        }

        // The name fades in...
        float words = Mathf.Clamp01((t - 0.05f) / 0.2f);
        nameText.color = WithAlpha(color, words);
        rarityText.color = WithAlpha(color, words);

        // ...and the NEW! ribbon pops on a beat later, overshooting a little before it settles.
        float sinceRibbon = Mathf.Max(0f, t - 0.15f);
        float ribbonScale = 1f - Mathf.Exp(-12f * sinceRibbon) * Mathf.Cos(16f * sinceRibbon);
        newRibbon.localScale = Vector3.one * ribbonScale;
    }

    // Sparks fly out from the middle of the picture.
    void ThrowSparks(int count)
    {
        Color sparkColor = Color.Lerp(color, Color.white, 0.4f);
        for (int i = 0; i < count; i++)
        {
            GameObject sparkObject = new GameObject("Spark", typeof(RectTransform), typeof(Image));
            RectTransform sparkTransform = (RectTransform)sparkObject.transform;
            sparkTransform.SetParent(transform, false);
            sparkTransform.anchoredPosition = picture.anchoredPosition;

            Image image = sparkObject.GetComponent<Image>();
            image.sprite = spark;
            image.color = sparkColor;
            image.raycastTarget = false;

            float angle = Random.Range(0f, Mathf.PI * 2f);
            float speed = Random.Range(260f, 560f);

            Spark newSpark = new Spark();
            newSpark.rectTransform = sparkTransform;
            newSpark.image = image;
            newSpark.velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
            newSpark.life = Random.Range(0.6f, 1.1f);
            newSpark.size = Random.Range(2, 4) * 9f; // 2 or 3 times the art's 9 pixels, so it stays crisp
            sparks.Add(newSpark);
        }
    }

    void MoveSparks(float deltaTime)
    {
        // Backwards, so finished sparks can be removed from the list as we go.
        for (int i = sparks.Count - 1; i >= 0; i--)
        {
            Spark current = sparks[i];
            current.age += deltaTime;
            if (current.age >= current.life)
            {
                Destroy(current.rectTransform.gameObject);
                sparks.RemoveAt(i);
                continue;
            }

            current.velocity *= Mathf.Exp(-3.5f * deltaTime);   // they slow down...
            current.velocity.y -= 320f * deltaTime;              // ...and drift down as they do
            current.rectTransform.anchoredPosition += current.velocity * deltaTime;

            float lifeUsed = current.age / current.life;
            float twinkle = 0.75f + 0.25f * Mathf.Sin(current.age * 30f + i);
            current.rectTransform.sizeDelta = Vector2.one * current.size * twinkle;
            current.image.color = WithAlpha(current.image.color, 1f - lifeUsed * lifeUsed);

            sparks[i] = current; // Spark is a struct, so the changed copy has to be put back
        }
    }

    static Color WithAlpha(Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!landed)
        {
            return;
        }

        // Corners go bottom-left, top-left, top-right, bottom-right.
        Vector3[] corners = new Vector3[4];
        ((RectTransform)transform).GetWorldCorners(corners);
        float middleY = (corners[0].y + corners[1].y) / 2f;
        ItemTooltip.Instance.ShowHover(item, new Vector2(corners[0].x, middleY), new Vector2(corners[2].x, middleY));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemTooltip.Instance.HideHover();
    }

    void OnDisable()
    {
        if (ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.HideHover();
        }
    }

    // The light lives outside this object once the window has moved it, so it has to be cleaned up too.
    void OnDestroy()
    {
        if (lights != null && lights.parent != transform)
        {
            Destroy(lights.gameObject);
        }
    }
}