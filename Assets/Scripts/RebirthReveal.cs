using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The Rebirth moment: a flash of light hides the board being cleared, then a window shows what this
// Rebirth gave. Each reward spins through the items it could have been, slower and slower, and lands
// with a burst in the colour of its rarity. The game stays paused until you collect them.
public class RebirthReveal : MonoBehaviour
{
    [Tooltip("A white image over the whole screen.")]
    [SerializeField] Image flash;
    [Tooltip("The window, with the dimmed board behind it. Must be a child, not this object.")]
    [SerializeField] GameObject panel;
    [Tooltip("The window itself. It pops in, and grows to fit the rewards.")]
    [SerializeField] RectTransform window;
    [SerializeField] TMP_Text title;
    [SerializeField] TMP_Text subtitle;
    [SerializeField] Transform slotParent;
    [Tooltip("Behind the slots. Each reward's glow and rays go here, so they never cover the reward next to it.")]
    [SerializeField] Transform lightLayer;
    [SerializeField] RewardSlot slotPrefab;
    [SerializeField] Button collectButton;

    [Header("Timing (real seconds)")]
    [SerializeField] float flashIn = 0.35f;
    [SerializeField] float flashOut = 0.9f;
    [Tooltip("How long the first reward spins.")]
    [SerializeField] float spinTime = 1.2f;
    [Tooltip("Each later reward spins this much longer, so they land one after another.")]
    [SerializeField] float stagger = 0.5f;

    [Header("Size")]
    [Tooltip("The window is never narrower than this.")]
    [SerializeField] float minWidth = 600f;
    [Tooltip("Room for each reward (a slot and the gap after it).")]
    [SerializeField] float widthPerReward = 216f;
    [Tooltip("Room either side of the rewards.")]
    [SerializeField] float margin = 200f;
    [Tooltip("The window is never wider than this (the rewards spill over its edges past it).")]
    [SerializeField] float maxWidth = 1880f;

    static readonly Color Cream = new Color(1f, 0.97f, 0.88f);

    int rewardsLanded;      // how many of this Rebirth's rewards have finished spinning
    Coroutine colorPulse;   // the screen lighting up for an Epic or Legendary reward

    public bool IsOpen
    {
        get { return panel.activeSelf; }
    }

    void Awake()
    {
        panel.SetActive(false);
        flash.gameObject.SetActive(false);
        collectButton.onClick.AddListener(Collect);
    }

    // Fade the screen to white. The board is cleared while it's covered.
    public IEnumerator FlashIn()
    {
        flash.gameObject.SetActive(true);
        for (float t = 0f; t < flashIn; t += Time.unscaledDeltaTime)
        {
            SetFlash(Cream, t / flashIn);
            yield return null;
        }
        SetFlash(Cream, 1f);
    }

    // Open the window for this Rebirth and start the reels.
    public void Show(int rebirth, double income, List<PlacedItem> rewards, List<bool> isNew)
    {
        title.text = "Rebirth " + rebirth;

        string rewardCount;
        if (rewards.Count == 1)
        {
            rewardCount = "1 reward";
        }
        else
        {
            rewardCount = rewards.Count + " rewards";
        }
        subtitle.text = "Income x" + income.ToString("0.##") + "     " + rewardCount;

        // The last Rebirth's slots go.
        foreach (Transform child in slotParent)
        {
            Destroy(child.gameObject);
        }

        // Wide enough for this many rewards, and no wider.
        float width = Mathf.Clamp(margin + widthPerReward * rewards.Count, minWidth, maxWidth);
        window.sizeDelta = new Vector2(width, window.sizeDelta.y);

        panel.SetActive(true);
        collectButton.interactable = false;
        StartCoroutine(Play(rebirth, rewards, isNew));
    }

    IEnumerator Play(int rebirth, List<PlacedItem> rewards, List<bool> isNew)
    {
        // What the reels flick through: everything this Rebirth could have given.
        List<Sprite> reel = new List<Sprite>();
        foreach (RebirthTable.Entry entry in RebirthManager.Instance.Table.PoolFor(rebirth))
        {
            if (entry.item.Icon != null)
            {
                reel.Add(entry.item.Icon);
            }
        }

        // One slot per reward. Each one spins a little longer than the one before.
        rewardsLanded = 0;
        for (int i = 0; i < rewards.Count; i++)
        {
            RewardSlot slot = Instantiate(slotPrefab, slotParent);
            slot.PutLightsIn(lightLayer);
            float spinFor = spinTime + stagger * i;
            slot.StartCoroutine(slot.Spin(reel, rewards[i], isNew[i], spinFor, OnRewardLanded));
        }

        // The light fades as the window pops in and the reels start turning.
        for (float t = 0f; t < flashOut; t += Time.unscaledDeltaTime)
        {
            SetFlash(Cream, 1f - t / flashOut);
            float pop = 1f - 0.15f * Mathf.Exp(-8f * t) * Mathf.Cos(10f * t); // a little overshoot
            window.localScale = Vector3.one * pop;
            yield return null;
        }
        window.localScale = Vector3.one;
        if (colorPulse == null)
        {
            flash.gameObject.SetActive(false);
        }

        // Collect only works once every reward has landed.
        while (rewardsLanded < rewards.Count)
        {
            yield return null;
        }
        collectButton.interactable = true;
    }

    // A reward finished spinning. An Epic or Legendary one lights up the whole screen in its colour.
    void OnRewardLanded(Rarity rarity)
    {
        rewardsLanded++;

        if (rarity < Rarity.Epic)
        {
            return;
        }

        Color color = RebirthManager.Instance.Table.ColorOf(rarity);
        float strength = 0.3f;
        if (rarity == Rarity.Legendary)
        {
            strength = 0.55f;
        }

        if (colorPulse != null)
        {
            StopCoroutine(colorPulse);
        }
        colorPulse = StartCoroutine(Pulse(color, strength));
    }

    IEnumerator Pulse(Color color, float strength)
    {
        const float duration = 0.5f;
        flash.gameObject.SetActive(true);
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            SetFlash(color, strength * (1f - t / duration));
            yield return null;
        }
        flash.gameObject.SetActive(false);
        colorPulse = null;
    }

    void SetFlash(Color color, float alpha)
    {
        flash.color = new Color(color.r, color.g, color.b, alpha);
    }

    // Close the window and let the game run again.
    void Collect()
    {
        if (colorPulse != null)
        {
            StopCoroutine(colorPulse);
            colorPulse = null;
            flash.gameObject.SetActive(false);
        }
        panel.SetActive(false);
        Time.timeScale = 1f;
    }
}