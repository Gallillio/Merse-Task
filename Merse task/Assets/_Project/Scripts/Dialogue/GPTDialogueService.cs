using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Core.Interfaces;
using Core.Services;

namespace Dialogue
{
    /// <summary>
    /// Service for handling dialogue interactions using Gemini API
    /// </summary>
    [RequireComponent(typeof(SentenceDisplayController))]
    public class GPTDialogueService : MonoBehaviour, IDialogueProvider
    {
        [Header("API Configuration")]
        [SerializeField] private string geminiApiKey;

        [Header("System Message")]
        [TextArea(3, 10)]
        [SerializeField] private string systemMessage;

        private GeminiAPI geminiAPI;
        private SentenceSplitter splitter = new SentenceSplitter();
        private SentenceDisplayController displayController;
        private ILoggingService logger;

        // Dictionary to store conversation history for each NPC
        private Dictionary<string, List<ChatMessage>> npcConversationHistories = new Dictionary<string, List<ChatMessage>>();
        private GameObject currentNpcObject;
        private TMP_Text currentResponseText;

        /// <summary>
        /// Event triggered when a new sentence is displayed
        /// </summary>
        public event Action<string> OnSentenceDisplayed;

        /// <summary>
        /// Event triggered when all sentences have been displayed
        /// </summary>
        public event Action OnDialogueCompleted;

        private void Awake()
        {
            // Get the sentence display controller component
            displayController = GetComponent<SentenceDisplayController>();
            logger = ServiceLocator.Get<ILoggingService>();

            // Create the Gemini API client
            geminiAPI = new GeminiAPI(geminiApiKey, logger);

            // Register for events from the display controller
            if (displayController != null)
            {
                displayController.OnSentenceDisplayed += OnSentenceShown;
                displayController.OnAllSentencesDisplayed += OnAllSentencesShown;
            }
            else
            {
                logger?.LogError("SentenceDisplayController not found on GPTDialogueService GameObject");
            }
        }

        private void OnDestroy()
        {
            // Unregister from display controller events
            if (displayController != null)
            {
                displayController.OnSentenceDisplayed -= OnSentenceShown;
                displayController.OnAllSentencesDisplayed -= OnAllSentencesShown;
            }
        }

        /// <summary>
        /// Start a dialogue interaction with an NPC
        /// </summary>
        /// <param name="npc">The NPC GameObject</param>
        /// <param name="input">The user's input text</param>
        /// <param name="instruction">Custom instruction for this dialogue</param>
        /// <param name="onResponse">Callback for when a response is received</param>
        public void StartDialogue(GameObject npc, string input, string instruction, Action<string> onResponse)
        {
            if (npc == null)
            {
                logger?.LogError("Cannot start dialogue: NPC GameObject is null");
                return;
            }

            // Store the current NPC for later reference
            currentNpcObject = npc;

            // Check if this is an automatic conversation (empty user input)
            bool isAutoConversation = string.IsNullOrWhiteSpace(input);
            if (isAutoConversation)
            {
                input = "The player approaches.";
                logger?.Log($"Using default input for auto-initiated conversation: '{input}'");
            }

            // Find the response text component on the NPC
            TMP_Text responseText = FindResponseTextComponent(npc);
            if (responseText == null)
            {
                logger?.LogError($"Response text component not found on NPC {npc.name}");
                return;
            }

            // Store the current response text component
            currentResponseText = responseText;
            currentResponseText.text = "Thinking...";

            // Get conversation history for this NPC
            List<ChatMessage> npcHistory = GetConversationHistoryForNPC(npc);

            // For auto-conversations, clear previous history to start fresh
            if (isAutoConversation)
            {
                npcHistory.Clear();
                logger?.Log("Cleared conversation history for auto-conversation");
            }

            // Add the user message to history
            npcHistory.Add(new ChatMessage { Role = "user", Content = input });

            // Use combined instruction (system message + NPC-specific instruction)
            string combinedInstruction = systemMessage;
            if (!string.IsNullOrEmpty(instruction))
            {
                combinedInstruction += "\n" + instruction;
            }

            // Send the request to Gemini API
            _ = RequestGeminiResponseAsync(input, npcHistory, combinedInstruction, (response) =>
            {
                // Handle the response
                OnGeminiResponseReceived(response, onResponse);
            });
        }

