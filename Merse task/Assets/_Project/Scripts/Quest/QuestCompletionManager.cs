using UnityEngine;
using Core.Interfaces;
using Core.Services;
using System.Collections.Generic;

namespace Quest
{
    /// <summary>
    /// Handles game-specific behavior when quests are completed
    /// </summary>
    public class QuestCompletionManager : MonoBehaviour
    {
        [Header("Auto-Registration")]
        [Tooltip("Automatically register all NPCs with quests in the scene")]
        [SerializeField] private bool autoRegisterQuests = true;

        [Header("Quest Settings")]
        [Tooltip("Optional list of quests to register at start")]
        [SerializeField] private List<QuestRequirement> questRequirements = new List<QuestRequirement>();

        [Header("Events")]
        [Tooltip("Objects to activate when all quests are completed")]
        [SerializeField] private List<GameObject> activateOnAllQuestsCompleted = new List<GameObject>();

        [Tooltip("Objects to deactivate when all quests are completed")]
        [SerializeField] private List<GameObject> deactivateOnAllQuestsCompleted = new List<GameObject>();

        [Header("Specific Objects")]
        [Tooltip("Reference to 'The Great Wall of America' to disable when all quests are completed")]
        [SerializeField] private GameObject theGreatWallOfAmerica;

        private IQuestCompletionTracker questTracker;
        private ILoggingService logger;

        private void Start()
        {
            // Get services
            questTracker = ServiceLocator.Get<IQuestCompletionTracker>();
            logger = ServiceLocator.Get<ILoggingService>();

            if (questTracker == null)
            {
                logger?.LogError("QuestCompletionTracker not available in QuestCompletionManager");
                enabled = false;
                return;
            }

            // Register for events
            questTracker.OnAllQuestsCompleted += HandleAllQuestsCompleted;
            questTracker.OnQuestCompleted += HandleQuestCompleted;

            // Register manually specified quests
            RegisterManualQuests();

            // Auto-register quests if enabled
            if (autoRegisterQuests && questTracker is QuestCompletionTracker tracker)
            {
                tracker.AutoRegisterAllQuestsInScene();
            }

            // Try to find "The Great Wall of America" if not assigned
            if (theGreatWallOfAmerica == null)
            {
                theGreatWallOfAmerica = GameObject.Find("The Great Wall of America");
                if (theGreatWallOfAmerica != null)
                {
                    logger?.Log("Found 'The Great Wall of America' GameObject");
                }
            }
        }

        private void RegisterManualQuests()
        {
            foreach (var quest in questRequirements)
            {
                questTracker.RegisterQuest(quest.questId, quest.questName);
            }
        }

        private void HandleQuestCompleted(string questId)
        {
            logger?.Log($"Quest completed: {questId}");

            // Display progress
            List<string> completed = questTracker.GetCompletedQuests();
            List<string> pending = questTracker.GetPendingQuests();

            Debug.Log($"Quest Progress: {completed.Count}/{completed.Count + pending.Count} completed");
        }

        private void HandleAllQuestsCompleted()
        {
            logger?.Log("All quests have been completed!");
            Debug.Log("All quests have been completed! Game completed!");

            // Activate reward objects
            foreach (var obj in activateOnAllQuestsCompleted)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                    logger?.Log($"Activated reward object: {obj.name}");
                }
            }

            // Deactivate specified objects
            foreach (var obj in deactivateOnAllQuestsCompleted)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                    logger?.Log($"Deactivated object: {obj.name}");
                }
            }

            // Handle the Great Wall specifically
            if (theGreatWallOfAmerica != null)
            {
                theGreatWallOfAmerica.SetActive(false);
                logger?.Log("The Great Wall of America has been disabled! Freedom!");
            }
            else
            {
                logger?.LogWarning("Could not find 'The Great Wall of America' to disable");
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (questTracker != null)
            {
                questTracker.OnAllQuestsCompleted -= HandleAllQuestsCompleted;
                questTracker.OnQuestCompleted -= HandleQuestCompleted;
            }
        }
    }

    /// <summary>
    /// Data structure for serializing quest requirements in the inspector
    /// </summary>
    [System.Serializable]
    public class QuestRequirement
    {
        public string questId;
        public string questName;
    }
}