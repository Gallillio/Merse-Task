using UnityEngine;
using Whisper;
using Whisper.Utils;
using UnityEngine.InputSystem;

public class NPCInteractionManager : MonoBehaviour
{
    // Static reference to track which NPC is currently active
    private static NPCInteractionManager activeNPC = null;

    [Header("STT References")]
    private WhisperManager whisperManager;
    private MicrophoneRecord microphoneRecordManager;
    private GPTManager gptManager;

    [Header("Input System")]
    public InputActionAsset inputAction; // Assign in Inspector

    [Header("Recording Settings")]
    private float minimumRecordingDuration = 0.3f;

    private string transcribedText = "";
    private bool hasSpeechBeenDetected = false;
    private InputAction recordAction;
    private bool playerInTriggerArea = false;

    // Reference to the listening feedback icon
    private GameObject activeListeningIcon;

    // Reference to the spatial panel model
    private GameObject spatialPanelModel;

    // Recording variables
    private float recordingStartTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Hide the first child by default (if it exists)
        if (transform.childCount > 0)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }

        // Find the Actively Listening Feedback Icon sibling
        activeListeningIcon = transform.parent.Find("_Actively Listening Feedback Icon")?.gameObject;
        if (activeListeningIcon != null)
        {
            activeListeningIcon.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Could not find '_Actively Listening Feedback Icon' sibling GameObject");
        }

        // Find the Spatial Panel Manipulator Model child
        spatialPanelModel = transform.Find("_Spatial Panel Manipulator Model")?.gameObject;
        if (spatialPanelModel != null)
        {
            spatialPanelModel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Could not find '_Spatial Panel Manipulator Model' child GameObject");
        }

        // Find managers if not assigned
        if (whisperManager == null)
        {
            GameObject whisperObj = GameObject.Find("_Whisper Manager");
            if (whisperObj != null)
            {
                whisperManager = whisperObj.GetComponent<WhisperManager>();
            }
            else
            {
                Debug.LogWarning("Could not find '_Whisper Manager' GameObject");
            }
        }

        if (microphoneRecordManager == null)
        {
            GameObject microphoneObj = GameObject.Find("_Microphone Record Manager");
            if (microphoneObj != null)
            {
                microphoneRecordManager = microphoneObj.GetComponent<MicrophoneRecord>();
            }
            else
            {
                Debug.LogWarning("Could not find '_Microphone Record Manager' GameObject");
            }
        }

        if (gptManager == null)
        {
            GameObject gptObj = GameObject.Find("_GPT Manager");
            if (gptObj != null)
            {
                gptManager = gptObj.GetComponent<GPTManager>();
            }
            else
            {
                Debug.LogWarning("Could not find '_GPT Manager' GameObject");
                gptManager = FindObjectOfType<GPTManager>();

                if (gptManager == null)
                {
                    Debug.LogError("GPT Manager reference not found!");
                }
            }
        }

        // Setup microphone and whisperManager events
        if (microphoneRecordManager != null)
        {
            microphoneRecordManager.OnRecordStop += OnRecordStop;
            microphoneRecordManager.OnVadChanged += OnVadChanged;
        }
        else
        {
            Debug.LogError("Microphone Record Manager reference not found!");
        }

        if (whisperManager != null)
        {
            whisperManager.OnNewSegment += OnNewSegment;
        }
        else
        {
            Debug.LogError("Whisper Manager reference not found!");
        }

        // Setup Input System for Secondary Button (but don't enable it yet)
        if (inputAction != null)
        {
            recordAction = inputAction.FindActionMap("Controller").FindAction("Secondary Button");

            // Don't enable yet - only when player enters trigger

            // Set up callbacks
            recordAction.started += ctx => OnRecordButtonPressed();
            recordAction.canceled += ctx => OnRecordButtonReleased();
        }
        else
        {
            Debug.LogError("Input Action Asset not assigned!");
        }
    }

    private void OnDestroy()
    {
        // If this is the active NPC, clear the reference
        if (activeNPC == this)
        {
            activeNPC = null;
        }

        // Clean up event listeners
        if (microphoneRecordManager != null)
        {
            microphoneRecordManager.OnRecordStop -= OnRecordStop;
            microphoneRecordManager.OnVadChanged -= OnVadChanged;
        }

        if (whisperManager != null)
        {
            whisperManager.OnNewSegment -= OnNewSegment;
        }

        // Disable input action if it was enabled
        if (recordAction != null && recordAction.enabled)
        {
            recordAction.Disable();
        }
    }

    // Method to handle Voice Activity Detection changes
    private void OnVadChanged(bool isSpeechDetected)
    {
        if (isSpeechDetected)
        {
            hasSpeechBeenDetected = true;
        }
    }

    // Show talk button when player is near and enable recording
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // If another NPC is active, disable it
            if (activeNPC != null && activeNPC != this)
            {
                activeNPC.DeactivateNPC();
            }

            // Set this as the active NPC
            activeNPC = this;
            playerInTriggerArea = true;

            // Get NPC instruction component
            NPCInstruction npcInstruction = transform.parent?.GetComponent<NPCInstruction>();
            if (npcInstruction != null && npcInstruction.hasQuest)
            {
                string npcName = transform.parent?.name ?? "Unknown NPC";

                // Check for quest items if this is not the first interaction
                if (npcInstruction.questActive && !npcInstruction.questCompleted)
                {
                    // Check if player has the quest item
                    bool hasQuestItem = QuestManager.Instance.HasItem(npcInstruction.questItemName);
                    if (hasQuestItem)
                    {
                        // Mark quest as completed
                        npcInstruction.questCompleted = true;
                        Debug.Log($"[QUEST STATE] Player is returning to {npcName} WITH the requested item '{npcInstruction.questItemName}'. Quest COMPLETED!");

                        // Auto-initiate conversation with completed quest prompt
                        StartAutomaticConversation(npcInstruction);
                    }
                    else
                    {
                        Debug.Log($"[QUEST STATE] Player is returning to {npcName} WITHOUT the requested item '{npcInstruction.questItemName}'. Quest still IN PROGRESS.");

                        // Auto-initiate conversation with in-progress prompt
                        StartAutomaticConversation(npcInstruction);
                    }
                }
                else if (!npcInstruction.questActive)
                {
                    // First interaction - but don't mark as active yet
                    // We'll set questActive after first conversation completes
                    Debug.Log($"[QUEST STATE] Player is meeting {npcName} for the FIRST TIME. Will activate quest for item '{npcInstruction.questItemName}' after conversation.");

                    // For first interaction, the player must initiate the conversation
                    // No automatic conversation start here
                }
                else if (npcInstruction.questCompleted)
                {
                    Debug.Log($"[QUEST STATE] Player is returning to {npcName} with a COMPLETED quest for '{npcInstruction.questItemName}'.");

                    // Auto-initiate conversation with completed quest prompt
                    StartAutomaticConversation(npcInstruction);
                }
            }
            else if (npcInstruction != null)
            {
                Debug.Log($"[QUEST STATE] NPC {transform.parent?.name ?? "Unknown"} doesn't have a quest configured.");
            }

            // Show spatial panel when player enters trigger area
            ShowSpatialPanel();

            // Enable recording input
            if (recordAction != null && !recordAction.enabled)
            {
                recordAction.Enable();
                Debug.Log($"Recording input enabled for {transform.parent.name}");
            }
        }
    }

    // Hide talk button when player moves away and disable recording
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DeactivateNPC();
        }
    }

    // Method to deactivate this NPC
    private void DeactivateNPC()
    {
        playerInTriggerArea = false;

        // If this is the active NPC, clear the reference
        if (activeNPC == this)
        {
            activeNPC = null;
        }

        // Hide spatial panel
        HideSpatialPanel();

        // Make sure recording stops if player walks away while recording
        if (microphoneRecordManager != null && microphoneRecordManager.IsRecording)
        {
            StopRecording();
        }

        // Disable recording input
        if (recordAction != null && recordAction.enabled)
        {
            recordAction.Disable();
            // Debug.Log($"Recording input disabled for {transform.parent.name} - player left NPC area");
        }

        // Clear the conversation history when player leaves
        if (gptManager != null && transform.parent != null)
        {
            gptManager.ClearConversationHistoryForNPC(transform.parent.gameObject);
            // Debug.Log($"Cleared conversation history for {transform.parent.name}");
        }
    }

    // Callback for when record button is pressed
    private void OnRecordButtonPressed()
    {
        // Only start recording if this is the active NPC
        if (activeNPC == this)
        {
            StartRecording();
        }
    }

    // Callback for when record button is released
    private void OnRecordButtonReleased()
    {
        // Only stop recording if this is the active NPC
        if (activeNPC == this)
        {
            StopRecording();
        }
    }

    // Method to start recording
    private void StartRecording()
    {
        // Double-check player is still in trigger area
        if (!playerInTriggerArea)
        {
            Debug.LogWarning("Tried to start recording but player is not in trigger area");
            return;
        }

        if (microphoneRecordManager != null && !microphoneRecordManager.IsRecording)
        {
            // Reset transcribed text and speech detection flag
            transcribedText = "";
            hasSpeechBeenDetected = false;

            // Configure microphone for VAD
            microphoneRecordManager.useVad = true;       // Enable Voice Activity Detection
            microphoneRecordManager.vadStop = false;     // Don't auto-stop
            microphoneRecordManager.dropVadPart = true;  // Drop the silent part at the end

            // Start recording
            microphoneRecordManager.StartRecord();
            recordingStartTime = Time.time;
            Debug.Log($"Started recording for {transform.parent.name}...");

            // Enable the actively listening icon
            if (activeListeningIcon != null)
            {
                activeListeningIcon.SetActive(true);
            }
        }
    }

    // Method to stop recording
    private void StopRecording()
    {
        if (microphoneRecordManager != null && microphoneRecordManager.IsRecording)
        {
            // Check if recording time is too short (less than minimum duration)
            float recordingDuration = Time.time - recordingStartTime;
            if (recordingDuration < minimumRecordingDuration)
            {
                Debug.Log($"Recording too short ({recordingDuration:F2}s), ignoring this recording");

                // Use a try-catch to handle potential errors for short recordings
                try
                {
                    // Just stop the recording - we will handle the empty recording in the OnRecordStop callback
                    microphoneRecordManager.StopRecord();
                }
                catch (System.ArgumentException ex)
                {
                    Debug.LogWarning($"Expected error for short recording: {ex.Message}");
                    // No need to do anything else, the recording was too short
                }
            }
            else
            {
                // Recording is long enough, proceed with stop
                microphoneRecordManager.StopRecord();
                Debug.Log($"Stopped recording for {transform.parent.name} (duration: {recordingDuration:F2}s)");
            }

            // Disable the actively listening icon
            if (activeListeningIcon != null)
            {
                activeListeningIcon.SetActive(false);
            }
        }
    }

    // Callback for when recording is stopped
    private async void OnRecordStop(AudioChunk recordedAudio)
    {
        // Only process if this is the active NPC
        if (activeNPC != this)
        {
            Debug.Log($"Ignoring recording from inactive NPC {transform.parent.name}");
            return;
        }

        // Check for empty or very short recordings
        if (recordedAudio.Data == null || recordedAudio.Data.Length == 0)
        {
            Debug.Log($"Empty recording received, nothing to process.");
            return;
        }

        // Check for short recording that might still have data but likely isn't useful
        float recordingDuration = Time.time - recordingStartTime;
        if (recordingDuration < minimumRecordingDuration)
        {
            Debug.Log($"Recording too short ({recordingDuration:F2}s), ignoring.");
            return;
        }

        if (whisperManager != null && hasSpeechBeenDetected)
        {
            // Debug.Log($"Processing speech to text for {transform.parent.name}: " + result.Result);

            // Get text from the recorded audio
            var result = await whisperManager.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);

            if (result != null && !string.IsNullOrWhiteSpace(result.Result))
            {
                // Debug.Log($"Transcription result for {transform.parent.name}: " + result.Result);

                // Get the NPC GameObject (parent of this GameObject)
                GameObject npcObject = transform.parent ? transform.parent.gameObject : null;

                // Get the NPC instructions
                NPCInstruction npcInstructionComponent = npcObject?.GetComponent<NPCInstruction>();

                if (npcInstructionComponent != null)
                {
                    // Special handling for first conversation
                    bool isFirstConversation = npcInstructionComponent.hasQuest && !npcInstructionComponent.questActive;

                    // Use the appropriate instruction based on quest state
                    string currentInstruction = npcInstructionComponent.GetCurrentInstruction();
                    Debug.Log($"Using instruction for {transform.parent.name}: {currentInstruction}");

                    // Send the transcribed text to GPTManager
                    if (gptManager != null)
                    {
                        // Mark this as a user-initiated conversation (not automatic)
                        bool isUserInitiated = true;

                        // Pass the NPC GameObject with the NPCInstruction component
                        gptManager.TrySendInput(result.Result, npcObject, currentInstruction);

                        // After first conversation, mark quest as active for next time
                        if (isFirstConversation)
                        {
                            npcInstructionComponent.questActive = true;
                            Debug.Log($"[QUEST STATE] Activated quest for {transform.parent.name} after first conversation");
                        }
                    }
                }
                else
                {
                    Debug.LogError($"NPCInstruction component not found on {npcObject?.name}");
                }
            }
            else
            {
                Debug.LogWarning($"No transcription result for {transform.parent.name}");
            }
        }
        else if (!hasSpeechBeenDetected)
        {
            Debug.Log($"No speech detected during recording for {transform.parent.name}");
        }
    }

    // Callback for when a new segment is recognized
    private void OnNewSegment(WhisperSegment segment)
    {
        transcribedText += segment.Text;
    }

    // Public method to show the spatial panel model
    public void ShowSpatialPanel()
    {
        if (spatialPanelModel != null)
        {
            spatialPanelModel.SetActive(true);
            // Debug.Log($"Spatial panel activated for {transform.parent?.name}");
        }
        else
        {
            Debug.LogWarning($"Cannot show spatial panel: _Spatial Panel Manipulator Model reference is null");
        }
    }

    // Public method to hide the spatial panel model
    public void HideSpatialPanel()
    {
        if (spatialPanelModel != null)
        {
            spatialPanelModel.SetActive(false);
        }
    }

    // Helper method to automatically start a conversation with the NPC
    private void StartAutomaticConversation(NPCInstruction npcInstruction)
    {
        if (gptManager == null)
        {
            Debug.LogError("Cannot auto-start conversation: GPT Manager is null");
            return;
        }

        // Get the parent NPC GameObject
        GameObject npcObject = transform.parent?.gameObject;
        if (npcObject == null)
        {
            Debug.LogError("Cannot auto-start conversation: NPC parent object is null");
            return;
        }

        // Get the appropriate instruction based on quest state
        string currentInstruction = npcInstruction.GetCurrentInstruction();
        Debug.Log($"[AUTO CONVERSATION] Starting automatic conversation for {npcObject.name} with: {currentInstruction}");

        // Use an empty greeting as the user input since the NPC is initiating
        string autoGreeting = ""; // Empty input since NPC is initiating

        // Send to GPT Manager
        gptManager.TrySendInput(autoGreeting, npcObject, currentInstruction);
    }
}
