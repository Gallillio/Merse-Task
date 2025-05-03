using UnityEngine;
using UnityEngine.InputSystem;
using Whisper;
using Whisper.Utils;
using System;
using System.Threading.Tasks;
using Core.Interfaces;
using Core.Services;
using Quest;

/// <summary>
/// Main controller for NPC interactions, managing dialogue, quests, and player input.
/// Replaces the previous NPCInteractionManager with a SOLID-compliant implementation.
/// </summary>
[RequireComponent(typeof(NPCInstructionUI))]
public class NPCInteractionController : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputAction;
    [SerializeField] private float minimumRecordingDuration = 0.3f;

    private IAudioService audioService;
    private IDialogueProvider dialogueProvider;
    private IQuestService questService;
    private ILoggingService loggingService;

    private NPCQuestState questState;
    private NPCInstructionUI instructionUI;
    private InputAction recordAction;
    private bool playerInTriggerArea = false;

    private WhisperManager whisperManager;
    private MicrophoneRecord microphoneRecordManager;

    private string transcribedText = "";
    private bool hasSpeechBeenDetected = false;
    private float recordingStartTime = 0f;

    // Static reference to track which NPC is currently active
    private static NPCInteractionController activeNPC = null;

    private void Awake()
    {
        // Get services
        audioService = ServiceLocator.Get<IAudioService>();
        dialogueProvider = ServiceLocator.Get<IDialogueProvider>();
        questService = ServiceLocator.Get<IQuestService>();
        loggingService = ServiceLocator.Get<ILoggingService>();

        // Get components
        questState = GetComponent<NPCQuestState>();
        instructionUI = GetComponent<NPCInstructionUI>();

        if (questState == null)
        {
            loggingService.LogWarning("NPCQuestState component not found on NPC");
        }

        // Find required managers
        FindRequiredManagers();

        // Setup input
        SetupInputActions();

        // Setup microphone and whisper events
        SetupSpeechRecognition();
    }

    private void FindRequiredManagers()
    {
        // Find WhisperManager if not assigned
        if (whisperManager == null)
        {
            GameObject whisperObj = GameObject.Find("_Whisper Manager");
            if (whisperObj != null)
            {
                whisperManager = whisperObj.GetComponent<WhisperManager>();
            }
            else
            {
                loggingService.LogWarning("Could not find '_Whisper Manager' GameObject");
            }
        }

        // Find MicrophoneRecord if not assigned
        if (microphoneRecordManager == null)
        {
            GameObject microphoneObj = GameObject.Find("_Microphone Record Manager");
            if (microphoneObj != null)
            {
                microphoneRecordManager = microphoneObj.GetComponent<MicrophoneRecord>();
            }
            else
            {
                loggingService.LogWarning("Could not find '_Microphone Record Manager' GameObject");
            }
        }
    }

    private void SetupInputActions()
    {
        if (inputAction != null)
        {
            recordAction = inputAction.FindActionMap("Controller").FindAction("Secondary Button");

            // Set up callbacks
            recordAction.started += ctx => OnRecordButtonPressed();
            recordAction.canceled += ctx => OnRecordButtonReleased();
        }
        else
        {
            loggingService.LogError("Input Action Asset not assigned!");
        }
    }

    private void SetupSpeechRecognition()
    {
        if (microphoneRecordManager != null)
        {
            microphoneRecordManager.OnRecordStop += OnRecordStop;
            microphoneRecordManager.OnVadChanged += OnVadChanged;
        }
        else
        {
            loggingService.LogError("Microphone Record Manager reference not found!");
        }

        if (whisperManager != null)
        {
            whisperManager.OnNewSegment += OnNewSegment;
        }
        else
        {
            loggingService.LogError("Whisper Manager reference not found!");
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

    public void OnPlayerEntered()
    {
        // If another NPC is active, deactivate it
        if (activeNPC != null && activeNPC != this)
        {
            activeNPC.DeactivateNPC();
        }

        // Set this as the active NPC
        activeNPC = this;
        playerInTriggerArea = true;

        // Show UI
        instructionUI.ShowSpatialPanel();

        // Enable input
        if (recordAction != null && !recordAction.enabled)
        {
            recordAction.Enable();
        }

        // Handle quest state
        if (questState != null && questState.hasQuest)
        {
            HandleQuestState();
        }
    }

    public void OnPlayerExited()
    {
        playerInTriggerArea = false;

        // Hide UI
        instructionUI.HideSpatialPanel();
        instructionUI.HideListeningIcon();

        // Disable input
        if (recordAction != null && recordAction.enabled)
        {
            recordAction.Disable();
        }

        // Stop audio
        audioService.StopNPCVoice();
        audioService.EndConversation();

        // If this is the active NPC, clear the reference
        if (activeNPC == this)
        {
            activeNPC = null;
        }
    }

    private void DeactivateNPC()
    {
        // Hide UI
        instructionUI.HideSpatialPanel();
        instructionUI.HideListeningIcon();

        // Disable input
        if (recordAction != null && recordAction.enabled)
        {
            recordAction.Disable();
        }

        // Stop audio
        audioService.StopNPCVoice();
        audioService.EndConversation();

        playerInTriggerArea = false;
    }

    private void HandleQuestState()
    {
        if (questState.questActive && !questState.questCompleted)
        {
            // Check if player has the quest item
            bool hasQuestItem = questService.HasItem(questState.questItemName);
            if (hasQuestItem)
            {
                // Mark quest as completed
                questState.questCompleted = true;
                loggingService.Log($"Player is returning with requested item '{questState.questItemName}'. Quest completed!");

                // Find and activate reward object
                ActivateQuestRewardObject(questState.questItemName);

                // Remove item from inventory
                GameObject removedItem = questService.RemoveItem(questState.questItemName);
                if (removedItem != null)
                {
                    loggingService.Log($"Removed quest item '{questState.questItemName}' from inventory");
                }

                // Auto-start conversation
                StartAutomaticConversation();
            }
            else
            {
                loggingService.Log($"Player is returning without the requested item '{questState.questItemName}'. Quest still in progress.");
                StartAutomaticConversation();
            }
        }
        else if (!questState.questActive)
        {
            // First interaction - player must initiate
            loggingService.Log($"First interaction with NPC. Quest for '{questState.questItemName}' will activate after conversation.");
            // No automatic conversation here
        }
        else if (questState.questCompleted)
        {
            loggingService.Log($"Player is returning to NPC with a completed quest for '{questState.questItemName}'.");
            StartAutomaticConversation();
        }
    }

    private void StartAutomaticConversation()
    {
        if (questState != null)
        {
            string currentPrompt = questState.GetCurrentPrompt();

            if (!string.IsNullOrEmpty(currentPrompt))
            {
                dialogueProvider.StartDialogue(gameObject, currentPrompt, "You are a helpful assistant", response =>
                {
                    loggingService.Log("Automatic conversation response received");

                    // If this is the first interaction, activate the quest after conversation
                    if (questState.hasQuest && !questState.questActive && !questState.questCompleted)
                    {
                        questState.questActive = true;
                        loggingService.Log($"Quest for '{questState.questItemName}' has been activated");
                    }
                });
            }
        }
    }

    private void OnRecordButtonPressed()
    {
        if (playerInTriggerArea)
        {
            StartRecording();
        }
    }

    private void OnRecordButtonReleased()
    {
        if (playerInTriggerArea)
        {
            StopRecording();
        }
    }

    private void StartRecording()
    {
        // Show the listening icon
        instructionUI.ShowListeningIcon();

        // Reset variables
        transcribedText = "";
        hasSpeechBeenDetected = false;
        recordingStartTime = Time.time;

        // Start audio recording
        if (microphoneRecordManager != null)
        {
            microphoneRecordManager.StartRecord();
            audioService.StartConversation();
            loggingService.Log("Started recording audio");
        }
        else
        {
            loggingService.LogError("Cannot start recording - MicrophoneRecord is null");
        }
    }

    private void StopRecording()
    {
        // Hide the listening icon
        instructionUI.HideListeningIcon();

        float recordingDuration = Time.time - recordingStartTime;

        // Only process if recording meets minimum duration
        if (recordingDuration >= minimumRecordingDuration)
        {
            if (microphoneRecordManager != null)
            {
                microphoneRecordManager.StopRecord();
                loggingService.Log("Stopped recording audio");
            }
            else
            {
                loggingService.LogError("Cannot stop recording - MicrophoneRecord is null");
            }
        }
        else
        {
            loggingService.LogWarning($"Recording too short ({recordingDuration:F2}s), ignoring");
            audioService.EndConversation();
        }
    }

    private void OnVadChanged(bool isSpeechDetected)
    {
        if (isSpeechDetected)
        {
            hasSpeechBeenDetected = true;
        }
    }

    private async void OnRecordStop(AudioChunk recordedAudio)
    {
        // Process the recorded audio chunk
        if (whisperManager != null)
        {
            // Only process if speech was detected
            if (hasSpeechBeenDetected)
            {
                try
                {
                    loggingService.Log("Processing speech with Whisper...");
                    var result = await whisperManager.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
                }
                catch (Exception ex)
                {
                    loggingService.LogError($"Error processing audio: {ex.Message}");
                    audioService.EndConversation();
                }
            }
            else
            {
                loggingService.LogWarning("No speech detected in recording");
                audioService.EndConversation();
            }
        }
        else
        {
            loggingService.LogError("Cannot process audio - WhisperManager is null");
            audioService.EndConversation();
        }
    }

    private void OnNewSegment(WhisperSegment segment)
    {
        // Process the transcribed text segment
        if (!string.IsNullOrWhiteSpace(segment.Text))
        {
            transcribedText = segment.Text.Trim();
            loggingService.Log($"Transcribed: {transcribedText}");

            // Get current prompt from NPC state
            string instructionPrompt = "You are a helpful assistant";
            if (questState != null)
            {
                // Use the appropriate prompt based on quest state
                string currentPrompt = questState.GetCurrentPrompt();

                // Start the dialogue
                dialogueProvider.StartDialogue(gameObject, transcribedText, instructionPrompt, response =>
                {
                    loggingService.Log("Dialogue response received");

                    // If this is the first interaction, activate the quest after conversation
                    if (questState?.hasQuest == true && !questState.questActive && !questState.questCompleted)
                    {
                        questState.questActive = true;
                        loggingService.Log($"Quest for '{questState.questItemName}' has been activated");
                    }
                });
            }
        }
    }

    private void ActivateQuestRewardObject(string questItemName)
    {
        // Find and activate the matching child GameObject on the NPC
        Transform parentTransform = transform.parent;
        if (parentTransform != null)
        {
            foreach (Transform child in parentTransform)
            {
                if (child.name.Contains(questItemName, StringComparison.OrdinalIgnoreCase))
                {
                    child.gameObject.SetActive(true);
                    loggingService.Log($"Activated reward object: {child.name}");
                    break;
                }
            }
        }
    }
}