# SOLID Refactoring Guide

This document provides a step-by-step implementation plan for refactoring our VR interaction system to follow SOLID principles. Follow these steps sequentially to ensure a smooth transition.

## Phase 1: Core Infrastructure ✅

### Step 1.1: Create Folder Structure ✅

- [x] Create `/Scripts/Core/Interfaces` directory
- [x] Create `/Scripts/Core/Services` directory
- [x] Create `/Scripts/NPC/Components` directory
- [x] Create `/Scripts/NPC/Logic` directory
- [x] Create `/Scripts/Dialogue` directory
- [x] Create `/Scripts/Inventory` directory
- [x] Create `/Scripts/Utilities` directory

### Step 1.2: Define Core Interfaces ✅

- [x] Create `IAudioService.cs`:

```csharp
public interface IAudioService
{
    void Play(SoundType type, float volume = 1f, bool loop = false);
    void PlayNPCVoice(float duration, float volume = 1f);
    void StopNPCVoice();
    void StartConversation();
    void EndConversation();
}
```

- [x] Create `IDialogueProvider.cs`:

```csharp
public interface IDialogueProvider
{
    void StartDialogue(GameObject npc, string input, string instruction, Action<string> onResponse);
    void AdvanceDialogue();
    event Action<string> OnSentenceDisplayed;
    event Action OnDialogueCompleted;
}
```

- [x] Create `IQuestService.cs`:

```csharp
public interface IQuestService
{
    bool HasItem(string itemName);
    GameObject RemoveItem(string itemName);
    void RegisterSocket(Transform socket);
    event Action<string> OnQuestCompleted;
}
```

- [x] Create `IInventoryService.cs`:

```csharp
public interface IInventoryService
{
    void AttachItem(Transform item, Transform socket);
    void DetachItem(Transform item);
    void PlayPickupSound(Transform item);
    event Action<Transform, Transform> OnItemAttached;
}
```

- [x] Create `ILoggingService.cs`:

```csharp
public interface ILoggingService
{
    void Log(string message, LogType type = LogType.Info);
    void LogWarning(string message);
    void LogError(string message);
}
```

### Step 1.3: Implement Service Locator ✅

- [x] Create `ServiceLocator.cs`:

```csharp
public static class ServiceLocator
{
    private static Dictionary<Type, object> _services = new Dictionary<Type, object>();

    public static void Register<T>(T service) where T : class
    {
        _services[typeof(T)] = service;
    }

    public static T Get<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out var service))
        {
            return (T)service;
        }

        Debug.LogError($"Service of type {typeof(T)} not registered!");
        return null;
    }
}
```

- [x] Create `ServiceInitializer.cs`:

```csharp
public class ServiceInitializer : MonoBehaviour
{
    private void Awake()
    {
        // Register core services
        ServiceLocator.Register<IAudioService>(FindObjectOfType<AudioService>());
        ServiceLocator.Register<IQuestService>(FindObjectOfType<QuestService>());
        ServiceLocator.Register<IInventoryService>(FindObjectOfType<InventoryService>());
        ServiceLocator.Register<ILoggingService>(new LoggingService());
    }
}
```

## Phase 2: Audio System Refactoring ✅

### Step 2.1: Extract SoundType Enum ✅

- [x] Create `SoundType.cs`:

```csharp
public enum SoundType
{
    BackgroundMusic,
    ItemPickup,
    NPCTalking,
    QuestComplete
}
```

### Step 2.2: Implement AudioService ✅

- [x] Create `AudioService.cs` based on existing SoundManager:

