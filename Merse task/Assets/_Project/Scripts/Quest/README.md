# VR Quest System Setup

This directory contains scripts for implementing a simple quest system where NPCs can detect items in the player's inventory slots.

## Setup Instructions

### 1. Quest Manager Setup

1. Add the `QuestManager.cs` script to an empty GameObject in your scene
2. This object will persist between scenes, tracking quest completion status

### 2. Quest Items Setup

1. For each collectable item that can be used in quests:
   - Add the `QuestItem.cs` component
   - Set a unique `itemID` for each item
   - Fill in the `itemName` and `itemDescription` fields

### 3. NPC Setup

1. Create an NPC GameObject with:
   - A collider component set to `isTrigger = true`
   - The `NPCQuestGiver.cs` script
2. Configure the NPCQuestGiver:
   - Set the `npcName`
   - Add quest requirements in the `questRequirements` array
   - For each quest requirement:
     - Set the `requiredItemID` to match the ID of a quest item
     - Set a unique `questID`
     - Add a description of the quest
   - Customize the dialogue messages

### 4. Player Setup

1. Ensure your player GameObject has the tag "Player"
2. Make sure your inventory sockets have the `PutItemInInventory.cs` component

## How It Works

1. When a player enters an NPC's trigger collider, the NPC checks all inventory sockets on the player
2. If the player has the required quest item in any socket, the quest is marked as completed
3. The NPC outputs debug messages confirming quest completion
4. Quest status is tracked in the QuestManager for persistence between scenes

## Example Quest Setup

Example NPC configuration:

- NPC Name: "Village Elder"
- Quest Requirement:
  - Required Item ID: "magic_herb"
  - Quest ID: "elder_healing_potion"
  - Quest Description: "Find the magic herb for the village elder"
  - Incomplete Dialogue: "I need a magic herb to make a healing potion. Please find one for me."
  - Complete Dialogue: "Thank you for bringing me the herb! Now I can make the healing potion."

Example Quest Item configuration:

- Item ID: "magic_herb"
- Item Name: "Magic Herb"
- Item Description: "A rare herb with magical healing properties"
