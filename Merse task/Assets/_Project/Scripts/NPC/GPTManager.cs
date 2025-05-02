using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.InputSystem; // For the new Input System
using System;

public class GPTManager : MonoBehaviour
{
    [Header("Google Gemini API Key (for testing only, do not use in future)")]
    public string geminiApiKey = "";

    [Header("System Configuration")]
    [TextArea(2, 5)]
    public string systemMessage; // Set in Inspector

    [Header("Input System")]
    public InputActionAsset inputAction; // Assign in Inspector
    private InputAction advanceAction;

    // Tracking the current NPC for responses
    private GameObject currentNpcObject;
    private TMP_Text currentResponseText;

    // Dictionary to store conversation history for each NPC
    private Dictionary<string, List<ChatMessage>> npcConversationHistories = new Dictionary<string, List<ChatMessage>>();

    // Sentence-by-sentence display fields
    private List<string> currentSentences = new List<string>();
    private int currentSentenceIndex = 0;
    private bool awaitingUserAdvance = false;

    void Start()
    {
        // Setup Input System action for advancing dialogue
        advanceAction = inputAction.FindActionMap("Controller").FindAction("Primary Button");
        advanceAction.Enable();
        advanceAction.performed += OnAdvancePerformed;
    }

    void OnDestroy()
    {
        advanceAction.performed -= OnAdvancePerformed;
    }

    private void OnAdvancePerformed(InputAction.CallbackContext ctx)
    {
        if (awaitingUserAdvance && currentResponseText != null)
        {
            ShowNextSentence();
        }
    }

    void Update()
    {
        // Keyboard fallback for testing
        if (awaitingUserAdvance && currentResponseText != null && Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            ShowNextSentence();
        }
    }

    // Get or create a conversation history for a specific NPC
    private List<ChatMessage> GetConversationHistoryForNPC(GameObject npcObject)
    {
        // Create a unique ID for this NPC (using instance ID)
        string npcId = npcObject != null ? npcObject.GetInstanceID().ToString() : "default";

        // If this NPC doesn't have a conversation history yet, create one
        if (!npcConversationHistories.ContainsKey(npcId))
        {
            npcConversationHistories[npcId] = new List<ChatMessage>();
            // Debug.Log($"Created new conversation history for NPC {npcId}");
        }

        return npcConversationHistories[npcId];
    }

    // Clear conversation history for a specific NPC
    public void ClearConversationHistoryForNPC(GameObject npcObject)
    {
        if (npcObject != null)
        {
            string npcId = npcObject.GetInstanceID().ToString();
            if (npcConversationHistories.ContainsKey(npcId))
            {
                npcConversationHistories[npcId].Clear();
                Debug.Log($"Cleared conversation history for NPC {npcId}");

                // Hide spatial panel when conversation is cleared
                NPCInteractionManager npcInteractionManager = npcObject.GetComponentInChildren<NPCInteractionManager>();
                if (npcInteractionManager != null)
                {
                    npcInteractionManager.HideSpatialPanel();
                }
            }
        }
    }

