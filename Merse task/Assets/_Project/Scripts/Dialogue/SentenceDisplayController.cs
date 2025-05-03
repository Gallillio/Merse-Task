using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using Core.Interfaces;
using Core.Services;

namespace Dialogue
{
    /// <summary>
    /// Controls displaying sentences one by one with user advancement
    /// </summary>
    public class SentenceDisplayController : MonoBehaviour
    {
        [Header("Input Configuration")]
        [SerializeField] private InputActionAsset inputAction;

        [Header("Voice Settings")]
        [Tooltip("How many characters per second the NPC talks")]
        [Range(5, 30)]
        [SerializeField] private float charactersPerSecond = 15f;

        [Tooltip("Minimum sound duration for very short sentences")]
        [SerializeField] private float minimumSoundDuration = 0.5f;

        [Tooltip("Maximum sound duration for very long sentences")]
        [SerializeField] private float maximumSoundDuration = 6f;

        private List<string> currentSentences = new List<string>();
        private int currentSentenceIndex = 0;
        private TMP_Text displayText;
        private bool awaitingUserAdvance = false;
        private IAudioService audioService;
        private ILoggingService logger;
        private InputAction advanceAction;

        /// <summary>
        /// Event triggered when a new sentence is displayed
        /// </summary>
        public event Action<string> OnSentenceDisplayed;

        /// <summary>
        /// Event triggered when all sentences have been displayed
        /// </summary>
        public event Action OnAllSentencesDisplayed;

        private void Awake()
        {
            // Get services from ServiceLocator
            audioService = ServiceLocator.Get<IAudioService>();
            logger = ServiceLocator.Get<ILoggingService>();
        }

        private void Start()
        {
            // Setup Input System action for advancing dialogue
            if (inputAction != null)
            {
                advanceAction = inputAction.FindActionMap("Controller").FindAction("Primary Button");
                advanceAction.Enable();
                advanceAction.performed += OnAdvanceActionPerformed;
                logger?.Log("Input action for dialogue advancement configured");
            }
            else
            {
                logger?.LogWarning("No input action asset assigned for dialogue advancement");
            }
        }

        private void OnDestroy()
        {
            // Clean up input action
            if (advanceAction != null)
            {
                advanceAction.performed -= OnAdvanceActionPerformed;
                advanceAction.Disable();
            }
        }

        private void Update()
        {
            // Keyboard fallback for testing
            if (awaitingUserAdvance && displayText != null && Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
            {
                AdvanceToNextSentence();
            }
        }

        private void OnAdvanceActionPerformed(InputAction.CallbackContext ctx)
        {
            if (awaitingUserAdvance && displayText != null)
            {
                AdvanceToNextSentence();
            }
        }

        /// <summary>
        /// Display a list of sentences one by one in the specified text component
        /// </summary>
        /// <param name="sentences">The sentences to display</param>
        /// <param name="targetText">The text component to display the sentences in</param>
        public void DisplaySentences(List<string> sentences, TMP_Text targetText)
        {
            if (sentences == null || sentences.Count == 0 || targetText == null)
            {
                logger?.LogWarning("Cannot display sentences: invalid parameters");
                return;
            }

            // Store references
            currentSentences = sentences;
            displayText = targetText;

            // Reset state
            currentSentenceIndex = 0;
            awaitingUserAdvance = true;

            // Start conversation (reduces background music)
            audioService?.StartConversation();

            // Display the first sentence
            if (currentSentences.Count > 0)
            {
                string firstSentence = currentSentences[0];
                displayText.text = firstSentence;

                // Notify listeners
                OnSentenceDisplayed?.Invoke(firstSentence);

                // Play NPC voice with delay to ensure text is visible first
                StartCoroutine(PlayNPCVoiceDelayed(firstSentence, 0.1f));

                logger?.Log($"Displaying first sentence: '{firstSentence}'");
            }
            else
            {
                displayText.text = "";
                logger?.Log("No sentences to display");
            }
        }

        /// <summary>
        /// Advance to the next sentence in the sequence
        /// </summary>
        public void AdvanceToNextSentence()
        {
            if (displayText == null)
                return;

            // Stop any currently playing NPC voice
            audioService?.StopNPCVoice();

            // Move to the next sentence
            currentSentenceIndex++;

            if (currentSentenceIndex < currentSentences.Count)
            {
                // Show the next sentence
                string sentenceToShow = currentSentences[currentSentenceIndex];
                displayText.text = sentenceToShow;

                // Notify listeners
                OnSentenceDisplayed?.Invoke(sentenceToShow);

                // Play NPC voice for this sentence
                StartCoroutine(PlayNPCVoiceDelayed(sentenceToShow, 0.1f));

                logger?.Log($"Displaying sentence {currentSentenceIndex + 1}/{currentSentences.Count}: '{sentenceToShow}'");
            }
            else
            {
                // We've shown all sentences
                awaitingUserAdvance = false;

                // End conversation and restore background music
                audioService?.EndConversation();

                // Notify listeners that all sentences have been displayed
                OnAllSentencesDisplayed?.Invoke();

                logger?.Log("All sentences have been displayed");
            }
        }

        /// <summary>
        /// Coroutine to play NPC voice after a short delay
        /// </summary>
        private IEnumerator PlayNPCVoiceDelayed(string text, float delay)
        {
            // Wait a short delay for the text to be visibly updated
            yield return new WaitForSeconds(delay);

            // Play the NPC voice
            PlayNPCVoice(text);
        }

        /// <summary>
        /// Play NPC voice with duration based on text length
        /// </summary>
        private void PlayNPCVoice(string text)
        {
            if (string.IsNullOrEmpty(text) || audioService == null)
                return;

            // Calculate appropriate duration based on text length
            float duration = Mathf.Clamp(
                text.Length / charactersPerSecond,
                minimumSoundDuration,
                maximumSoundDuration
            );

            // Play the NPC voice sound
            audioService.PlayNPCVoice(duration, 1.0f);

            logger?.Log($"Playing NPC voice for {duration:F2} seconds");
        }
    }
}