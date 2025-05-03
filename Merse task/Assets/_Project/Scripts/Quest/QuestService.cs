using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Interfaces;
using Core.Services;

namespace Quest
{
    /// <summary>
    /// Service for managing quests and tracking inventory items
    /// </summary>
    public class QuestService : MonoBehaviour, IQuestService
    {
        [Header("Player Inventory")]
        [SerializeField] private List<Transform> inventorySockets = new List<Transform>();

        private ILoggingService logger;

        /// <summary>
        /// Event triggered when a quest is completed
        /// </summary>
        public event Action<string> OnQuestCompleted;

        private void Awake()
        {
            logger = ServiceLocator.Get<ILoggingService>();
        }

        /// <summary>
        /// Check if player has a specific item
        /// </summary>
        /// <param name="itemName">The name of the item to check for</param>
        /// <returns>True if player has the item, false otherwise</returns>
        public bool HasItem(string itemName)
        {
            if (string.IsNullOrEmpty(itemName))
            {
                logger?.LogWarning("Trying to check for item with empty name");
                return false;
            }

            foreach (Transform socket in inventorySockets)
            {
                if (socket == null) continue;

                // Check all children of this socket
                foreach (Transform child in socket)
                {
                    // Compare by name (case insensitive)
                    if (child.name.ToLower().Contains(itemName.ToLower()))
                    {
                        logger?.Log($"Found quest item '{itemName}' in player inventory: {child.name}");
                        return true;
                    }
                }
            }

            logger?.Log($"Quest item '{itemName}' not found in player inventory");
            return false;
        }

        /// <summary>
        /// Remove an item from player's inventory
        /// </summary>
        /// <param name="itemName">The name of the item to remove</param>
        /// <returns>The removed GameObject, or null if not found</returns>
        public GameObject RemoveItem(string itemName)
        {
            if (string.IsNullOrEmpty(itemName))
            {
                logger?.LogWarning("Trying to remove item with empty name");
                return null;
            }

            foreach (Transform socket in inventorySockets)
            {
                if (socket == null) continue;

                // Check all children of this socket
                foreach (Transform child in socket)
                {
                    // Compare by name (case insensitive)
                    if (child.name.ToLower().Contains(itemName.ToLower()))
                    {
                        // Save reference to the GameObject before detaching
                        GameObject itemToRemove = child.gameObject;

                        // Detach from parent
                        child.SetParent(null);

                        // Trigger the quest completed event
                        OnQuestCompleted?.Invoke(itemName);

                        logger?.Log($"Removed quest item '{itemName}' from socket {socket.name}");
                        return itemToRemove;
                    }
                }
            }

            logger?.LogWarning($"Could not find quest item '{itemName}' to remove from inventory");
            return null;
        }

        /// <summary>
        /// Register an inventory socket for tracking
        /// </summary>
        /// <param name="socket">The socket transform to register</param>
        public void RegisterSocket(Transform socket)
        {
            if (socket != null && !inventorySockets.Contains(socket))
            {
                inventorySockets.Add(socket);
                logger?.Log($"Registered inventory socket: {socket.name}");
            }
            else if (socket == null)
            {
                logger?.LogWarning("Attempted to register null socket");
            }
        }

        /// <summary>
        /// Get all registered inventory sockets
        /// </summary>
        /// <returns>List of inventory socket transforms</returns>
        public List<Transform> GetAllSockets()
        {
            return new List<Transform>(inventorySockets);
        }

        /// <summary>
        /// Complete a quest with the specified item name
        /// </summary>
        /// <param name="questItemName">Name of the quest item</param>
        public void CompleteQuest(string questItemName)
        {
            if (string.IsNullOrEmpty(questItemName))
            {
                logger?.LogWarning("Attempted to complete quest with empty item name");
                return;
            }

            // Invoke the event
            OnQuestCompleted?.Invoke(questItemName);
            logger?.Log($"Quest completed for item: {questItemName}");
        }
    }
}