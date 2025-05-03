# Quest Completion Tracking System

## Overview

The Quest Completion Tracking system provides a way to track the overall completion of quests in the game. It follows SOLID principles with:

1. **IQuestCompletionTracker** - Interface defining the contract for quest tracking
2. **QuestCompletionTracker** - Implementation of the interface
3. **QuestCompletionManager** - Game-specific handler for completion events

## Setup Instructions

### Step 1: Create a Quest Completion Tracker GameObject

1. Create a new GameObject in your scene named "\_Quest Completion Tracker"
2. Add the `QuestCompletionTracker` component

### Step 2: Register with Service Initializer

1. Find the `ServiceInitializer` GameObject in your scene
2. Assign the `QuestCompletionTracker` component to the "Quest Completion Tracker Implementation" field

### Step 3: Set Up Quest Completion Manager (Optional)

1. Create a new GameObject or use an existing game manager object
2. Add the `QuestCompletionManager` component
3. Configure the settings:
   - Enable/disable auto-registration of quests
   - Add manual quest requirements if needed
   - Add GameObjects to activate when all quests are completed

## Usage Options

### Option 1: Auto-Registration (Recommended)

The easiest way to use the system is to enable auto-registration in the QuestCompletionManager. This will:

1. Find all NPCs with QuestState components
2. Register each quest automatically with the tracking system
3. Track completion based on the QuestService events

### Option 2: Manual Registration

If you prefer more control, you can manually register quests:

1. In the `QuestCompletionManager` inspector, add quest requirements
2. Each requirement needs:
   - `questId`: A unique identifier (e.g., "main_quest_1")
   - `questName`: The name of the quest item

### Option 3: Code Registration

You can also register quests through code:

```csharp
// Get the quest tracker service
var questTracker = ServiceLocator.Get<IQuestCompletionTracker>();

// Register a quest
questTracker.RegisterQuest("quest_id", "Quest Name");

// Complete a quest programmatically
questTracker.CompleteQuest("quest_id");

// Check if all quests are completed
bool allDone = questTracker.AreAllQuestsCompleted();
```

## Event System

The system provides events you can subscribe to:

1. **OnQuestCompleted** - Triggered when a specific quest is completed
2. **OnAllQuestsCompleted** - Triggered when all registered quests are completed

## SOLID Principles Implementation

- **Single Responsibility**: Each class has a specific role
- **Open/Closed**: The system can be extended without modifying existing code
- **Liskov Substitution**: Different implementations of IQuestCompletionTracker can be swapped
- **Interface Segregation**: Clean interface with only necessary methods
- **Dependency Inversion**: Components depend on abstractions, not concrete implementations

## Integration with Existing Systems

This system builds on top of the existing Quest and NPC systems by:

1. Listening to the QuestService.OnQuestCompleted events
2. Reading quest state from NPCQuestState components
3. Providing a central tracking mechanism for overall game progression
