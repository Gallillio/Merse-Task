using UnityEngine;
using System.Collections;

namespace Core.Interfaces
{
    /// <summary>
    /// Interface for audio playback services
    /// </summary>
    public interface IAudioService
    {
        /// <summary>
        /// Play a sound effect or music
        /// </summary>
        /// <param name="type">The type of sound to play</param>
        /// <param name="volume">Volume level (0-1)</param>
        /// <param name="loop">Whether the sound should loop</param>
        void Play(SoundType type, float volume = 1f, bool loop = false);

        /// <summary>
        /// Play an NPC voice sound with a specific duration
        /// </summary>
        /// <param name="duration">How long the voice should play</param>
        /// <param name="volume">Volume level (0-1)</param>
        void PlayNPCVoice(float duration, float volume = 1f);

        /// <summary>
        /// Stop any currently playing NPC voice
        /// </summary>
        void StopNPCVoice();

        /// <summary>
        /// Reduce background music volume for a conversation
        /// </summary>
        void StartConversation();

        /// <summary>
        /// Restore background music volume after a conversation
        /// </summary>
        void EndConversation();
    }
}