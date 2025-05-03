using UnityEngine;
using System;

namespace Core.Interfaces
{
    /// <summary>
    /// Interface for quest management
    /// </summary>
    public interface IQuestService
    {
        /// <summary>
        /// Check if player has a specific item
        /// </summary>
        /// <param name="itemName">The name of the item to check for</param>
        /// <returns>True if player has the item, false otherwise</returns>
        bool HasItem(string itemName);

        /// <summary>
        /// Remove an item from player's inventory
        /// </summary>
        /// <param name="itemName">The name of the item to remove</param>
        /// <returns>The removed GameObject, or null if not found</returns>
        GameObject RemoveItem(string itemName);

        /// <summary>
        /// Register an inventory socket for tracking
        /// </summary>
        /// <param name="socket">The socket transform to register</param>
        void RegisterSocket(Transform socket);

        /// <summary>
        /// Event triggered when a quest is completed
        /// </summary>
        event Action<string> OnQuestCompleted;
    }
}