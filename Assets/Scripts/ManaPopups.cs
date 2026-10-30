using System.Collections.Generic;
using TMPro;
using UnityEngine;

// The "+12" that floats up out of a Transmutator and fades, each time it turns an Element into mana.
public class ManaPopups : MonoBehaviour
{
    public static ManaPopups Instance { get; private set; }

    [SerializeField] Camera cam;
    [Tooltip("Needs a CanvasGroup, so the mana symbol fades with the number.")]
    [SerializeField] TMP_Text popupPrefab;
    [SerializeField] float lifetime = 1f;
    [Tooltip("How far a popup rises before it's gone (in canvas units).")]
    [SerializeField] float rise = 60f;

    class Popup
    {
        public TMP_Text text;
        public CanvasGroup group;
        public Vector3 world;
        public float age;
    }

    readonly List<Popup> active = new List<Popup>();
    readonly Stack<Popup> spare = new Stack<Popup>();
    Canvas canvas;

    void Awake()
    {
        Instance = this;
        canvas = GetComponentInParent<Canvas>();
    }

    public void Show(Vector3 world, double mana)
    {
        Popup popup;
        if (spare.Count > 0)
        {
            popup = spare.Pop();
        }
        else
        {
            popup = new Popup { text = Instantiate(popupPrefab, transform) };
            popup.group = popup.text.GetComponent<CanvasGroup>();
        }

        popup.text.gameObject.SetActive(true);
        popup.text.text = "+" + Format.ManaIcon + Format.Short(mana);
        popup.world = world;
        popup.age = 0f;
        active.Add(popup);
        Place(popup);
    }

    void LateUpdate()
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            Popup popup = active[i];
            popup.age += Time.deltaTime;
            if (popup.age >= lifetime)
            {
                popup.text.gameObject.SetActive(false);
                active.RemoveAt(i);
                spare.Push(popup);
                continue;
            }
            Place(popup);
        }
    }

    // Rise quickly, then slow down; fade out over the last part of its life.
    void Place(Popup popup)
    {
        float t = popup.age / lifetime;
        Vector3 screen = cam.WorldToScreenPoint(popup.world);
        screen.y += rise * canvas.scaleFactor * (1f - (1f - t) * (1f - t));
        popup.text.rectTransform.position = screen;
        popup.group.alpha = t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f;
    }
}