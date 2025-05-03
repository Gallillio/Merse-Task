# Dialogue System Setup Guide

## Overview

The new dialogue system follows SOLID principles with a modular architecture. It consists of:

1. `GPTDialogueService` - Main service that implements `IDialogueProvider`
2. `SentenceDisplayController` - Controls UI display of dialogue sentences
3. `GeminiAPI` - Client for communicating with Google's Gemini API
4. `SentenceSplitter` - Utility for text processing

## Setup Instructions

### Step 1: Create a GameObject for the Dialogue Service

1. Create a new GameObject in your scene named "\_Dialogue Service"
2. Add the `SentenceDisplayController` component
3. Add the `GPTDialogueService` component (will automatically be added due to RequireComponent)

### Step 2: Configure the Dialogue Service

1. On the `GPTDialogueService` component:

   - Enter your Gemini API key in the "Gemini Api Key" field
   - Add your system message in the "System Message" text area
   - This is the general prompt that guides all NPC conversations

2. On the `SentenceDisplayController` component:
   - Assign your input action asset
   - Configure character speed and sound duration settings

### Step 3: Register with Service Initializer

1. Find the `ServiceInitializer` GameObject in your scene
2. Assign the `GPTDialogueService` component to the "Dialogue Provider Implementation" field

### Step 4: NPC Configuration

Each NPC that uses the dialogue system should have:

1. An `NPCInstruction` component
   - This will be fully implemented in a later phase
   - Currently handles storing the text UI component and NPC-specific instructions

## Usage

The system handles:

- NPC-specific conversation histories
- Sentence-by-sentence display with voice
- Auto-conversations when player approaches
- Custom NPC instructions that modify behavior

## Key Improvements

- **Separation of Concerns**: Each class has a single responsibility
- **Dependency Injection**: Services are resolved through the ServiceLocator
- **Interface-Based Design**: All interactions are through the IDialogueProvider interface
- **Async Operations**: API calls use async/await for better performance
- **Event-Driven Communication**: Components communicate through events