```csharp
[RequireComponent(typeof(AudioSource))]
public class AudioService : MonoBehaviour, IAudioService
{
    [SerializeField] private AudioClip[] soundList;
    [SerializeField, Range(0.1f, 0.9f)] private float musicDuckingAmount = 0.3f;

    private AudioSource primaryAudioSource;
    private AudioSource oneTimeAudioSource;
    private AudioSource npcVoiceAudioSource;

    private float originalBackgroundVolume;
    private bool isInConversation = false;
    private Coroutine npcVoiceCoroutine;

    private void Awake()
    {
        // Initialize audio sources
        primaryAudioSource = GetComponent<AudioSource>();
        oneTimeAudioSource = gameObject.AddComponent<AudioSource>();
        npcVoiceAudioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        // Start background music
        Play(SoundType.BackgroundMusic, 0.7f, true);
    }

    public void Play(SoundType type, float volume = 1f, bool loop = false)
    {
        // Implementation similar to existing SoundManager.PlaySound
    }

    public void PlayNPCVoice(float duration, float volume = 1f)
    {
        // Implementation similar to existing SoundManager.PlayNPCTalkingSound
    }

    public void StopNPCVoice()
    {
        // Stop the NPC voice sound
    }

    public void StartConversation()
    {
        // Lower background music for conversation
    }

    public void EndConversation()
    {
        // Restore background music after conversation
    }

    // Helper coroutines and private methods
}
```

### Step 2.3: Transition from SoundManager ✅

- [x] Create example `ItemSocketInteractor.cs` using IAudioService (replacing PutItemInInventory)
- [x] Replace static SoundManager.PlaySound() calls with IAudioService
- [x] Add proper logging through ILoggingService

## Phase 3: Dialogue System Refactoring ✅

### Step 3.1: Implement SentenceSplitter ✅

- [x] Create `SentenceSplitter.cs`:

```csharp
public class SentenceSplitter
{
    public List<string> Split(string text)
    {
        // Move sentence splitting logic from GPTManager here
    }
}
```

### Step 3.2: Create GeminiAPI Client ✅

- [x] Create `GeminiAPI.cs`:

```csharp
public class GeminiAPI
{
    private string apiKey;

    public GeminiAPI(string apiKey)
    {
        this.apiKey = apiKey;
    }

    public async Task<string> GenerateResponseAsync(string userInput, List<ChatMessage> history, string instruction)
    {
        // Move Gemini API communication from GPTManager here
    }
}
```

### Step 3.3: Implement SentenceDisplayController ✅

- [x] Create `SentenceDisplayController.cs`:

```csharp
public class SentenceDisplayController : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputAction;

    private List<string> currentSentences = new List<string>();
    private int currentSentenceIndex = 0;
    private TMP_Text displayText;
    private bool awaitingUserAdvance = false;
    private IAudioService audioService;

    public event Action<string> OnSentenceDisplayed;
    public event Action OnAllSentencesDisplayed;

    private void Awake()
    {
        audioService = ServiceLocator.Get<IAudioService>();
    }

    public void DisplaySentences(List<string> sentences, TMP_Text targetText)
    {
        // Display sentences one by one
    }

    public void AdvanceToNextSentence()
    {
        // Show next sentence and play voice
    }
}
```

### Step 3.4: Implement GPTDialogueService ✅

- [x] Create `GPTDialogueService.cs`:

```csharp
public class GPTDialogueService : MonoBehaviour, IDialogueProvider
{
    [SerializeField] private string geminiApiKey;
    [SerializeField] private string systemMessage;

    private GeminiAPI geminiAPI;
    private SentenceSplitter splitter = new SentenceSplitter();
    private SentenceDisplayController displayController;
    private Dictionary<string, List<ChatMessage>> npcConversationHistories = new Dictionary<string, List<ChatMessage>>();
    private GameObject currentNpcObject;

    public event Action<string> OnSentenceDisplayed;
    public event Action OnDialogueCompleted;

    private void Awake()
    {
        geminiAPI = new GeminiAPI(geminiApiKey);
        displayController = GetComponent<SentenceDisplayController>();

        // Subscribe to display controller events
        displayController.OnSentenceDisplayed += (sentence) => OnSentenceDisplayed?.Invoke(sentence);
        displayController.OnAllSentencesDisplayed += () => OnDialogueCompleted?.Invoke();
    }

    public void StartDialogue(GameObject npc, string input, string instruction, Action<string> onResponse)
    {
        // Start a dialogue with the NPC
    }

    public void AdvanceDialogue()
    {
        displayController.AdvanceToNextSentence();
    }

    // Helper methods
}
```

