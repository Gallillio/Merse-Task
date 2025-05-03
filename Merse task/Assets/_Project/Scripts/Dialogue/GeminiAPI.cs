using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Core.Interfaces;

namespace Dialogue
{
    /// <summary>
    /// Handles communication with Google's Gemini API
    /// </summary>
    public class GeminiAPI
    {
        private readonly string apiKey;
        private readonly ILoggingService logger;

        /// <summary>
        /// Initialize a new instance of the GeminiAPI class
        /// </summary>
        /// <param name="apiKey">The API key for Gemini</param>
        /// <param name="logger">Logging service</param>
        public GeminiAPI(string apiKey, ILoggingService logger = null)
        {
            this.apiKey = apiKey;
            this.logger = logger;
        }

        /// <summary>
        /// Generate a response from Gemini asynchronously
        /// </summary>
        /// <param name="userInput">The user's input</param>
        /// <param name="history">Conversation history</param>
        /// <param name="systemInstruction">System instruction for the model</param>
        /// <returns>The generated response</returns>
        public async Task<string> GenerateResponseAsync(string userInput, List<ChatMessage> history, string systemInstruction)
        {
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}";

            var contents = new List<object>();

            // Handle system message/instruction
            string combinedSystemMessage = systemInstruction;

            // Add conversation history, prepending system message to the first user message
            bool systemPrepended = false;
            if (history != null)
            {
                foreach (var msg in history)
                {
                    string text = msg.Content;
                    if (!systemPrepended && !string.IsNullOrEmpty(combinedSystemMessage) && msg.Role == "user")
                    {
                        text = combinedSystemMessage + "\n" + text;
                        systemPrepended = true;
                    }
                    contents.Add(new
                    {
                        role = msg.Role.ToLower(),
                        parts = new object[] { new { text = text } }
                    });
                }
            }

            // Add the new message
            string newMessageText = userInput;
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
            logger?.Log($"Gemini API Request Payload: {jsonBody}");

            // Create web request
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                // Send request
                UnityWebRequestAsyncOperation operation = request.SendWebRequest();

                // Wait for completion using Task instead of coroutine
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                string responseText = request.downloadHandler.text;
                logger?.Log($"Gemini API Response: {responseText}");

                if (request.result == UnityWebRequest.Result.Success)
                {
                    // Parse response
                    var geminiResponse = JsonConvert.DeserializeObject<GeminiResponse>(responseText);
                    string generated = geminiResponse?.Candidates != null && geminiResponse.Candidates.Length > 0
                        ? geminiResponse.Candidates[0].Content?.Parts[0]?.Text
                        : null;

                    return generated;
                }
                else
                {
                    logger?.LogError("Gemini API error: " + request.error);
                    return null;
                }
            }
        }
    }

    /// <summary>
    /// Represents a message in a conversation
    /// </summary>
    [System.Serializable]
    public class ChatMessage
    {
        /// <summary>
        /// The role of the message sender (e.g., "user" or "model")
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// The content of the message
        /// </summary>
        public string Content { get; set; }
    }

    /// <summary>
    /// Helper classes for Gemini API JSON deserialization
    /// </summary>
    [System.Serializable]
    internal class GeminiResponse
    {
        [JsonProperty("candidates")]
        public GeminiCandidate[] Candidates { get; set; }
    }

    [System.Serializable]
    internal class GeminiCandidate
    {
        [JsonProperty("content")]
        public GeminiContent Content { get; set; }
    }

    [System.Serializable]
    internal class GeminiContent
    {
        [JsonProperty("parts")]
        public GeminiPart[] Parts { get; set; }
    }

    [System.Serializable]
    internal class GeminiPart
    {
        [JsonProperty("text")]
        public string Text { get; set; }
    }
}