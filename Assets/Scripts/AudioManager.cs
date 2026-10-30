using System.Collections.Generic;
using UnityEngine;

// Plays the game's sound effects. There is one in the scene, on the AudioManager object with the
// AudioSource it plays through. Anything that makes a sound calls it, for example
// AudioManager.Instance.PlayCantAfford().
//
// There is no music: the factory makes the soundtrack. Condensers drip, Catalysts tink, statuses
// whoosh and Transmutators ding, and the more valuable the Element, the higher the note. Those
// sounds come in floods, so the same one never plays twice within a moment of itself.
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

    [Header("The factory")]
    [Tooltip("A Condenser dropping an Element.")]
    [SerializeField] AudioClip drop;
    [Tooltip("A Catalyst working on an Element.")]
    [SerializeField] AudioClip catalyst;
    [Tooltip("A Transmutator turning an Element into mana.")]
    [SerializeField] AudioClip transmute;
    [Tooltip("The same sound never plays twice within this many seconds.")]
    [SerializeField] float minimumGap = 0.05f;
    [Tooltip("How many sounds with a pitch of their own can play at once.")]
    [SerializeField] int voiceCount = 16;

    // The notes of a major pentatonic scale, in semitones above the first. Any of them sound good
    // together, so a busy factory still sounds musical.
    static readonly int[] Scale = { 0, 2, 4, 7, 9 };

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

    public void PlayReelTick()
    {
        PlayOften(reelTick, Random.Range(0.96f, 1.04f));
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

    // ---------------- The factory ----------------

    public void PlayDrop()
    {
        PlayOften(drop, Random.Range(0.94f, 1.06f));
    }

    // Higher notes for more valuable Elements.
    public void PlayCatalyst(double elementValue)
    {
        PlayOften(catalyst, PitchForValue(elementValue));
    }

    public void PlayTransmute(double elementValue)
    {
        PlayOften(transmute, PitchForValue(elementValue));
    }

    // A status (Burning, Frozen...) was put on an Element. The sound lives on the status itself.
    public void PlayStatus(ElementTag status)
    {
        if (status != null)
        {
            PlayOften(status.sound, Random.Range(0.95f, 1.05f));
        }
    }

    // A status was taken off an Element (a Quencher putting out a Burning one).
    public void PlayStatusRemoved(ElementTag status)
    {
        if (status != null)
        {
            PlayOften(status.soundWhenRemoved, Random.Range(0.95f, 1.05f));
        }
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

    // For sounds that can come in floods: skipped if the same sound only just played, and played
    // at a pitch of its own.
    void PlayOften(AudioClip clip, float pitch)
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

    // One note up the scale every time an Element's value grows ten times: 1 plays the first note,
    // 10 the second, 100 the third, and so on, up to an octave and a half higher.
    static float PitchForValue(double value)
    {
        int step = 0;
        double nextStep = 10;
        while (value >= nextStep && step < 8)
        {
            step++;
            nextStep *= 10;
        }

        int semitones = Scale[step % Scale.Length] + 12 * (step / Scale.Length);
        return Mathf.Pow(2f, semitones / 12f);
    }
}