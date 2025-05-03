using System;
using System.Collections.Generic;

namespace Core.Interfaces
{
    /// <summary>
    /// Interface for tracking overall quest completion
    /// </summary>
    public interface IQuestCompletionTracker
    {
        /// <summary>
        /// Register a quest to be tracked
        /// </summary>
        /// <param name="questId">Unique identifier for the quest</param>
        /// <param name="questName">Human-readable name of the quest</param>
        void RegisterQuest(string questId, string questName);

        /// <summary>
        /// Mark a quest as completed
        /// </summary>
        /// <param name="questId">Unique identifier for the quest</param>
        void CompleteQuest(string questId);

        /// <summary>
        /// Check if all registered quests have been completed
        /// </summary>
        /// <returns>True if all quests are completed, false otherwise</returns>
        bool AreAllQuestsCompleted();

        /// <summary>
        /// Get a list of all completed quests
        /// </summary>
        /// <returns>List of completed quest identifiers</returns>
        List<string> GetCompletedQuests();

        /// <summary>
        /// Get a list of all pending quests
        /// </summary>
        /// <returns>List of pending quest identifiers</returns>
        List<string> GetPendingQuests();

        /// <summary>
        /// Event triggered when all quests have been completed
        /// </summary>
        event Action OnAllQuestsCompleted;

        /// <summary>
        /// Event triggered when a specific quest is completed
        /// </summary>
        event Action<string> OnQuestCompleted;
    }
}