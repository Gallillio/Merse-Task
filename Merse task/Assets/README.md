# VR Interaction System

This project is a Unity VR application featuring interactive NPCs, quests, sound management, and inventory systems. It allows players to interact with NPCs through voice, the NPC responds with a Gemini AI generated Response depending on the NPC, complete quests, and manage items in VR.

## System Overview

### 1. Sound Management System

**Location:** `_Project/Scripts/SoundManager/`

The Sound Manager handles all audio playback in the game, including background music, NPC talking sounds, item pickup effects, and quest completion notifications.

**Key Features:**

- Background music with automatic looping
- NPCs have talking sounds with duration based on dialogue length
- Dynamic volume adjustment (ducking) during conversations
- Special quest completion sound that temporarily pauses background music
- Seamless audio transitions between gameplay states

**Key Scripts:**

- `SoundManager.cs`: Singleton manager that handles all sound playback, providing methods to play sounds with different parameters including looping and volume control.

**Sound Types:**

- `BackgroundMusic`: Ambient music that plays continuously in the background
- `ItemPickup`: Played when items are collected
- `NPCTalking`: Voice sounds for NPCs when they're speaking
- `QuestComplete`: Special sound that plays when a quest is completed

**Unity Setup Requirements:**

- Requires an AudioSource component on the GameObject
- Needs AudioClip assets assigned to the soundList array in the Inspector
- Configurable ducking amount for background music during conversations (0.1-0.9 range)

### 2. NPC Interaction System

**Location:** `_Project/Scripts/NPC/`

This system enables conversation with NPCs using voice input, processes responses using Google's Gemini API, and manages quest states.

**Key Features:**

- Voice-based conversation with NPCs
- Sentence-by-sentence text display with advancing via controller button
- Quest system integrated with dialogues
- NPC talking sound effects synchronized with text display
- Background music ducking during conversations

**Key Scripts:**

- `NPCInteractionManager.cs`: Handles player proximity detection, microphone input, and speech-to-text processing
- `GPTManager.cs`: Communicates with Google's Gemini API to generate NPC responses
- `NPCInstruction.cs`: Stores NPC dialogue data and quest information
- `QuestManager.cs`: Tracks quest progress and inventory items related to quests
- `QuestSocket.cs`: Helper component for quest-related item detection

**NPC Setup Requirements:**

- Each NPC needs an NPCInstruction component with dialogue text
- For quest NPCs, configure questItemName and completion prompts
- NPCs should have a spatial panel object for displaying text responses
- Proper hierarchy with parent NPC object and child interaction manager

### 3. Inventory System

**Location:** `_Project/Scripts/Inventory/`

This system allows players to pick up, store, and manage items in VR, including quest items.

**Key Features:**

- Socket-based inventory system allowing items to be placed in specific locations
- Sound effects for item pickup events
- Item scaling and parent management when stored in inventory
- Detection of items for quest completion

**Key Scripts:**

- `PutItemInInventory.cs`: Core inventory script that handles storing items in sockets
- `FistInventory.cs`: Specialized inventory script for items held directly

**Inventory Setup Requirements:**

- Requires XR Socket Interactors for item placement
- Items must have appropriate XR Interactable components
- Reference to a "Collectables" GameObject for item organization

### 4. VR Interaction Utilities

**Location:** `_Project/Scripts/`

Supporting scripts that enhance VR functionality and player experience.

**Key Scripts:**

- `AnimateHand.cs`: Animates hand models based on controller input (grip and trigger)
- `HMDInfoManager.cs`: Detects and logs VR headset information

**Setup Requirements:**

- AnimateHand requires an Animator component and properly configured hand animations
- Both scripts need appropriate references set in the Inspector

## Quest System Details

Quests are managed through NPCs and involve finding specific items:

1. Initial interaction with an NPC activates their quest
2. Quest information is stored in NPCInstruction components
3. Player must find the requested quest item in the world
4. When returning with the correct item, the quest is marked as completed
5. Quest completion may activate a reward object on the NPC
6. The quest item is removed from player inventory upon completion

**Quest Completion Process:**

1. Player approaches NPC with the quest item in inventory
2. System automatically detects the item and completes the quest
3. Quest completion dialogue is shown
4. Quest completion sound plays
5. Reward object is activated (if configured)

## Audio Behavior Details

The audio system features sophisticated behavior:

1. Background music plays continuously during gameplay
2. When conversation starts, background music volume is reduced
3. During NPC dialogue, talking sounds play with duration based on text length
4. Volume remains reduced throughout the entire conversation
5. When conversation ends, background music returns to normal volume
6. For quest completion, special sound plays while background music is temporarily paused
7. If player walks away mid-conversation, all audio returns to normal state

## Technical Requirements

- Unity with XR Interaction Toolkit
- Google Gemini API key for NPC conversations
- Whisper speech-to-text system for voice input
- AudioMixer setup for sound management
- Properly configured XR Rig with controllers

## Integration Notes

- The system uses Unity's new Input System for controller input
- Sound Manager is designed as a singleton for easy access from any script
- NPCs use a spatial UI system for displaying text
- Inventory uses Unity's XR Socket Interactor system
- Voice detection uses Voice Activity Detection (VAD) for better performance
