using UnityEngine;
using System;

namespace Core.Interfaces
{
    /// <summary>
    /// Interface for dialogue generation and display
    /// </summary>
    public interface IDialogueProvider
    {
        /// <summary>
        /// Start a dialogue interaction with an NPC
        /// </summary>
        /// <param name="npc">The NPC GameObject</param>
        /// <param name="input">The user's input text</param>
        /// <param name="instruction">Custom instruction for this dialogue</param>
        /// <param name="onResponse">Callback for when a response is received</param>
        void StartDialogue(GameObject npc, string input, string instruction, Action<string> onResponse);

        /// <summary>
        /// Advance to the next sentence in the dialogue
        /// </summary>
        void AdvanceDialogue();

        /// <summary>
        /// Event triggered when a new sentence is displayed
        /// </summary>
        event Action<string> OnSentenceDisplayed;

        /// <summary>
        /// Event triggered when all sentences have been displayed
        /// </summary>
        event Action OnDialogueCompleted;
    }
}