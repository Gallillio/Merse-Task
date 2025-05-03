# Quest System Setup Guide

## Overview

The new Quest System follows SOLID principles with a modular architecture. It consists of:

1. `QuestService` - Main service that implements `IQuestService`
2. `NPCQuestState` - Component for tracking quest state on NPCs

## Setup Instructions

### Step 1: Create a Quest Service GameObject

1. Create a new GameObject in your scene named "\_Quest Service"
2. Add the `QuestService` component

### Step 2: Configure Quest Service

1. On the `QuestService` component:
   - Add any inventory sockets that you want the quest system to track
   - These will be automatically populated when sockets register themselves

### Step 3: Register with Service Initializer

1. Find the `ServiceInitializer` GameObject in your scene
2. Assign the `QuestService` component to the "Quest Service Implementation" field

### Step 4: Setup NPCs with Quests

1. Add the `NPCQuestState` component to any NPC GameObject that should have a quest
2. Configure the quest settings:
   - Set `hasQuest` to true
   - Enter the `questItemName` that this NPC is looking for
   - Enter prompts for different quest states:
     - `initialPrompt`: First interaction with the player
     - `questInProgressPrompt`: When player returns without the item
     - `completedQuestPrompt`: When player has completed the quest
   - Optionally assign a `questRewardObject` to be activated when the quest is completed

## Usage

The quest system handles:

- Tracking items in the player's inventory
- Detecting when a player has obtained a quest item
- Managing quest state (not started, active, completed)
- Providing appropriate dialogue prompts based on quest state
- Playing quest completion sound

## Key Improvements

- **Removal of Singletons**: Uses dependency injection via ServiceLocator
- **Interface-Based Design**: All interactions are through the IQuestService interface
- **Separation of Concerns**: QuestService handles item tracking while NPCQuestState handles NPC-specific quest logic
- **Event-Driven Architecture**: Components communicate through events rather than direct references
- **Improved Testing**: Services can be mocked or replaced in test environments
