using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public enum SoundType
{
    BackgroundMusic,
    ItemPickup,
    NPCTalking,
    QuestComplete
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] soundList;

    [Header("Volume Settings")]
    [Range(0.1f, 0.9f)]
    [SerializeField] private float musicDuckingAmount = 0.3f; // How much to reduce bg music during speech

    private static SoundManager instance;
    private AudioSource audioSource;
    private AudioSource oneTimeAudioSource;
    private AudioSource npcTalkingAudioSource;

    private bool wasBackgroundMusicPlaying = false;
    private float backgroundMusicVolume = 0f;
    private Coroutine npcTalkingCoroutine;
    private bool isInConversation = false;

    private void Awake()
    {
        instance = this;

        // Create a second audio source for one-time sounds
        oneTimeAudioSource = gameObject.AddComponent<AudioSource>();

        // Create a third audio source specifically for NPC talking
        npcTalkingAudioSource = gameObject.AddComponent<AudioSource>();
    }

    public void Start()
    {
        audioSource = GetComponent<AudioSource>();
        // Start background music on game start
        PlaySound(SoundType.BackgroundMusic, 0.7f, true);
    }

    public static void PlaySound(SoundType sound, float volume = 1f, bool loop = false)
    {
        if (sound == SoundType.QuestComplete)
        {
            // If playing quest complete sound, pause background music if it's playing
            if (instance.audioSource.isPlaying && instance.audioSource.loop)
            {
                instance.wasBackgroundMusicPlaying = true;
                instance.backgroundMusicVolume = instance.audioSource.volume;
                instance.audioSource.Pause();
            }

            // Play the quest complete sound on the one-time audio source
            instance.oneTimeAudioSource.clip = instance.soundList[(int)sound];
            instance.oneTimeAudioSource.volume = volume;
            instance.oneTimeAudioSource.loop = false;
            instance.oneTimeAudioSource.Play();

            // Start coroutine to resume background music after quest complete sound finishes
            instance.StartCoroutine(instance.ResumeBackgroundMusicAfterSound(instance.oneTimeAudioSource.clip.length));
            return;
        }

        if (loop)
        {
            instance.audioSource.clip = instance.soundList[(int)sound];
            instance.audioSource.volume = volume;
            instance.audioSource.loop = true;
            instance.audioSource.Play();
        }
        else
        {
            instance.audioSource.PlayOneShot(instance.soundList[(int)sound], volume);
        }
    }

    // Start a conversation - lower background music volume
    public static void StartConversation()
    {
        // If already in a conversation, don't do anything
        if (instance.isInConversation)
            return;

        instance.isInConversation = true;

        // Lower background music volume if it's playing
        if (instance.audioSource.isPlaying)
        {
            // Store original volume to restore later
            instance.backgroundMusicVolume = instance.audioSource.volume;
            // Lower the volume by the ducking amount
            instance.audioSource.volume = instance.backgroundMusicVolume * instance.musicDuckingAmount;
        }
    }

    // End a conversation - restore background music volume
    public static void EndConversation()
    {
        // If not in a conversation, don't do anything
        if (!instance.isInConversation)
            return;

        instance.isInConversation = false;

        // Restore background music volume
        if (instance.audioSource.isPlaying && instance.backgroundMusicVolume > 0)
        {
            instance.audioSource.volume = instance.backgroundMusicVolume;
        }
    }

    // New method to play NPC talking for a specified duration
    public static void PlayNPCTalkingSound(float duration, float volume = 1f)
    {
        // Stop any previous NPC talking
        StopNPCTalking();

        // Ensure we're in conversation mode (lowers background music)
        StartConversation();

        // Set up the NPC talking audio source
        instance.npcTalkingAudioSource.clip = instance.soundList[(int)SoundType.NPCTalking];
        instance.npcTalkingAudioSource.volume = volume;
        instance.npcTalkingAudioSource.loop = true;  // Loop so we can control duration
        instance.npcTalkingAudioSource.Play();

        // Start coroutine to stop after duration
        instance.npcTalkingCoroutine = instance.StartCoroutine(instance.StopNPCTalkingAfterDuration(duration));

        // Debug.Log($"Started NPC talking sound with duration: {duration:F2}s");
    }

    // Method to stop NPC talking
    public static void StopNPCTalking()
    {
        if (instance.npcTalkingCoroutine != null)
        {
            instance.StopCoroutine(instance.npcTalkingCoroutine);
            instance.npcTalkingCoroutine = null;
        }

        if (instance.npcTalkingAudioSource.isPlaying)
        {
            instance.npcTalkingAudioSource.Stop();
            // Debug.Log("Stopped NPC talking sound");

            // Don't restore background music volume here - we want it to stay lowered
            // until the entire conversation is over
        }
    }

    private IEnumerator StopNPCTalkingAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (instance.npcTalkingAudioSource.isPlaying)
        {
            instance.npcTalkingAudioSource.Stop();

            // Don't restore background music volume here - we want it to stay lowered
            // until the entire conversation is over
        }

        instance.npcTalkingCoroutine = null;
    }

    public static void StopLoopingSound()
    {
        if (instance.audioSource.loop)
        {
            instance.audioSource.Stop();
            instance.audioSource.loop = false;
        }
    }

    // Coroutine to resume background music after a delay
    private IEnumerator ResumeBackgroundMusicAfterSound(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (wasBackgroundMusicPlaying)
        {
            audioSource.volume = backgroundMusicVolume;
            audioSource.UnPause();
            wasBackgroundMusicPlaying = false;
        }
    }
}