# Inventory System - SOLID Implementation Guide

This directory contains the SOLID-compliant inventory system components. The legacy system has been completely removed and replaced with a service-based architecture.

## Components

1. **InventoryService** - Core service implementing IInventoryService

   - Handles item attachments and detachments
   - Manages inventory item scaling
   - Tracks original item scales
   - Interacts with the audio service

2. **ItemSocketInteractor** - Component for socket-based inventory

   - Replaces the legacy PutItemInInventory component
   - Integrates with the XR Interaction Toolkit
   - Uses InventoryService for actual functionality
   - Manages events for item selection/deselection

3. **FistInventoryController** - UI controller for wrist-based inventory
   - Replaces the legacy FistInventory component
   - Shows/hides inventory based on wrist rotation
   - Uses ServiceLocator for dependency injection

## Migration Steps

If you're updating existing GameObjects in your scenes, follow these steps:

1. **Socket-based Inventory**

   - Remove any existing **PutItemInInventory** components
   - Add the **ItemSocketInteractor** component to all inventory sockets
   - Ensure an **InventoryService** component exists in your scene
   - Register the InventoryService in ServiceInitializer

2. **Wrist-based Inventory UI**
   - Remove any existing **FistInventory** components
   - Add the **FistInventoryController** component to the inventory canvas
   - Configure the min/max rotation angles
   - Assign the inventory UI GameObject

## Example Setup

```csharp
// In ServiceInitializer.cs
private void InitializeServices()
{
    // Register the inventory service
    InventoryService inventoryService = FindObjectOfType<InventoryService>();
    ServiceLocator.Register<IInventoryService>(inventoryService);
}
```

## Common Issues

- **Item Scaling**: If items show incorrect scaling in sockets, ensure only ItemSocketInteractor is attached to socket objects
- **Quest Detection**: Ensure all sockets have ItemSocketInteractor to be properly registered with QuestService
- **Missing References**: If collectables parent is missing, ensure you have a GameObject named "Collectables" in your scene