        /// <summary>
        /// Advance to the next sentence in the dialogue
        /// </summary>
        public void AdvanceDialogue()
        {
            displayController?.AdvanceToNextSentence();
        }

        /// <summary>
        /// Find the response text component on an NPC
        /// </summary>
        private TMP_Text FindResponseTextComponent(GameObject npcObject)
        {
            // Look for NPCInstructionUI component on the NPC
            var npcInstructionComponent = npcObject.GetComponent<NPCInstructionUI>();

            // If not found on the parent object, try looking in children
            if (npcInstructionComponent == null)
            {
                logger?.Log($"NPCInstructionUI not found on {npcObject.name}, searching in children...");
                npcInstructionComponent = npcObject.GetComponentInChildren<NPCInstructionUI>();
            }

            // If found, check the responseText
            if (npcInstructionComponent != null && npcInstructionComponent.responseText != null)
            {
                return npcInstructionComponent.responseText;
            }

            return null;
        }

        /// <summary>
        /// Get or create conversation history for a specific NPC
        /// </summary>
        private List<ChatMessage> GetConversationHistoryForNPC(GameObject npcObject)
        {
            // Create a unique ID for this NPC (using instance ID)
            string npcId = npcObject.GetInstanceID().ToString();

            // If this NPC doesn't have a conversation history yet, create one
            if (!npcConversationHistories.ContainsKey(npcId))
            {
                npcConversationHistories[npcId] = new List<ChatMessage>();
                logger?.Log($"Created new conversation history for NPC {npcId}");
            }

            return npcConversationHistories[npcId];
        }

        /// <summary>
        /// Clear conversation history for a specific NPC
        /// </summary>
        public void ClearConversationHistoryForNPC(GameObject npcObject)
        {
            if (npcObject != null)
            {
                string npcId = npcObject.GetInstanceID().ToString();
                if (npcConversationHistories.ContainsKey(npcId))
                {
                    npcConversationHistories[npcId].Clear();
                    logger?.Log($"Cleared conversation history for NPC {npcId}");
                }
            }
        }

        /// <summary>
        /// Request a response from Gemini API asynchronously
        /// </summary>
        private async Task RequestGeminiResponseAsync(string message, List<ChatMessage> history, string instruction, Action<string> onResponse)
        {
            try
            {
                // Request response from Gemini API
                string response = await geminiAPI.GenerateResponseAsync(message, history, instruction);

                // Call the callback with the response
                onResponse?.Invoke(response);
            }
            catch (Exception ex)
            {
                logger?.LogError($"Error requesting Gemini response: {ex.Message}");
                onResponse?.Invoke(null);
            }
        }

        /// <summary>
        /// Handle the response received from Gemini API
        /// </summary>
        private void OnGeminiResponseReceived(string response, Action<string> originalCallback)
        {
            if (currentNpcObject != null)
            {
                // Add model response to this NPC's conversation history
                List<ChatMessage> npcHistory = GetConversationHistoryForNPC(currentNpcObject);
                npcHistory.Add(new ChatMessage { Role = "model", Content = response });
            }

            if (string.IsNullOrEmpty(response) || currentResponseText == null)
            {
                if (currentResponseText != null)
                {
                    currentResponseText.text = "Error getting response.";
                }

                // Invoke the original callback
                originalCallback?.Invoke(response);
                return;
            }

            // Clean the response
            string cleanedResponse = splitter.CleanResponseText(response);

            // Split response into sentences
            List<string> sentences = splitter.SplitIntoSentences(cleanedResponse);

            // Use the display controller to display sentences
            displayController.DisplaySentences(sentences, currentResponseText);

            // Invoke the original callback 
            originalCallback?.Invoke(response);
        }

        /// <summary>
        /// Event handler for when a sentence is shown by the display controller
        /// </summary>
        private void OnSentenceShown(string sentence)
        {
            // Forward event to subscribers
            OnSentenceDisplayed?.Invoke(sentence);
        }

        /// <summary>
        /// Event handler for when all sentences have been shown
        /// </summary>
        private void OnAllSentencesShown()
        {
            // Forward event to subscribers
            OnDialogueCompleted?.Invoke();
        }
    }
}