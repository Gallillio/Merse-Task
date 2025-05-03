# Inventory System Setup Guide

## Overview

The refactored Inventory System follows SOLID principles with a service-based architecture. It consists of:

1. `InventoryService` - Main service that implements `IInventoryService`
2. `ItemSocketInteractor` - Component attached to XR sockets that uses the service

## Setup Instructions

### Step 1: Create an Inventory Service GameObject

1. Create a new GameObject in your scene named "\_Inventory Service"
2. Add the `InventoryService` component

### Step 2: Configure Inventory Service

1. On the `InventoryService` component:
   - Assign your "Collectables" parent GameObject if you have one
   - This is where items will be returned when detached from sockets

### Step 3: Register with Service Initializer

1. Find the `ServiceInitializer` GameObject in your scene
2. Assign the `InventoryService` component to the "Inventory Service Implementation" field

### Step 4: Setup Item Sockets

1. Make sure your socket GameObjects have:
   - An `XRSocketInteractor` component
   - The `ItemSocketInteractor` component
2. The ItemSocketInteractor will automatically:
   - Register with the QuestService
   - Use the InventoryService for managing item attachments

## Usage

The inventory system handles:

- Attaching items to sockets (with proper parenting and scaling)
- Detaching items and returning them to the collectables area
- Playing appropriate sound effects for interactions
- Maintaining original item scales
- Tracking which items are in which sockets

## Integration with Quest System

This system integrates with the Quest System by:

1. Registering all sockets with the QuestService
2. Quest NPCs can check if the player has specific items in their inventory
3. Items can be removed from inventory when completing quests

## Key Improvements

- **Separated Responsibilities**: InventoryService manages the core logic, while ItemSocketInteractor handles XR interactions
- **Dependency Injection**: Uses ServiceLocator for dependencies
- **Cleaner Code**: Reduced duplicate code across sockets
- **Better Logging**: Comprehensive logging throughout the system
- **Proper Error Handling**: Graceful fallbacks when services are unavailable
- **Consistent Scale Management**: Centralized handling of item scales when attaching/detaching
