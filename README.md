Hey Team 👋
Make sure to follow the controlling instructions cuz there isnt much UI to help you navigate the game yet.

## Table of Contents

- [Overview](#overview)
- [Installation](#installation)
- [Getting Started](#getting-started)
- [Controls](#controls)
- [Gameplay](#gameplay)
- [Features](#features)
  - [Speech Recognition](#speech-recognition)
  - [Quest System](#quest-system)
  - [Inventory System](#inventory-system)
  - [NPC Dialogue](#npc-dialogue)
  - [XR Interaction](#xr-interaction)
  - [Input System](#input-system)
- [Troubleshooting](#troubleshooting)
- [Credits](#credits)

## Overview

This immersive VR experience puts you in a vibrant interactive world where you'll communicate with NPCs through natural speech, solve puzzles, complete quests, and manipulate objects in a fully interactive environment. The game combines cutting-edge speech recognition with intuitive VR controls to create a seamless and immersive experience.

Your mission is to help various NPCs by finding items and completing tasks for them, ultimately unlocking "The Great Wall of America" and discovering what lies beyond.

![Game Overview Screenshot](Assets/Documentation/Images/game_overview.png)

## Installation

### Requirements

- Unity 2021.3 or newer
- VR Headset (Meta Quest, Valve Index, HTC Vive, etc.)
- Microphone (integrated or external)
- Minimum 8GB RAM
- GPU with VR support

### Setup Instructions

1. Clone or download this repository
2. Open the project in Unity
3. Make sure you have all required packages installed:
   - XR Interaction Toolkit (version 2.4.3+)
   - Whisper Speech-to-Text package
   - [Optional] Speech to Text Showcase asset
4. Connect your VR headset
5. Press Play in Unity or build the project for your target platform

## Getting Started

When you first launch the game, you'll find yourself in a colorful environment with NPCs waiting to interact with you. Look around to familiarize yourself with your surroundings. NPCs with available quests will have speech bubble indicators above them.

Movement is handled through smooth locomotion or teleportation (depending on your settings). You can select your preferred movement style in the settings menu.

![Getting Started](Assets/Documentation/Images/getting_started.png)

## Controls

### Basic Controls

- **Move**: Use the left thumbstick/joystick
- **Turn**: Use the right thumbstick/joystick (smooth or snap turning)
- **Grab Objects**: Grip button (middle finger) on controllers
- **Select/Use Objects**: Trigger button (index finger) on controllers
- **Talk to NPCs**: Hold Secondary Button (B/Y) on your controller while near an NPC, then speak into your microphone
- **UI Interaction**: Primary Button (A/X) or pointing and using trigger
- **Menu**: Menu button on controllers

All buttons can be remapped in the settings if needed. The Input System has been configured to work with most common VR controllers.

![Control Diagram](Assets/Documentation/Images/controls.png)

## Gameplay

### Talking to NPCs

1. Approach an NPC (you'll see a dialog indicator when close enough)
2. Press and hold the Secondary Button (B/Y) on your controller
3. Speak naturally into your microphone - the microphone icon will show when active
4. Release the button when finished speaking
5. The NPC will process your speech and respond appropriately
6. Continue the conversation to learn about quests and the world

The speech recognition system will understand natural language, so you can phrase your questions and responses in different ways.

![NPC Interaction](Assets/Documentation/Images/npc_interaction.png)

### Completing Quests

1. Talk to NPCs to receive quests (they'll tell you what they need)
2. Listen carefully to their requests - quests are tracked automatically
3. Explore the world to find the required items or complete objectives
4. Collect items by grabbing them with your controller's grip button
5. Return to NPCs with completed objectives
6. Either place requested items in their designated socket areas or speak to the NPC about the quest

Quests have multiple stages and can lead to new areas and discoveries. Completing all quests will unlock "The Great Wall of America" and reveal the game's conclusion.

![Quest Completion](Assets/Documentation/Images/quest_completion.png)

### Inventory Management

1. Grab items by using the grip button when your hand is near them
2. Items are automatically stored in your inventory when grabbed
3. To use an item from inventory, grab it from your inventory slots
4. You can examine items by bringing them closer to your face
5. Return items to the world by releasing the grip button
6. Place quest items in designated sockets to complete objectives

The inventory system allows you to carry multiple items at once, eliminating the need to make several trips when collecting quest items.

![Inventory System](Assets/Documentation/Images/inventory.png)

## Features

### Speech Recognition

The game uses Whisper speech-to-text technology to enable natural conversations with NPCs. This system:

- Processes your speech in real-time with high accuracy
- Detects when you've stopped speaking to avoid cutting you off
- Converts speech to text for NPC interactions
- Works with various accents and languages
- Provides visual feedback when listening via a microphone icon
- Uses voice activity detection (VAD) to filter out background noise
- Processes contextual understanding to make conversations feel natural

This speech system allows you to ask questions and give responses in your own words, creating a more immersive and natural interaction than traditional dialogue trees.

![Speech Recognition](Assets/Documentation/Images/speech_recognition.png)

### Quest System

The dynamic quest system features:

- Multiple NPCs with unique quests and personalities
- Contextual quest tracking that remembers your progress
- Quests that require specific items to be collected and delivered
- Multi-stage quests with progressive challenges
- A central quest completion tracker that handles game progression
- Visual indicators showing active and completed quests
- Automatic quest activation when speaking with NPCs
- A final objective that unlocks when all other quests are completed

The quest system drives the game's progression and encourages exploration of the environment to find all required items.

![Quest System](Assets/Documentation/Images/quest_system.png)

### Inventory System

The socket-based inventory system provides:

- Intuitive object grabbing and manipulation
- Dedicated inventory slots for storing collected items
- Visual feedback when items are selected or stored
- Proper physics interactions with objects
- Scale adjustments when items are stored and retrieved
- Integration with the quest system for item tracking
- A simple way to return items to the world when needed

The inventory makes collecting and managing quest items intuitive and immersive, with full physics support for realistic interactions.

![Inventory System Detail](Assets/Documentation/Images/inventory_detail.png)

### NPC Dialogue

The dialogue system offers:

- Natural language processing for realistic conversations
- Contextual responses based on your quest progress
- NPCs that remember previous interactions
- Visual feedback during conversations through UI elements
- Character-specific dialogue styles and personalities
- Seamless integration with the Whisper speech recognition system
- Automatic quest updates based on conversation content

NPCs respond differently depending on your progress in the game, creating a dynamic narrative experience.

![NPC Dialogue System](Assets/Documentation/Images/dialogue_system.png)

### XR Interaction

The XR interaction system provides:

- Intuitive grabbing and manipulation of objects
- Physical interactions with all items in the environment
- Haptic feedback for more immersive interactions
- Teleportation and smooth locomotion options
- UI interaction through pointing and selection
- Object highlighting when interactive elements are nearby
- Socket interactions for inventory and quest completion

Based on Unity's XR Interaction Toolkit, the system makes interacting with the virtual world feel natural and responsive.

![XR Interaction](Assets/Documentation/Images/xr_interaction.png)

### Input System

The game uses Unity's new Input System with complete controller mapping:

- Fully customizable button mappings
- Support for all major VR controllers
- Adaptive bindings that work across different devices
- Proper Secondary Button support for microphone activation
- Debug tools for testing button inputs
- Integration with XR Interaction Toolkit
- Automatic fallbacks for different controller types

The input system has been carefully configured to ensure all buttons work correctly across different VR platforms.

![Input System](Assets/Documentation/Images/input_system.png)

## Troubleshooting

### Microphone Not Working

- Ensure your microphone is properly connected and enabled
- Check microphone permissions in your operating system
- Verify the correct microphone is selected in Unity's audio settings
- Try speaking louder or in a quieter environment
- Restart the game if speech recognition becomes unresponsive
- Check that you're holding the Secondary Button (B/Y) while speaking

### VR Controls Issues

- Ensure your controllers are properly paired and charged
- If certain buttons don't work, use the included InputBindingFixer component to add proper XR bindings
- Check controller bindings in the SteamVR or Oculus settings
- Restart your VR headset if buttons aren't responding
- Test with the included InputTester scene to verify all buttons are functioning
- For Oculus Quest users, make sure you have the latest firmware

### Performance Problems

- Reduce graphics settings if the game is running slowly
- Close other applications running in the background
- Ensure your computer meets the minimum requirements
- Try reducing the physical objects in the scene
- Lower the quality settings of the speech recognition if needed
- For Quest users, ensure proper cooling and battery level

## Credits

- Game Design & Development: [Your Name/Team]
- Speech Recognition: Whisper by OpenAI
- XR Interaction Framework: Unity XR Interaction Toolkit
- 3D Models: [List sources]
- Audio: [List sources]
- Special Thanks: [Acknowledgements]
