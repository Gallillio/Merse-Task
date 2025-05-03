# Quest System for NPC Interactions

This document explains how to set up and use the quest system with NPCs in your VR project.

## Overview

The quest system allows NPCs to:

- Request specific items from the player
- Track whether the player has found the requested items
- Change dialogue based on quest progression state
- Recognize when items are in the player's inventory (XR Sockets)

## Setup Instructions

### 1. Setting up the Quest Manager

1. Create an empty GameObject in your scene named `_Quest Manager`
2. Add the `QuestManager` component to it
3. Alternatively, the QuestManager will be created automatically when needed

### 2. Setting up Player Inventory

1. Locate your player's XR Socket objects (the ones that can hold grabbable items)
2. Add the `QuestSocket` script to each XR Socket
3. Leave `autoRegister` checked to automatically add them to the QuestManager's list
4. Alternatively, manually add socket references in the QuestManager's `playerInventorySockets` list

### 3. Configuring NPCs with Quests

For each NPC that should offer a quest:

1. Select the NPC's parent GameObject (which should have the `NPCInstruction` component)
2. In the Inspector, check the `hasQuest` checkbox
3. Enter the name of the quest item in `questItemName` (case-insensitive, partial matches work)
4. Fill in the dialogue prompts:
   - `npcInstruction`: The initial dialogue when player first meets the NPC
   - `questInProgressPrompt`: The dialogue when player returns without the requested item
   - `completedQuestPrompt`: The dialogue when player returns with the requested item

Example prompts:

**npcInstruction (Initial prompt):**

```
You are a sarcastic character that knows he is in a videogame. You should ask the user if he can find your purse. Talk in a funny sarcastic way.
```

**questInProgressPrompt:**

```
You are a sarcastic character that is still waiting for the player to find your purse. Express impatience and remind them what they're supposed to be looking for. Talk in a funny sarcastic way.
```

**completedQuestPrompt:**

```
You are a sarcastic character that is happy the player found your purse. Thank them in your own sarcastic way, maybe even express surprise they actually managed to do it. Talk in a funny sarcastic way.
```

### 4. Setting up Quest Items

1. Make sure your grabbable item objects have names that include the `questItemName` specified in the NPC's configuration
2. When a player places these items in their inventory (XR Socket), the system will automatically detect them

## How It Works

1. When a player first interacts with an NPC, they receive the initial dialogue (`npcInstruction`)
2. The NPC's quest is marked as active
3. If the player returns to the NPC without the requested item:
   - They'll receive the "in progress" dialogue (`questInProgressPrompt`)
4. If the player returns with the requested item in their inventory:
   - The quest is marked as completed
   - They'll receive the completion dialogue (`completedQuestPrompt`)

## Troubleshooting

- **Items not being detected**: Make sure the item's name contains the text specified in the NPC's `questItemName` field
- **Socket not registering**: Ensure the QuestSocket component is attached and check the console for registration messages
- **Wrong prompt playing**: Verify the quest state by checking the NPCInstruction component's `questActive` and `questCompleted` values in the Inspector during play mode

## Advanced Usage

- You can manually register inventory sockets using `QuestManager.Instance.RegisterInventorySocket(transform)`
- Quest state is tracked per NPC, so multiple quests can be active simultaneously
- The system uses partial name matching for items, so "Key" will match "GoldenKey", "key_item", etc.
