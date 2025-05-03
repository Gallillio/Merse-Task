# SOLID VR Interaction System Implementation Guide

This document provides comprehensive setup instructions for implementing our refactored VR interaction system that follows SOLID principles.

## Required Packages

Before implementing the system, ensure you have the following Unity packages installed:

1. **XR Interaction Toolkit** - Core VR interaction capabilities

   - Install via Package Manager
   - Version 2.4.0+
   - Includes XR Direct and Socket Interactors

2. **Whisper Speech-to-Text** - For voice recognition

   - Import from the \_Imports folder
   - Includes WhisperManager and MicrophoneRecord components

3. **TextMeshPro** - Required for text display

   - Install via Package Manager
   - Version 3.0.6+

4. **New Input System** - Required for controller input
   - Install via Package Manager
   - Version 1.5.0+

## Project Structure

The refactored system is organized into the following components:

1. **Core Infrastructure**
   - Service Locator pattern for dependency injection
   - Interface-based design for all major components
2. **Audio System**
   - Replaces the singleton SoundManager with AudioService
3. **Dialogue System**
   - Handles NPC conversations with Gemini API
   - Sentence-by-sentence display with voice
4. **Quest System**
   - Tracks quest progress and inventory items
   - Manages quest state for NPCs
5. **Inventory System**
   - Socket-based item management
   - XR interaction integration
6. **NPC Interaction System**
   - Voice input capture and processing
   - Integration with Whisper STT

## Setup Instructions

### Step 1: Core Service Infrastructure

1. **Namespace Setup**

   - Ensure all scripts are in their appropriate namespaces:
     - Core interfaces: `Core.Interfaces`
     - Core services: `Core.Services`
     - Inventory components: `Inventory`
     - Quest components: `Quest`
     - Dialogue components: `Dialogue`

2. **Create a Service Initializer GameObject**

   - Create an empty GameObject named `_Service Initializer`
   - Add the `ServiceInitializer` component
   - This GameObject should be set to not be destroyed on scene changes
   - It should be placed at the root level of your scene hierarchy

3. **Configure Log Level**

   - Configure the debug logging level on the ServiceInitializer if needed
   - Set `logServiceRegistration` to true during development

4. **Script Execution Order**
   - In Project Settings → Script Execution Order
   - Set ServiceInitializer.cs to execute before default time (-100)
   - This ensures services are registered before other components try to use them

### Step 2: Audio System Setup

1. **Create an Audio Service GameObject**

   - Create a GameObject named `_Audio Service`
   - Add the `AudioService` component
   - Ensure it has an AudioSource component (added automatically by RequireComponent)

2. **Configure the Audio Service**

   - Assign your sound clips to the `soundList` array:
     - Index 0: Background Music
     - Index 1: Item Pickup sound
     - Index 2: NPC Talking sound
     - Index 3: Quest Complete sound
   - Set the `musicDuckingAmount` (0.1-0.9 range) to control volume reduction during conversations
   - Additional AudioSources will be created at runtime

3. **Register with Service Initializer**
   - On the `ServiceInitializer` GameObject
   - Assign the `AudioService` component to the "Audio Service Implementation" field

### Step 3: Dialogue System Setup

1. **Create a Dialogue Service GameObject**

   - Create a GameObject named `_Dialogue Service`
   - Add the `SentenceDisplayController` component
   - Add the `GPTDialogueService` component

2. **Configure GPTDialogueService**

   - Enter your Gemini API key in the "Gemini Api Key" field
   - Add a system message in the "System Message" text area (e.g., "You are a helpful NPC in a fantasy world")
   - This is the general prompt that guides all NPC conversations

3. **Configure SentenceDisplayController**

   - Assign your Input Action asset
   - Configure settings for sentence display speed and timing

4. **Register with Service Initializer**
   - On the `ServiceInitializer` GameObject
   - Assign the `GPTDialogueService` component to the "Dialogue Provider Implementation" field

### Step 4: Quest System Setup

1. **Create a Quest Service GameObject**

   - Create a GameObject named `_Quest Service`
   - Add the `QuestService` component

2. **Configure Quest Service**

   - The inventory sockets will be registered automatically at runtime
   - No manual configuration is required for Quest Service

3. **Register with Service Initializer**
   - On the `ServiceInitializer` GameObject
   - Assign the `QuestService` component to the "Quest Service Implementation" field

### Step 5: Inventory System Setup

1. **Create an Inventory Service GameObject**

   - Create a GameObject named `_Inventory Service`
   - Add the `InventoryService` component

2. **Configure Inventory Service**

   - Create a GameObject named "Collectables" if it doesn't exist
   - Assign this to the "Collectables Parent" field on the Inventory Service
   - This is where detached items will be returned

3. **Register with Service Initializer**

   - On the `ServiceInitializer` GameObject
   - Assign the `InventoryService` component to the "Inventory Service Implementation" field

4. **Setup Item Sockets**
   - On each socket GameObject:
     - Add an `XRSocketInteractor` component from the XR Interaction Toolkit
     - Add the `ItemSocketInteractor` component

### Step 6: NPC Interaction Setup

1. **Create NPC Prefab Structure**

   - For each NPC, create this hierarchy:
     ```
     NPC Parent
     ├── _NPC Trigger (with Collider component)
     ├── _Actively Listening Feedback Icon
     ├── _NPC Model
     └── _Spatial Panel System
         ├── _Spatial Panel Manipulator Model
         └── Response Text (with TMP_Text component)
     ```

