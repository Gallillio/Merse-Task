using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using Core.Interfaces;

namespace Core.Services
{
    /// <summary>
    /// Implementation of IAudioService that provides sound playback
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioService : MonoBehaviour, IAudioService
    {
        [Header("Sound Configuration")]
        [SerializeField] private AudioClip[] soundList;

        [Header("Volume Settings")]
        [Range(0.1f, 0.9f)]
        [SerializeField] private float musicDuckingAmount = 0.3f;

        private AudioSource primaryAudioSource;
        private AudioSource oneTimeAudioSource;
        private AudioSource npcVoiceAudioSource;

        private float originalBackgroundVolume;
        private bool isInConversation = false;
        private Coroutine npcVoiceCoroutine;
        private bool wasBackgroundMusicPlaying = false;

        private ILoggingService logger;

        /// <summary>
        /// Event triggered when a sound is played
        /// </summary>
        public event Action<SoundType> OnSoundPlayed;

        /// <summary>
        /// Initialize audio sources
        /// </summary>
        private void Awake()
        {
            // Initialize audio sources
            primaryAudioSource = GetComponent<AudioSource>();
            oneTimeAudioSource = gameObject.AddComponent<AudioSource>();
            npcVoiceAudioSource = gameObject.AddComponent<AudioSource>();
        }

        /// <summary>
        /// Start background music when the service starts
        /// </summary>
        private void Start()
        {
            logger = ServiceLocator.Get<ILoggingService>();

            // Start background music
            Play(Core.Interfaces.SoundType.BackgroundMusic, 0.7f, true);

            logger?.Log("AudioService initialized");
        }

        /// <summary>
        /// Play a sound effect or music
        /// </summary>
        /// <param name="type">The type of sound to play</param>
        /// <param name="volume">Volume level (0-1)</param>
        /// <param name="loop">Whether the sound should loop</param>
        public void Play(Core.Interfaces.SoundType type, float volume = 1f, bool loop = false)
        {
            // Handle quest complete sound specially - it pauses background music
            if (type == Core.Interfaces.SoundType.QuestComplete)
            {
                PlayQuestCompleteSound(volume);
                return;
            }

            // Regular sound playback
            if (loop)
            {
                primaryAudioSource.clip = soundList[(int)type];
                primaryAudioSource.volume = volume;
                primaryAudioSource.loop = true;
                primaryAudioSource.Play();

                logger?.Log($"Playing looping sound: {type}");
            }
            else
            {
                primaryAudioSource.PlayOneShot(soundList[(int)type], volume);
                logger?.Log($"Playing one-shot sound: {type}");
            }

            // Trigger the event
            OnSoundPlayed?.Invoke(type);
        }

        /// <summary>
        /// Specially handle quest complete sound (pause background music)
        /// </summary>
        private void PlayQuestCompleteSound(float volume)
        {
            // If background music is playing, pause it
            if (primaryAudioSource.isPlaying && primaryAudioSource.loop)
            {
                wasBackgroundMusicPlaying = true;
                originalBackgroundVolume = primaryAudioSource.volume;
                primaryAudioSource.Pause();

                logger?.Log("Paused background music for quest complete sound");
            }

            // Play quest complete sound on one-time audio source
            oneTimeAudioSource.clip = soundList[(int)Core.Interfaces.SoundType.QuestComplete];
            oneTimeAudioSource.volume = volume;
            oneTimeAudioSource.loop = false;
            oneTimeAudioSource.Play();

            // Resume background music after the sound finishes
            StartCoroutine(ResumeBackgroundMusicAfterDelay(oneTimeAudioSource.clip.length));

            logger?.Log("Playing quest complete sound");

            // Trigger the event
            OnSoundPlayed?.Invoke(Core.Interfaces.SoundType.QuestComplete);
        }

        /// <summary>
        /// Play an NPC voice sound with a specific duration
        /// </summary>
        /// <param name="duration">How long the voice should play</param>
        /// <param name="volume">Volume level (0-1)</param>
        public void PlayNPCVoice(float duration, float volume = 1f)
        {
            // Stop any previous voice sound
            StopNPCVoice();

            // Make sure we're in conversation mode (reduces background music volume)
            StartConversation();

            // Set up the NPC voice audio source
            npcVoiceAudioSource.clip = soundList[(int)Core.Interfaces.SoundType.NPCTalking];
            npcVoiceAudioSource.volume = volume;
            npcVoiceAudioSource.loop = true;  // Enable looping for controlled duration
            npcVoiceAudioSource.Play();

            // Start coroutine to stop after duration
            npcVoiceCoroutine = StartCoroutine(StopNPCVoiceAfterDuration(duration));

            logger?.Log($"Playing NPC voice for {duration:F2} seconds");

            // Trigger the event
            OnSoundPlayed?.Invoke(Core.Interfaces.SoundType.NPCTalking);
        }

        /// <summary>
        /// Stop any currently playing NPC voice
        /// </summary>
        public void StopNPCVoice()
        {
            // Stop the coroutine if it's running
            if (npcVoiceCoroutine != null)
            {
                StopCoroutine(npcVoiceCoroutine);
                npcVoiceCoroutine = null;
            }

            // Stop the voice audio
            if (npcVoiceAudioSource.isPlaying)
            {
                npcVoiceAudioSource.Stop();
                logger?.Log("Stopped NPC voice sound");
            }
        }

        /// <summary>
        /// Reduce background music volume for a conversation
        /// </summary>
        public void StartConversation()
        {
            // If already in conversation mode, do nothing
            if (isInConversation)
                return;

            isInConversation = true;

            // Reduce background music volume if it's playing
            if (primaryAudioSource.isPlaying)
            {
                // Store original volume to restore later
                originalBackgroundVolume = primaryAudioSource.volume;

                // Reduce the volume
                primaryAudioSource.volume = originalBackgroundVolume * musicDuckingAmount;

                logger?.Log($"Reducing background music volume for conversation from {originalBackgroundVolume:F2} to {primaryAudioSource.volume:F2}");
            }
        }

        /// <summary>
        /// Restore background music volume after a conversation
        /// </summary>
        public void EndConversation()
        {
            // If not in conversation mode, do nothing
            if (!isInConversation)
                return;

            isInConversation = false;

            // Restore background music volume if it's playing
            if (primaryAudioSource.isPlaying && originalBackgroundVolume > 0)
            {
                primaryAudioSource.volume = originalBackgroundVolume;
                logger?.Log($"Restored background music volume to {originalBackgroundVolume:F2}");
            }
        }

        /// <summary>
        /// Coroutine to stop NPC voice after a specific duration
        /// </summary>
        private IEnumerator StopNPCVoiceAfterDuration(float duration)
        {
            yield return new WaitForSeconds(duration);

            if (npcVoiceAudioSource.isPlaying)
            {
                npcVoiceAudioSource.Stop();
                logger?.Log($"NPC voice completed after {duration:F2} seconds");
            }

            npcVoiceCoroutine = null;
        }

        /// <summary>
        /// Coroutine to resume background music after a delay
        /// </summary>
        private IEnumerator ResumeBackgroundMusicAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            if (wasBackgroundMusicPlaying)
            {
                primaryAudioSource.volume = originalBackgroundVolume;
                primaryAudioSource.UnPause();
                wasBackgroundMusicPlaying = false;

                logger?.Log("Resumed background music after quest complete sound");
            }
        }
    }
}