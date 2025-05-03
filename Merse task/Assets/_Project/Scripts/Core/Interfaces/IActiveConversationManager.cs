using System;
using UnityEngine;

namespace Core.Interfaces
{
    /// <summary>
    /// Interface for managing which NPC is the active conversation partner
    /// </summary>
    public interface IActiveConversationManager
    {
        /// <summary>
        /// The current NPC the player is conversing with, or null if not in conversation
        /// </summary>
        GameObject CurrentConversationPartner { get; }

        /// <summary>
        /// Whether the player is currently in a conversation with any NPC
        /// </summary>
        bool IsInConversation { get; }

        /// <summary>
        /// Try to start a conversation with the specified NPC
        /// Returns true if successful, false if another conversation is already active
        /// </summary>
        /// <param name="npc">The NPC GameObject to start a conversation with</param>
        /// <returns>True if successful, false if another conversation is already active</returns>
        bool TryStartConversation(GameObject npc);

        /// <summary>
        /// End the current conversation
        /// </summary>
        void EndConversation();

        /// <summary>
        /// Force-end any active conversation and start a new one
        /// </summary>
        /// <param name="npc">The NPC GameObject to start a conversation with</param>
        void ForceStartConversation(GameObject npc);

        /// <summary>
        /// Event triggered when a conversation starts with an NPC
        /// </summary>
        event Action<GameObject> OnConversationStarted;

        /// <summary>
        /// Event triggered when a conversation ends
        /// </summary>
        event Action OnConversationEnded;
    }
}