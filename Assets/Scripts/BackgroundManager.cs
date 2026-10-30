using System.Collections;
using UnityEngine;

// The picture behind the board, in three layers:
//   Back  - the room or the landscape
//   Mist  - a soft haze so the machines stay easy to read
//   Front - the frame around the edges, drawn in front of everything
// Each area has a look for every time of day. Every Rebirth moves one step along the list:
// workshop dawn, day, dusk, night, then the forest.
public class BackgroundManager : MonoBehaviour
{
    // One area at one time of day.
    [System.Serializable]
    public struct Look
    {
        public string name;
        public Sprite back;
        public Sprite mist;
        public Sprite front;
    }

    [SerializeField] SpriteRenderer back;
    [SerializeField] SpriteRenderer mist;
    [SerializeField] SpriteRenderer front;

    [Tooltip("In order. 0 Rebirths shows the first look, 1 Rebirth the second, and so on.")]
    [SerializeField] Look[] looks;
    [Tooltip("Seconds to fade from one look to the next.")]
    [SerializeField] float fadeTime = 2f;

    int shownLook = -1;

    void Start()
    {
        RebirthManager.Instance.Reborn += OnReborn;
        Show(RebirthManager.Instance.Rebirths, false);
    }

    void OnDestroy()
    {
        if (RebirthManager.Instance != null)
        {
            RebirthManager.Instance.Reborn -= OnReborn;
        }
    }

    void OnReborn()
    {
        Show(RebirthManager.Instance.Rebirths, true);
    }

    // Show the look for this many Rebirths. Past the end of the list, stay on the last one.
    void Show(int rebirths, bool fade)
    {
        int index = Mathf.Min(rebirths, looks.Length - 1);
        if (index == shownLook)
        {
            return;
        }
        shownLook = index;

        Look look = looks[index];
        if (fade)
        {
            StartCoroutine(FadeTo(back, look.back));
            StartCoroutine(FadeTo(mist, look.mist));
            StartCoroutine(FadeTo(front, look.front));
        }
        else
        {
            back.sprite = look.back;
            mist.sprite = look.mist;
            front.sprite = look.front;
        }
    }

    // Lay a copy of the old picture on top, put the new one underneath, then fade the copy away.
    IEnumerator FadeTo(SpriteRenderer layer, Sprite sprite)
    {
        SpriteRenderer old = Instantiate(layer, layer.transform.parent);
        old.sortingOrder = layer.sortingOrder + 1;
        layer.sprite = sprite;

        // Unscaled time, so it keeps fading while the game is paused for the Rebirth reward.
        for (float t = 0f; t < fadeTime; t += Time.unscaledDeltaTime)
        {
            old.color = new Color(1f, 1f, 1f, 1f - t / fadeTime);
            yield return null;
        }
        Destroy(old.gameObject);
    }
}