    public void TrySendInput(string userInput, GameObject npcObject = null)
    {
        if (!string.IsNullOrEmpty(userInput) && npcObject != null)
        {
            Debug.Log($"TrySendInput called for NPC: {npcObject.name}");

            // Track the current NPC for responses
            currentNpcObject = npcObject;

            // First, ensure the spatial panel is enabled so we can access components inside it
            NPCInteractionManager npcInteractionManager = npcObject.GetComponentInChildren<NPCInteractionManager>();
            if (npcInteractionManager != null)
            {
                npcInteractionManager.ShowSpatialPanel();
                Debug.Log("Showing spatial panel before accessing responseText");
            }
            else
            {
                Debug.LogError($"No NPCInteractionManager found on {npcObject.name} or its children");
            }

            // Search for NPCInstruction component more thoroughly
            NPCInstruction npcInstructionComponent = npcObject.GetComponent<NPCInstruction>();

            // If not found on the parent object, try looking in children
            if (npcInstructionComponent == null)
            {
                Debug.Log($"NPCInstruction not found on {npcObject.name}, searching in children...");
                npcInstructionComponent = npcObject.GetComponentInChildren<NPCInstruction>();
            }

            // If found, check the responseText
            if (npcInstructionComponent != null)
            {
                Debug.Log($"Found NPCInstruction on {npcInstructionComponent.gameObject.name}");

                if (npcInstructionComponent.responseText != null)
                {
                    currentResponseText = npcInstructionComponent.responseText;
                    currentResponseText.text = "Hmmm...";
                    Debug.Log($"Set responseText to 'Hmmm...'");
                }
                else
                {
                    Debug.LogError($"NPCInstruction found on {npcInstructionComponent.gameObject.name} but responseText is null");
                    return;
                }
            }
            else
            {
                Debug.LogError($"NPCInstruction component not found on {npcObject.name} or any of its children");
                return;
            }

            // Get this NPC's conversation history
            List<ChatMessage> npcConversationHistory = GetConversationHistoryForNPC(npcObject);

            // Add user message to this NPC's conversation history
            npcConversationHistory.Add(new ChatMessage { role = "user", content = userInput });

            // Get NPC instructions
            string npcInstruction = npcInstructionComponent.npcInstruction;
            Debug.Log($"Using NPC instruction: {(string.IsNullOrEmpty(npcInstruction) ? "None" : npcInstruction)}");

            // Send the request with this NPC's conversation history
            RequestGeminiResponse(userInput, npcConversationHistory, OnGeminiResponse, npcInstruction);
        }
        else
        {
            Debug.LogError("Cannot process input: " +
                (string.IsNullOrEmpty(userInput) ? "Empty user input" : "No NPC object provided"));
        }
    }

    void OnGeminiResponse(string response)
    {
        if (currentNpcObject != null)
        {
            Debug.Log("OnGeminiResponse called");

            // Add model response to this NPC's conversation history
            List<ChatMessage> npcConversationHistory = GetConversationHistoryForNPC(currentNpcObject);
            npcConversationHistory.Add(new ChatMessage { role = "model", content = response });

            // Enable spatial panel on the NPC before showing response
            NPCInteractionManager npcInteractionManager = currentNpcObject.GetComponentInChildren<NPCInteractionManager>();
            if (npcInteractionManager != null)
            {
                Debug.Log("Showing spatial panel");
                npcInteractionManager.ShowSpatialPanel();
            }
            else
            {
                Debug.LogWarning("Could not find NPCInteractionManager on NPC: " + currentNpcObject.name);
            }
        }

        if (string.IsNullOrEmpty(response) || currentResponseText == null)
        {
            if (currentResponseText != null)
                currentResponseText.text = "Error getting response.";
            awaitingUserAdvance = false;
            return;
        }

        // Split response into sentences
        currentSentences = SplitIntoSentences(response);
        currentSentenceIndex = 0;
        awaitingUserAdvance = true;

        if (currentSentences.Count > 0)
            currentResponseText.text = currentSentences[0];
        else
            currentResponseText.text = "";
    }

