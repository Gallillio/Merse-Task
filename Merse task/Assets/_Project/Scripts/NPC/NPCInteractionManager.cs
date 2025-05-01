using UnityEngine;
using Whisper;
using Whisper.Utils;
using UnityEngine.InputSystem;

public class NPCInteractionManager : MonoBehaviour
{
    // Static reference to track which NPC is currently active
    private static NPCInteractionManager activeNPC = null;

    [Header("STT References")]
    public WhisperManager whisper;
    public MicrophoneRecord microphoneRecord;
    public GPTManager textGenerator;

    [Header("Input System")]
    public InputActionAsset inputAction; // Assign in Inspector

    private string transcribedText = "";
    private bool hasSpeechBeenDetected = false;
    private InputAction recordAction;
    private bool playerInTriggerArea = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Hide the first child by default (if it exists)
        if (transform.childCount > 0)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }

        // Setup microphone and whisper events
        if (microphoneRecord != null)
        {
            microphoneRecord.OnRecordStop += OnRecordStop;
            microphoneRecord.OnVadChanged += OnVadChanged;
        }

        if (whisper != null)
        {
            whisper.OnNewSegment += OnNewSegment;
        }

        // Find GPTManager if not assigned
        if (textGenerator == null)
        {
            textGenerator = FindObjectOfType<GPTManager>();
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
        if (microphoneRecord != null)
        {
            microphoneRecord.OnRecordStop -= OnRecordStop;
            microphoneRecord.OnVadChanged -= OnVadChanged;
        }

        if (whisper != null)
        {
            whisper.OnNewSegment -= OnNewSegment;
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

            // Show talk indicator
            if (transform.childCount > 0)
            {
                transform.GetChild(0).gameObject.SetActive(true);
            }

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

        // Hide talk indicator
        if (transform.childCount > 0)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }

        // Make sure recording stops if player walks away while recording
        if (microphoneRecord != null && microphoneRecord.IsRecording)
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
        if (textGenerator != null && transform.parent != null)
        {
            textGenerator.ClearConversationHistoryForNPC(transform.parent.gameObject);
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

        if (microphoneRecord != null && !microphoneRecord.IsRecording)
        {
            // Reset transcribed text and speech detection flag
            transcribedText = "";
            hasSpeechBeenDetected = false;

            // Configure microphone for VAD
            microphoneRecord.useVad = true;       // Enable Voice Activity Detection
            microphoneRecord.vadStop = false;     // Don't auto-stop
            microphoneRecord.dropVadPart = true;  // Drop the silent part at the end

            // Start recording
            microphoneRecord.StartRecord();
            Debug.Log($"Started recording for {transform.parent.name}...");

            // Visual feedback - optional
            if (transform.childCount > 0)
            {
                // Change color or add some indicator that recording is active
            }
        }
    }

    // Method to stop recording
    private void StopRecording()
    {
        if (microphoneRecord != null && microphoneRecord.IsRecording)
        {
            microphoneRecord.StopRecord();
            Debug.Log($"Stopped recording for {transform.parent.name}");
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

        if (whisper != null && hasSpeechBeenDetected)
        {
            Debug.Log($"Processing speech to text for {transform.parent.name}...");

            // Get text from the recorded audio
            var result = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);

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
                if (textGenerator != null)
                {
                    // Pass the NPC GameObject with the NPCInstruction component
                    textGenerator.TrySendInput(result.Result, npcObject);
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
}