## Phase 4: Quest System Refactoring ✅

### Step 4.1: Implement QuestService ✅

- [x] Create `QuestService.cs`:

```csharp
public class QuestService : MonoBehaviour, IQuestService
{
    [SerializeField] private List<Transform> inventorySockets = new List<Transform>();

    public event Action<string> OnQuestCompleted;

    public bool HasItem(string itemName)
    {
        // Check inventory sockets for item
    }

    public GameObject RemoveItem(string itemName)
    {
        // Find and remove item from inventory
    }

    public void RegisterSocket(Transform socket)
    {
        if (!inventorySockets.Contains(socket))
        {
            inventorySockets.Add(socket);
        }
    }
}
```

### Step 4.2: Create NPCQuestState ✅

- [x] Create `NPCQuestState.cs`:

```csharp
public class NPCQuestState : MonoBehaviour
{
    [Header("Quest Settings")]
    public bool hasQuest = false;
    public string questItemName;

    [Header("Quest Prompts")]
    [TextArea(2, 5)]
    public string initialPrompt;
    [TextArea(2, 5)]
    public string questInProgressPrompt;
    [TextArea(2, 5)]
    public string completedQuestPrompt;

    [HideInInspector]
    public bool questActive = false;
    [HideInInspector]
    public bool questCompleted = false;

    public string GetCurrentPrompt()
    {
        // Return the appropriate prompt based on state
    }
}
```

## Phase 5: Inventory System Refactoring ✅

### Step 5.1: Implement InventoryService ✅

- [x] Create `InventoryService.cs`:

```csharp
public class InventoryService : MonoBehaviour, IInventoryService
{
    [SerializeField] private Transform collectablesParent;

    private IAudioService audioService;
    private Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();

    public event Action<Transform, Transform> OnItemAttached;

    private void Awake()
    {
        audioService = ServiceLocator.Get<IAudioService>();

        if (collectablesParent == null)
        {
            collectablesParent = GameObject.Find("Collectables")?.transform;
        }
    }

    public void AttachItem(Transform item, Transform socket)
    {
        // Attach item to socket
        // Store original scale
        // Play sound

        OnItemAttached?.Invoke(item, socket);
    }

    public void DetachItem(Transform item)
    {
        // Return item to collectables parent
        // Restore original scale
    }

    public void PlayPickupSound(Transform item)
    {
        audioService.Play(SoundType.ItemPickup);
    }
}
```

### Step 5.2: Refactor ItemSocketInteractor ✅

- [x] Refactor `ItemSocketInteractor.cs`:

```csharp
public class ItemSocketInteractor : MonoBehaviour
{
    private IInventoryService inventoryService;
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socketInteractor;
    private Transform currentHeldItem;
    private bool wasManuallyGrabbed = false;
    private HashSet<Transform> itemsPlayedSound = new HashSet<Transform>();

    private void Awake()
    {
        inventoryService = ServiceLocator.Get<IInventoryService>();
        socketInteractor = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
    }

    private void OnEnable()
    {
        socketInteractor.selectEntered.AddListener(OnSelectEntered);
        socketInteractor.selectExited.AddListener(OnSelectExited);
    }

    private void OnDisable()
    {
        socketInteractor.selectEntered.RemoveListener(OnSelectEntered);
        socketInteractor.selectExited.RemoveListener(OnSelectExited);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        // Implementation based on PutItemInInventory
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        // Implementation based on PutItemInInventory
    }
}
```

## Phase 6: NPC Interaction Refactoring ✅

### Step 6.1: Create NPCDialogueTrigger ✅

- [x] Create `NPCDialogueTrigger.cs`:

```csharp
public class NPCDialogueTrigger : MonoBehaviour
{
    private NPCInteractionController controller;

    private void Awake()
    {
        controller = GetComponentInParent<NPCInteractionController>();

        if (controller == null)
        {
            Debug.LogError("NPCDialogueTrigger requires an NPCInteractionController component in parent hierarchy!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && controller != null)
        {
            controller.OnPlayerEntered();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && controller != null)
        {
            controller.OnPlayerExited();
        }
    }
}
```

