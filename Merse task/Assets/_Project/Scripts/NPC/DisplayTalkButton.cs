using UnityEngine;
using Whisper;
using Whisper.Utils;
using UnityEngine.InputSystem;

public class DisplayTalkButton : MonoBehaviour
{
    [Header("STT References")]
    public WhisperManager whisper;
    public MicrophoneRecord microphoneRecord;
    public GPTManager textGenerator;

    [Header("Input System")]
    public InputActionAsset inputAction; // Assign in Inspector

    private string transcribedText = "";
    private bool hasSpeechBeenDetected = false;
    private InputAction recordAction;

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

        // Setup Input System for Secondary Button
        if (inputAction != null)
        {
            recordAction = inputAction.FindActionMap("Controller").FindAction("Secondary Button");
            recordAction.Enable();

            // Start recording when button is pressed
            recordAction.started += ctx => StartRecording();

            // Stop recording when button is released
            recordAction.canceled += ctx => StopRecording();

            Debug.Log("Input system initialized for Secondary Button");
        }
        else
        {
            Debug.LogError("Input Action Asset not assigned!");
        }
    }

    private void OnDestroy()
    {
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

        // Disable input action
        if (recordAction != null)
        {
            recordAction.Disable();
        }
    }

    // Method to handle Voice Activity Detection changes
    private void OnVadChanged(bool isSpeechDetected)
    {
        // Debug.Log("VAD change: Speech detected = " + isSpeechDetected);

        if (isSpeechDetected)
        {
            hasSpeechBeenDetected = true;
        }
    }

    // Show talk button when player is near
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (transform.childCount > 0)
            {
                transform.GetChild(0).gameObject.SetActive(true);
            }
        }
    }

    // Hide talk button when player moves away
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (transform.childCount > 0)
            {
                transform.GetChild(0).gameObject.SetActive(false);
            }

            // Make sure recording stops if player walks away while recording
            if (microphoneRecord != null && microphoneRecord.IsRecording)
            {
                StopRecording();
            }
        }
    }

    // Method to start recording
    private void StartRecording()
    {
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
            Debug.Log("Started recording...");

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
            Debug.Log("Stopped recording");
        }
    }

    // Callback for when recording is stopped
    private async void OnRecordStop(AudioChunk recordedAudio)
    {
        if (whisper != null && hasSpeechBeenDetected)
        {
            Debug.Log("Processing speech to text...");

            // Get text from the recorded audio
            var result = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);

            if (result != null && !string.IsNullOrWhiteSpace(result.Result))
            {
                Debug.Log("Transcription result: " + result.Result);

                // Set the transcribed text to GPTManager's input field if available
                if (textGenerator != null && textGenerator.inputField != null)
                {
                    textGenerator.TrySendInput(result.Result);
                }
            }
            else
            {
                Debug.LogWarning("No transcription result or empty result.");
            }
        }
        else if (!hasSpeechBeenDetected)
        {
            Debug.Log("No speech detected during recording");
        }
    }

    // Callback for when a new segment is recognized
    private void OnNewSegment(WhisperSegment segment)
    {
        transcribedText += segment.Text;
        // Debug.Log("Current transcription: " + transcribedText);
    }
}