    private List<string> SplitIntoSentences(string text)
    {
        var sentences = new List<string>();
        if (string.IsNullOrEmpty(text))
            return sentences;
        System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"(?<=[.?!])\s+");
        sentences.AddRange(regex.Split(text));
        return sentences;
    }

    private void ShowNextSentence()
    {
        if (currentResponseText == null)
            return;

        currentSentenceIndex++;
        if (currentSentenceIndex < currentSentences.Count)
        {
            currentResponseText.text = currentSentences[currentSentenceIndex];
        }
        else
        {
            // Don't set text to empty string before hiding panel
            // currentResponseText.text = "";
            awaitingUserAdvance = false;

            // Hide the spatial panel when we're done showing all sentences
            if (currentNpcObject != null)
            {
                NPCInteractionManager npcInteractionManager = currentNpcObject.GetComponentInChildren<NPCInteractionManager>();
                if (npcInteractionManager != null)
                {
                    npcInteractionManager.HideSpatialPanel();
                }
            }
        }
    }

    // Call this to send a message to Gemini
    public void RequestGeminiResponse(string message, List<ChatMessage> conversationHistory, System.Action<string> onResponse)
    {
        StartCoroutine(SendMessageToGemini(message, conversationHistory, onResponse));
    }

    // Overload to allow per-NPC instruction
    public void RequestGeminiResponse(string message, List<ChatMessage> conversationHistory, System.Action<string> onResponse, string npcInstruction)
    {
        StartCoroutine(SendMessageToGemini(message, conversationHistory, onResponse, npcInstruction));
    }

    // Modified coroutine to accept npcInstruction
    public IEnumerator SendMessageToGemini(string message, List<ChatMessage> conversationHistory, System.Action<string> onResponse, string npcInstruction = null)
    {
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={geminiApiKey}";

        var contents = new List<object>();
        // Combine global system message and per-NPC instruction if present
        string combinedSystemMessage = systemMessage;
        if (!string.IsNullOrEmpty(npcInstruction))
        {
            if (!string.IsNullOrEmpty(systemMessage))
            {
                combinedSystemMessage += "\n" + npcInstruction;
                // Debug.Log($"Combined system message: {systemMessage} + NPC instruction: {npcInstruction}");
            }
            else
            {
                combinedSystemMessage = npcInstruction;
                Debug.Log($"Using only NPC instruction (no system message): {npcInstruction}");
            }
        }
        else
        {
            Debug.Log($"Using only system message (no NPC instruction): {systemMessage}");
        }

        // Add conversation history, but prepend system message to the first user message
        bool systemPrepended = false;
        if (conversationHistory != null)
        {
            foreach (var msg in conversationHistory)
            {
                string text = msg.content;
                if (!systemPrepended && !string.IsNullOrEmpty(combinedSystemMessage) && msg.role == "user")
                {
                    text = combinedSystemMessage + "\n" + text;
                    systemPrepended = true;
                }
                contents.Add(new
                {
                    role = msg.role,
                    parts = new object[] { new { text = text } }
                });
            }
        }
        // Add the new message
        string newMessageText = message;
        if (!systemPrepended && !string.IsNullOrEmpty(combinedSystemMessage))
        {
            newMessageText = combinedSystemMessage + "\n" + newMessageText;
        }
        contents.Add(new
        {
            role = "user",
            parts = new object[] { new { text = newMessageText } }
        });

        var payload = new
        {
            contents = contents,
            generationConfig = new
            {
                temperature = 1,
                topK = 40,
                topP = 0.95,
                maxOutputTokens = 8192,
                responseMimeType = "text/plain"
            }
        };

        string jsonBody = JsonConvert.SerializeObject(payload);

        Debug.Log($"Gemini API Request Payload: {jsonBody}");

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        string responseText = request.downloadHandler.text;
        Debug.Log($"Gemini API Response: {responseText}");

        if (request.result == UnityWebRequest.Result.Success)
        {
            // Parse the response to extract the generated text
            var geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseText);
            string generated = geminiResponse?.candidates != null && geminiResponse.candidates.Length > 0
                ? geminiResponse.candidates[0].content?.parts[0]?.text
                : null;
            onResponse?.Invoke(generated);
        }
        else
        {
            Debug.LogError("Gemini API error: " + request.error);
            onResponse?.Invoke(null);
        }
    }
}

// Helper classes for JSON serialization/deserialization
[System.Serializable]
public class ChatMessage
{
    public string role; // "user" or "model"
    public string content;
}

[System.Serializable]
public class GeminiResponse
{
    public Candidate[] candidates;
}

[System.Serializable]
public class Candidate
{
    public Content content;
}

[System.Serializable]
public class Content
{
    public Part[] parts;
}

[System.Serializable]
public class Part
{
    public string text;
}