### Step 6.2: Create NPCInstructionUI ✅

- [x] Create `NPCInstructionUI.cs`:

```csharp
public class NPCInstructionUI : MonoBehaviour
{
    [SerializeField] private GameObject spatialPanelModel;
    [SerializeField] private GameObject listeningIcon;
    [HideInInspector] public TMP_Text responseText;

    private ILoggingService loggingService;

    private void Awake()
    {
        loggingService = ServiceLocator.Get<ILoggingService>();

        // Auto-find the spatial panel if not assigned
        if (spatialPanelModel == null)
        {
            spatialPanelModel = transform.Find("_Spatial Panel Manipulator Model")?.gameObject;
            if (spatialPanelModel == null)
            {
                loggingService.LogWarning("Could not find '_Spatial Panel Manipulator Model' child GameObject");
            }
        }

        // Auto-find the listening icon if not assigned
        if (listeningIcon == null)
        {
            listeningIcon = transform.parent.Find("_Actively Listening Feedback Icon")?.gameObject;
            if (listeningIcon == null)
            {
                loggingService.LogWarning("Could not find '_Actively Listening Feedback Icon' sibling GameObject");
            }
        }

        // Auto-find the response text
        responseText = GetComponentInChildren<TMP_Text>();
        if (responseText == null)
        {
            loggingService.LogWarning("Could not find TMP_Text component in children");
        }

        // Hide UI elements initially
        HideSpatialPanel();
        HideListeningIcon();
    }

    public void ShowSpatialPanel()
    {
        if (spatialPanelModel != null)
        {
            spatialPanelModel.SetActive(true);
        }
    }

    public void HideSpatialPanel()
    {
        if (spatialPanelModel != null)
        {
            spatialPanelModel.SetActive(false);
        }
    }

    public void ShowListeningIcon()
    {
        if (listeningIcon != null)
        {
            listeningIcon.SetActive(true);
        }
    }

    public void HideListeningIcon()
    {
        if (listeningIcon != null)
        {
            listeningIcon.SetActive(false);
        }
    }
}
```

### Step 6.3: Implement NPCInteractionController ✅

- [x] Create `NPCInteractionController.cs`:

```csharp
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

        // Find required references
        FindRequiredManagers();
        SetupInputActions();
        SetupSpeechRecognition();
    }

    public void OnPlayerEntered()
    {
        playerInTriggerArea = true;
        instructionUI.ShowSpatialPanel();

        // Check quest state and handle automatic conversations
        if (questState != null && questState.hasQuest)
        {
            HandleQuestState();
        }
    }

    public void OnPlayerExited()
    {
        playerInTriggerArea = false;
        instructionUI.HideSpatialPanel();
        audioService.StopNPCVoice();
        audioService.EndConversation();
    }

    // Additional methods for handling recording, speech processing, etc.
}
```

## Phase 7: Testing and Integration

### Step 7.1: Verify Core Services

- [ ] Test AudioService functionality
- [ ] Test DialogueProvider functionality
- [ ] Test QuestService functionality
- [ ] Test InventoryService functionality

### Step 7.2: Test NPC Interactions

- [ ] Test player approaching NPC
- [ ] Test dialogue flow
- [ ] Test quest detection and completion
- [ ] Test audio transitions

### Step 7.3: Integration Tests

- [ ] Complete end-to-end quest and dialogue flow
- [ ] Verify all sounds play correctly
- [ ] Test inventory functionality
- [ ] Ensure no regressions in behavior

## Additional Notes

### MonoBehaviour Integration Tips

- Get services in Awake() when possible
- Use [RequireComponent] attributes to ensure dependencies
- Use SerializeField for Inspector-configurable properties
- Implement OnValidate() to verify required references

### Best Practices

- Keep business logic out of MonoBehaviours when possible
- Use events for loose coupling between systems
- Always check for null references
- Use consistent naming conventions
- Document public methods and properties

### Debugging

- Log service registrations and resolutions
- Use descriptive error messages for missing services
- Implement graceful fallbacks where possible
