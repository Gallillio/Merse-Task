using System;
using UnityEngine;
using Core.Interfaces;

namespace Core.Services
{
    /// <summary>
    /// Service for managing which NPC is the active conversation partner
    /// </summary>
    public class ActiveConversationManager : MonoBehaviour, IActiveConversationManager
    {
        private GameObject currentNPC;
        private ILoggingService logger;

        /// <summary>
        /// The current NPC the player is conversing with, or null if not in conversation
        /// </summary>
        public GameObject CurrentConversationPartner => currentNPC;

        /// <summary>
        /// Whether the player is currently in a conversation with any NPC
        /// </summary>
        public bool IsInConversation => currentNPC != null;

        /// <summary>
        /// Event triggered when a conversation starts with an NPC
        /// </summary>
        public event Action<GameObject> OnConversationStarted;

        /// <summary>
        /// Event triggered when a conversation ends
        /// </summary>
        public event Action OnConversationEnded;

        private void Awake()
        {
            logger = ServiceLocator.Get<ILoggingService>();
        }

        /// <summary>
        /// Try to start a conversation with the specified NPC
        /// Returns true if successful, false if another conversation is already active
        /// </summary>
        public bool TryStartConversation(GameObject npc)
        {
            if (npc == null)
            {
                logger?.LogWarning("Attempted to start conversation with null NPC");
                return false;
            }

            // If already in conversation with this NPC, return true
            if (currentNPC == npc)
            {
                return true;
            }

            // If already in conversation with another NPC, return false
            if (currentNPC != null)
            {
                logger?.Log($"Cannot start conversation with {npc.name} - already in conversation with {currentNPC.name}");
                return false;
            }

            // Start conversation with this NPC
            currentNPC = npc;
            logger?.Log($"Started conversation with {npc.name}");
            OnConversationStarted?.Invoke(npc);
            return true;
        }

        /// <summary>
        /// End the current conversation
        /// </summary>
        public void EndConversation()
        {
            if (currentNPC != null)
            {
                logger?.Log($"Ended conversation with {currentNPC.name}");
                currentNPC = null;
                OnConversationEnded?.Invoke();
            }
        }

        /// <summary>
        /// Force-end any active conversation and start a new one
        /// </summary>
        public void ForceStartConversation(GameObject npc)
        {
            if (npc == null)
            {
                logger?.LogWarning("Attempted to force-start conversation with null NPC");
                return;
            }

            // End current conversation if there is one
            if (currentNPC != null && currentNPC != npc)
            {
                logger?.Log($"Force-ending conversation with {currentNPC.name} to start new conversation with {npc.name}");
                OnConversationEnded?.Invoke();
            }

            // Start new conversation
            currentNPC = npc;
            logger?.Log($"Started conversation with {npc.name}");
            OnConversationStarted?.Invoke(npc);
        }
    }
}