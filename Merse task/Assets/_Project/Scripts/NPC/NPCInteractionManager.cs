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

    private string transcribedText = "";
    private bool hasSpeechBeenDetected = false;
    private InputAction recordAction;
    private bool playerInTriggerArea = false;

    // Reference to the listening feedback icon
    private GameObject activeListeningIcon;

    // Reference to the spatial panel model
    private GameObject spatialPanelModel;

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

            // Note: Spatial panel now remains hidden until response is received
            // We'll enable it via ShowSpatialPanel method

            // Enable recording input
            if (recordAction != null && !recordAction.enabled)
            {
                recordAction.Enable();
                Debug.Log($"Recording input enabled for {transform.parent.name} - player can now hold Secondary Button to talk");
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
            microphoneRecordManager.StopRecord();
            Debug.Log($"Stopped recording for {transform.parent.name}");

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

        if (whisperManager != null && hasSpeechBeenDetected)
        {
            Debug.Log($"Processing speech to text for {transform.parent.name}...");

            // Get text from the recorded audio
            var result = await whisperManager.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);

            if (result != null && !string.IsNullOrWhiteSpace(result.Result))
            {
                Debug.Log($"Transcription result for {transform.parent.name}: " + result.Result);

                // Get the NPC GameObject (parent of this GameObject)
                GameObject npcObject = transform.parent ? transform.parent.gameObject : null;

                // Get the NPC instructions
                NPCInstruction npcInstructionComponent = npcObject?.GetComponent<NPCInstruction>();
                string npcInstruction = npcInstructionComponent?.npcInstruction;

                // Debug.Log($"NPC Instructions for {transform.parent.name}: " +
                //     (string.IsNullOrEmpty(npcInstruction) ? "None" : npcInstruction));

                // Send the transcribed text to GPTManager
                if (gptManager != null)
                {
                    // Pass the NPC GameObject with the NPCInstruction component
                    gptManager.TrySendInput(result.Result, npcObject);
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
            Debug.Log($"Spatial panel activated for {transform.parent?.name}");
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
}
