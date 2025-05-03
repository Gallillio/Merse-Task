namespace Core.Interfaces
{
    /// <summary>
    /// Types of sounds available in the system
    /// </summary>
    public enum SoundType
    {
        /// <summary>
        /// Background music that plays continuously
        /// </summary>
        BackgroundMusic,

        /// <summary>
        /// Sound played when picking up items
        /// </summary>
        ItemPickup,

        /// <summary>
        /// Voice sound for NPCs when speaking
        /// </summary>
        NPCTalking,

        /// <summary>
        /// Sound played when completing a quest
        /// </summary>
        QuestComplete
    }
}