2. **Configure NPC Components**

   - On the NPC Parent:

     - Add the `NPCQuestState` component
     - Configure quest settings if this NPC has a quest

   - On the NPC Parent:

     - Add the `NPCInstructionUI` component
     - It will auto-find the UI components but you can also assign them directly

   - On the NPC Parent:

     - Add the `NPCInteractionController` component
     - Assign your Input Action asset

   - On the \_NPC Trigger:
     - Add a Collider component (set to isTrigger = true)
     - Add the `NPCDialogueTrigger` component
     - Size the trigger area appropriately for player detection

3. **Configure Quest Settings (if applicable)**
   - On the `NPCQuestState` component:
     - Set `hasQuest` to true if this NPC offers a quest
     - Enter the `questItemName` that this NPC is looking for
     - Enter prompts for different quest states:
       - `initialPrompt`: First interaction with the player
       - `questInProgressPrompt`: When player returns without the item
       - `completedQuestPrompt`: When player has completed the quest
     - Optionally create and assign a `questRewardObject` to be activated when completed

### Step 7: Whisper STT Integration

1. **Setup Whisper Manager**

   - Create a GameObject named `_Whisper Manager`
   - Add the `WhisperManager` component from the Whisper package
   - Configure the model path and settings

2. **Setup Microphone Recording**
   - Create a GameObject named `_Microphone Record Manager`
   - Add the `MicrophoneRecord` component from the Whisper package
   - Configure microphone settings and VAD (Voice Activity Detection)

### Step 8: Player Setup

1. **Configure XR Rig**

   - Ensure your XR Rig includes:
     - Left and right controllers with XR Direct Interactors
     - Inventory sockets (for item storage) with XR Socket Interactors and ItemSocketInteractor components

2. **Input Actions**
   - Create or configure an Input Action asset with:
     - "Controller" action map containing:
       - "Secondary Button" action (for recording speech)
       - "Primary Button" action (for advancing dialogue)
   - Assign this action asset to:
     - NPCInteractionController components
     - SentenceDisplayController component

## Testing Checklist

Once you've completed setup, verify the following:

1. **Service Registration**

   - Play the scene and check console logs
   - Verify all services are registered successfully
   - Fix any missing references

2. **Audio System**

   - Background music should play automatically
   - Test item pickup sounds
   - Test conversation volume ducking
   - Test quest completion sound

3. **Dialogue System**

   - Test NPC conversations
   - Verify sentence-by-sentence display
   - Confirm input for advancing text works

4. **Quest System**

   - Test giving a quest to player
   - Test quest item detection
   - Test quest completion

5. **Inventory System**

   - Test picking up items
   - Test storing items in sockets
   - Test dropping items from inventory

6. **NPC Interaction**
   - Test approaching an NPC (trigger entry)
   - Test voice recording and processing
   - Test automatic conversations for quest NPCs

## Troubleshooting

- **Missing References**: Ensure all service implementations are assigned in the ServiceInitializer
- **Script Execution Order**: Set ServiceInitializer to execute before other scripts
- **Audio Issues**: Check AudioSources and ensure clips are assigned
- **Voice Recognition**: Check Whisper Manager configuration and microphone settings
- **NPC Conversations**: Verify Gemini API key and connection
- **Quest Detection**: Check that items have correct names matching questItemName fields
- **Type Ambiguity**: If you encounter errors about ambiguous type references (like LogType or SoundType), use fully qualified type names:

  ```csharp
  // Instead of:
  void Log(string message, LogType type = LogType.Info);

  // Use:
  void Log(string message, Core.Interfaces.LogType type = Core.Interfaces.LogType.Info);
  ```

## Advanced Configuration

- **Customize System Messages**: Modify the prompt templates for different NPC types
- **Add More Sound Types**: Extend the SoundType enum and add more audio clips
- **Custom NPC Behaviors**: Inherit from existing components to create specialized NPCs
- **Extend Services**: Add new methods to interfaces and implement in service classes

## Migration from Legacy Systems

If you're migrating from the original implementation to this SOLID refactored version, follow these guidelines:

### Replacing SoundManager Calls

- Replace all static SoundManager calls with IAudioService:

  ```csharp
  // OLD: Static singleton call
  SoundManager.PlaySound(SoundType.ItemPickup);

  // NEW: Service-based approach
  IAudioService audioService = ServiceLocator.Get<IAudioService>();
  audioService.Play(Core.Interfaces.SoundType.ItemPickup);
  ```

### Replacing QuestManager Calls

- Replace all QuestManager singleton references with IQuestService:

  ```csharp
  // OLD: Singleton reference
  bool hasItem = QuestManager.Instance.HasItem("Gem");

  // NEW: Service-based approach
  IQuestService questService = ServiceLocator.Get<IQuestService>();
  bool hasItem = questService.HasItem("Gem");
  ```

### Replacing NPCInteractionManager

- Replace NPCInteractionManager with the three new components:
  - NPCQuestState (for quest-related functionality)
  - NPCInstructionUI (for UI management)
  - NPCInteractionController (for core interaction logic)
  - NPCDialogueTrigger (for player proximity detection)

### Updating GPTManager References

- Replace GPTManager with GPTDialogueService accessed through IDialogueProvider

  ```csharp
  // OLD: Direct reference
  gptManager.TrySendInput(input, npcObject, instruction);

  // NEW: Interface-based reference
  IDialogueProvider dialogueProvider = ServiceLocator.Get<IDialogueProvider>();
  dialogueProvider.StartDialogue(npcObject, input, instruction, response => {
      // Handle the response
  });
  ```
