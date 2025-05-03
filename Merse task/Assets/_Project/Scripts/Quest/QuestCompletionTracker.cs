using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core.Interfaces;
using Core.Services;

namespace Quest
{
    /// <summary>
    /// Tracks overall quest completion across all NPCs
    /// </summary>
    public class QuestCompletionTracker : MonoBehaviour, IQuestCompletionTracker
    {
        [Header("Quest Data")]
        [SerializeField, Tooltip("Optional list of quests to register at start")]
        private List<QuestData> initialQuests = new List<QuestData>();

        private Dictionary<string, string> allQuests = new Dictionary<string, string>();
        private HashSet<string> completedQuests = new HashSet<string>();

        private IQuestService questService;
        private ILoggingService logger;
        private bool hasNotifiedAllQuestsCompleted = false;

        /// <summary>
        /// Event triggered when all quests have been completed
        /// </summary>
        public event Action OnAllQuestsCompleted;

        /// <summary>
        /// Event triggered when a specific quest is completed
        /// </summary>
        public event Action<string> OnQuestCompleted;

        private void Awake()
        {
            // Register this with ServiceLocator
            ServiceLocator.Register<IQuestCompletionTracker>(this);
        }

        private void Start()
        {
            // Get services
            questService = ServiceLocator.Get<IQuestService>();
            logger = ServiceLocator.Get<ILoggingService>();

            // Register for quest completion events
            if (questService != null)
            {
                questService.OnQuestCompleted += HandleQuestCompleted;
                logger?.Log("QuestCompletionTracker subscribed to questService.OnQuestCompleted events");
            }
            else
            {
                logger?.LogError("QuestService not available in QuestCompletionTracker");
            }

            // Register initial quests
            foreach (var quest in initialQuests)
            {
                RegisterQuest(quest.questId, quest.questName);
            }
        }

        private void HandleQuestCompleted(string questItemName)
        {
            // Find matching quest ID
            string questId = FindQuestIdByName(questItemName);

            if (!string.IsNullOrEmpty(questId))
            {
                // Mark the quest as completed
                CompleteQuest(questId);
            }
            else
            {
                logger?.LogWarning($"No quest ID found for completed item: {questItemName}");
            }
        }

        /// <summary>
        /// Find a quest ID by its item name
        /// </summary>
        private string FindQuestIdByName(string questItemName)
        {
            foreach (var quest in allQuests)
            {
                if (quest.Value.Equals(questItemName, StringComparison.OrdinalIgnoreCase))
                {
                    return quest.Key;
                }
            }
            return null;
        }

        /// <summary>
        /// Register a quest to be tracked
        /// </summary>
        /// <param name="questId">Unique identifier for the quest</param>
        /// <param name="questName">Human-readable name of the quest</param>
        public void RegisterQuest(string questId, string questName)
        {
            if (string.IsNullOrEmpty(questId) || string.IsNullOrEmpty(questName))
            {
                logger?.LogWarning("Cannot register quest with empty ID or name");
                return;
            }

            if (!allQuests.ContainsKey(questId))
            {
                allQuests.Add(questId, questName);
                logger?.Log($"Registered quest '{questName}' with ID: {questId}");
            }
            else
            {
                logger?.LogWarning($"Quest with ID '{questId}' is already registered");
            }
        }

        /// <summary>
        /// Mark a quest as completed
        /// </summary>
        /// <param name="questId">Unique identifier for the quest</param>
        public void CompleteQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId))
            {
                logger?.LogWarning("Cannot complete quest with empty ID");
                return;
            }

            // Check if this quest exists
            if (!allQuests.ContainsKey(questId))
            {
                logger?.LogWarning($"Cannot complete unregistered quest: {questId}");
                return;
            }

            // Check if the quest is already completed
            if (completedQuests.Contains(questId))
            {
                logger?.Log($"Quest '{questId}' already completed, ignoring");
                return;
            }

            // Mark as completed
            completedQuests.Add(questId);
            string questName = allQuests[questId];

            logger?.Log($"Quest '{questName}' (ID: {questId}) marked as completed");

            // Trigger quest completed event
            OnQuestCompleted?.Invoke(questId);

            // Check if all quests are completed
            CheckAllQuestsCompleted();
        }

        /// <summary>
        /// Check if all registered quests have been completed
        /// </summary>
        /// <returns>True if all quests are completed, false otherwise</returns>
        public bool AreAllQuestsCompleted()
        {
            if (allQuests.Count == 0)
            {
                return false;
            }

            return completedQuests.Count >= allQuests.Count;
        }

        /// <summary>
        /// Check if all quests are completed and trigger event if needed
        /// </summary>
        private void CheckAllQuestsCompleted()
        {
            if (AreAllQuestsCompleted() && !hasNotifiedAllQuestsCompleted)
            {
                hasNotifiedAllQuestsCompleted = true;

                logger?.Log($"All quests completed! Total quests: {allQuests.Count}");
                Debug.Log($"All quests completed! Total quests: {allQuests.Count}");

                // Trigger event
                OnAllQuestsCompleted?.Invoke();
            }
        }

        /// <summary>
        /// Get a list of all completed quests
        /// </summary>
        /// <returns>List of completed quest identifiers</returns>
        public List<string> GetCompletedQuests()
        {
            return completedQuests.ToList();
        }

        /// <summary>
        /// Get a list of all pending quests
        /// </summary>
        /// <returns>List of pending quest identifiers</returns>
        public List<string> GetPendingQuests()
        {
            return allQuests.Keys.Where(questId => !completedQuests.Contains(questId)).ToList();
        }

        /// <summary>
        /// Auto-register all NPCs with quests in the scene
        /// </summary>
        public void AutoRegisterAllQuestsInScene()
        {
            // Find all NPCs with quest components
            NPCQuestState[] questNPCs = FindObjectsOfType<NPCQuestState>();

            int count = 0;
            foreach (var npc in questNPCs)
            {
                if (npc.hasQuest && !string.IsNullOrEmpty(npc.questItemName))
                {
                    // Generate a unique ID for this quest
                    string questId = $"quest_{npc.gameObject.name}_{npc.questItemName}";

                    // Register the quest
                    RegisterQuest(questId, npc.questItemName);
                    count++;
                }
            }

            logger?.Log($"Auto-registered {count} quests from NPCs in the scene");
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (questService != null)
            {
                questService.OnQuestCompleted -= HandleQuestCompleted;
            }
        }
    }

    /// <summary>
    /// Data structure for serializing quest information in the inspector
    /// </summary>
    [System.Serializable]
    public class QuestData
    {
        public string questId;
        public string questName;
    }
}