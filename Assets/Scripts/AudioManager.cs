using System.Collections.Generic;
using UnityEngine;

// Plays the game's sound effects. There is one in the scene, on the AudioManager object with the
// AudioSource it plays through. Anything that makes a sound calls it, for example
// AudioManager.Instance.PlayCantAfford().
//
// There is no music, and the factory itself runs quietly: sounds mark what the player does (buying,
// placing, upgrading, Rebirth) rather than every Element, so they never turn into a constant drone.
//
// Sounds still play while the game is paused for the Rebirth window: pausing (Time.timeScale = 0)
// doesn't stop audio.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Tooltip("Plays every sound effect. Its volume is the volume of all of them.")]
    [SerializeField] AudioSource source;

    [Header("Rebirth")]
    [Tooltip("The screen filling with light as a Rebirth starts.")]
    [SerializeField] AudioClip rebirth;
    [Tooltip("You've just got enough mana to Rebirth.")]
    [SerializeField] AudioClip rebirthReady;
    [Tooltip("A reward landing, one sound per rarity: Common, Uncommon, Rare, Epic, Legendary.")]
    [SerializeField] AudioClip[] rewardLanded = new AudioClip[5];
    [Tooltip("A reward's reel turning to its next picture.")]
    [SerializeField] AudioClip reelTick;
    [Tooltip("Collecting the rewards and closing the window.")]
    [SerializeField] AudioClip collect;

    [Header("Spending mana")]
    [Tooltip("Clicking something you can't afford yet.")]
    [SerializeField] AudioClip cantAfford;
    [SerializeField] AudioClip buy;
    [SerializeField] AudioClip upgrade;

    [Header("The board")]
    [Tooltip("An item put down on the board.")]
    [SerializeField] AudioClip place;
    [Tooltip("An item put back into the inventory.")]
    [SerializeField] AudioClip stash;

    [Header("Rapid sounds (the reels)")]
    [Tooltip("The same rapid sound never plays twice within this many seconds.")]
    [SerializeField] float minimumGap = 0.05f;
    [Tooltip("How many rapid sounds, each at a pitch of its own, can play at once.")]
    [SerializeField] int voiceCount = 16;

    AudioSource[] voices;      // extra sources for sounds played at their own pitch
    int nextVoice;             // the voices take turns
    readonly Dictionary<AudioClip, float> lastPlayed = new Dictionary<AudioClip, float>();

    void Awake()
    {
        Instance = this;

        // Changing a source's pitch bends every sound it is still playing, so each pitched sound
        // gets a voice of its own. They copy the main source's settings.
        voices = new AudioSource[voiceCount];
        for (int i = 0; i < voiceCount; i++)
        {
            AudioSource voice = gameObject.AddComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.spatialBlend = 0f;
            voice.volume = source.volume;
            voice.outputAudioMixerGroup = source.outputAudioMixerGroup;
            voices[i] = voice;
        }
    }

    // ---------------- Rebirth ----------------

    public void PlayRebirth()
    {
        Play(rebirth);
    }

    public void PlayRebirthReady()
    {
        Play(rebirthReady);
    }

    // Each rarity has its own sound, bigger the rarer it is.
    public void PlayRewardLanded(Rarity rarity)
    {
        int index = (int)rarity;
        if (index < rewardLanded.Length)
        {
            Play(rewardLanded[index]);
        }
    }

    // Several reels can turn at once, so their clicks are spaced out and vary a little in pitch.
    public void PlayReelTick()
    {
        PlayRapid(reelTick, Random.Range(0.96f, 1.04f));
    }

    public void PlayCollect()
    {
        Play(collect);
    }

    // ---------------- Spending mana ----------------

    public void PlayCantAfford()
    {
        Play(cantAfford);
    }

    public void PlayBuy()
    {
        Play(buy);
    }

    public void PlayUpgrade()
    {
        Play(upgrade);
    }

    // ---------------- The board ----------------

    public void PlayPlace()
    {
        Play(place);
    }

    public void PlayStash()
    {
        Play(stash);
    }

    // ---------------- Playing ----------------

    // For sounds that only happen now and then. PlayOneShot lets sounds overlap instead of
    // cutting each other off.
    void Play(AudioClip clip)
    {
        if (clip != null && source != null)
        {
            source.PlayOneShot(clip);
        }
    }

    // For sounds that can come many at a time: skipped if the same sound only just played, and
    // played at a pitch of its own.
    void PlayRapid(AudioClip clip, float pitch)
    {
        if (clip == null || PlayedRecently(clip))
        {
            return;
        }

        AudioSource voice = voices[nextVoice];
        nextVoice = (nextVoice + 1) % voices.Length;
        voice.pitch = pitch;
        voice.PlayOneShot(clip);
    }

    // Did this sound play less than minimumGap seconds ago? If not, it counts as playing now.
    // (Real time, so it works while the game is paused too.)
    bool PlayedRecently(AudioClip clip)
    {
        float lastTime;
        if (lastPlayed.TryGetValue(clip, out lastTime) && Time.unscaledTime - lastTime < minimumGap)
        {
            return true;
        }
        lastPlayed[clip] = Time.unscaledTime;
        return false;
    }
}