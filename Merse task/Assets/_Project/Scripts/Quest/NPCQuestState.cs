using UnityEngine;
using Core.Interfaces;
using Core.Services;

namespace Quest
{
    /// <summary>
    /// Tracks and manages quest state for an NPC
    /// </summary>
    public class NPCQuestState : MonoBehaviour
    {
        [Header("Quest Settings")]
        [Tooltip("Whether this NPC offers a quest")]
        public bool hasQuest = false;

        [Tooltip("The name of the item this NPC is looking for")]
        public string questItemName;

        [Header("Quest Prompts")]
        [Tooltip("Initial prompt when player first meets NPC")]
        [TextArea(2, 5)]
        public string initialPrompt;

        [Tooltip("Prompt when player returns without the quest item")]
        [TextArea(2, 5)]
        public string questInProgressPrompt;

        [Tooltip("Prompt when player completes the quest")]
        [TextArea(2, 5)]
        public string completedQuestPrompt;

        [Header("Quest Rewards")]
        [Tooltip("GameObject to activate when quest is completed")]
        [SerializeField] private GameObject questRewardObject;

        [Header("Quest State")]
        [Tooltip("Whether the quest has been given to the player")]
        [HideInInspector]
        public bool questActive = false;

        [Tooltip("Whether the quest has been completed")]
        [HideInInspector]
        public bool questCompleted = false;

        private IQuestService questService;
        private IAudioService audioService;
        private ILoggingService logger;

        /// <summary>
        /// Initialize and get service references
        /// </summary>
        private void Start()
        {
            questService = ServiceLocator.Get<IQuestService>();
            audioService = ServiceLocator.Get<IAudioService>();
            logger = ServiceLocator.Get<ILoggingService>();

            // Hide the reward object initially
            if (questRewardObject != null)
            {
                questRewardObject.SetActive(false);
            }
        }

        /// <summary>
        /// Check if player has the quest item when they approach
        /// </summary>
        public void CheckQuestCompletion()
        {
            if (!hasQuest || questCompleted || !questActive || questService == null)
                return;

            // Check if player has the quest item
            if (questService.HasItem(questItemName))
            {
                CompleteQuest();
            }
        }

        /// <summary>
        /// Complete the quest, activate rewards, and update state
        /// </summary>
        public void CompleteQuest()
        {
            if (!hasQuest || questCompleted || !questActive || questService == null)
                return;

            // Mark quest as completed
            questCompleted = true;

            // Remove the quest item from inventory
            GameObject removedItem = questService.RemoveItem(questItemName);
            if (removedItem != null)
            {
                // Destroy the item after removing it
                Destroy(removedItem);
            }

            // Show the reward
            if (questRewardObject != null)
            {
                questRewardObject.SetActive(true);
            }

            // Play quest complete sound
            audioService?.Play(Core.Interfaces.SoundType.QuestComplete);

            logger?.Log($"Quest completed for NPC {gameObject.name}, item: {questItemName}");
        }

        /// <summary>
        /// Mark the quest as active after first interaction
        /// </summary>
        public void ActivateQuest()
        {
            if (!hasQuest || questActive)
                return;

            questActive = true;
            logger?.Log($"Quest activated for NPC {gameObject.name}");
        }

        /// <summary>
        /// Get the current prompt based on quest state
        /// </summary>
        /// <returns>The appropriate prompt for the current quest state</returns>
        public string GetCurrentPrompt()
        {
            if (!hasQuest)
            {
                // No quest, return initial prompt
                return initialPrompt;
            }
            else if (!questActive)
            {
                // First interaction
                return initialPrompt;
            }
            else if (questActive && !questCompleted)
            {
                // Quest in progress
                return string.IsNullOrEmpty(questInProgressPrompt) ? initialPrompt : questInProgressPrompt;
            }
            else if (questCompleted)
            {
                // Quest completed
                return string.IsNullOrEmpty(completedQuestPrompt) ? initialPrompt : completedQuestPrompt;
            }

            // Default fallback
            return initialPrompt;
        }

        /// <summary>
        /// Whether this NPC has a quest
        /// </summary>
        public bool HasQuest => hasQuest;

        /// <summary>
        /// Whether the quest is active
        /// </summary>
        public bool QuestActive
        {
            get => questActive;
            set => questActive = value;
        }

        /// <summary>
        /// Whether the quest is completed
        /// </summary>
        public bool QuestCompleted
        {
            get => questCompleted;
            set => questCompleted = value;
        }

        /// <summary>
        /// The name of the quest item
        /// </summary>
        public string QuestItemName => questItemName;
    }
}