using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.InputSystem; // For the new Input System

public class GPTManager : MonoBehaviour
{
    [Header("Google Gemini API Key (for testing only, do not use in future)")]
    public string geminiApiKey = "";

    [Header("UI References")]
    public TMP_InputField inputField; // Assign in Inspector
    public TMP_Text responseText;     // Assign in Inspector
    [TextArea(2, 5)]
    public string systemMessage; // Set in Inspector

    [Header("Input System")]
    public InputActionAsset inputAction; // Assign in Inspector
    private InputAction advanceAction;

    private List<ChatMessage> conversationHistory = new List<ChatMessage>();

    // Sentence-by-sentence display fields
    private List<string> currentSentences = new List<string>();
    private int currentSentenceIndex = 0;
    private bool awaitingUserAdvance = false;

    void Start()
    {
        inputField.onSubmit.AddListener(OnInputSubmit);
        // Setup Input System action for advancing dialogue
        advanceAction = inputAction.FindActionMap("Controller").FindAction("Primary Button"); // Use Menu for debug
        advanceAction.Enable();
        advanceAction.performed += OnAdvancePerformed;
    }

    void OnDestroy()
    {
        advanceAction.performed -= OnAdvancePerformed;
    }

    private void OnAdvancePerformed(InputAction.CallbackContext ctx)
    {
        if (awaitingUserAdvance)
        {
            ShowNextSentence();
        }
    }

    void Update()
    {
        // Optionally, you can add a keyboard fallback for testing:
        if (awaitingUserAdvance && Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
        {
            Debug.Log("Enter key pressed: advancing dialogue");
            ShowNextSentence();
        }
    }

    void OnInputSubmit(string userInput)
    {
        TrySendInput(userInput);
    }

    public void TrySendInput(string userInput)
    {
        if (!string.IsNullOrEmpty(userInput))
        {
            conversationHistory.Add(new ChatMessage { role = "user", content = userInput });
            responseText.text = "Thinking...";

            // Find the NPC GameObject by traversing up from the inputField
            Transform t = inputField.transform;
            NPCInstruction npcInstructionComponent = null;
            for (int i = 0; i < 3 && t != null; i++)
                t = t.parent;
            if (t != null)
                npcInstructionComponent = t.GetComponent<NPCInstruction>();
            string npcInstruction = npcInstructionComponent != null ? npcInstructionComponent.npcInstruction : null;

            RequestGeminiResponse(userInput, conversationHistory, OnGeminiResponse, npcInstruction);
            inputField.text = ""; // Clear input
        }
    }

    void OnGeminiResponse(string response)
    {
        conversationHistory.Add(new ChatMessage { role = "model", content = response });

        if (string.IsNullOrEmpty(response))
        {
            responseText.text = "Error getting response.";
            awaitingUserAdvance = false;
            return;
        }

        // Split response into sentences
        currentSentences = SplitIntoSentences(response);
        currentSentenceIndex = 0;
        awaitingUserAdvance = true;

        if (currentSentences.Count > 0)
            responseText.text = currentSentences[0];
        else
            responseText.text = "";
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
        currentSentenceIndex++;
        if (currentSentenceIndex < currentSentences.Count)
        {
            responseText.text = currentSentences[currentSentenceIndex];
        }
        else
        {
            responseText.text = "";
            awaitingUserAdvance = false;
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
                combinedSystemMessage += "\n" + npcInstruction;
            else
                combinedSystemMessage = npcInstruction;
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

    // Helper to send a message using the NPC's instruction
    public void RequestGeminiResponseWithNPC(GameObject npc, string message, List<ChatMessage> conversationHistory, System.Action<string> onResponse)
    {
        var npcInstructionComponent = npc.GetComponent<NPCInstruction>();
        string npcInstruction = npcInstructionComponent != null ? npcInstructionComponent.npcInstruction : null;
        RequestGeminiResponse(message, conversationHistory, onResponse, npcInstruction);
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
