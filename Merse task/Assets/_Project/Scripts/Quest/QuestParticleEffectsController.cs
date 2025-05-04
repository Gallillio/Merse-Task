using UnityEngine;
using Core.Interfaces;
using Core.Services;

namespace Quest
{
    /// <summary>
    /// Handles activation of particle effects when a quest is completed
    /// Follows SOLID principles by having a single responsibility and using dependency injection
    /// </summary>
    [RequireComponent(typeof(NPCQuestState))]
    public class QuestParticleEffectsController : MonoBehaviour
    {
        [Header("Particles")]
        [Tooltip("The game object containing particle systems to activate when quest is completed")]
        [SerializeField] private GameObject questCompletionParticles;

        [Header("Activation Settings")]
        [Tooltip("Whether to activate particles automatically when quest sound plays")]
        [SerializeField] private bool activateWithQuestCompleteSound = true;

        private IAudioService audioService;
        private NPCQuestState questState;
        private ILoggingService logger;

        private bool isListeningForSound = false;
        private bool particlesActivated = false;

        private void Awake()
        {
            // Get references
            questState = GetComponent<NPCQuestState>();

            // Ensure the particles are initially disabled
            if (questCompletionParticles != null)
            {
                questCompletionParticles.SetActive(false);
            }
            else
            {
                // Try to find particles in parent object if not directly assigned
                Transform parent = transform.parent;
                if (parent != null)
                {
                    // Look for a child named "Complete Quest Particles"
                    Transform particlesTransform = parent.Find("Complete Quest Particles");
                    if (particlesTransform != null)
                    {
                        questCompletionParticles = particlesTransform.gameObject;
                        questCompletionParticles.SetActive(false);
                    }
                }
            }
        }

        private void Start()
        {
            // Get services
            audioService = ServiceLocator.Get<IAudioService>();
            logger = ServiceLocator.Get<ILoggingService>();

            // Subscribe to audio events if available
            if (audioService != null && activateWithQuestCompleteSound)
            {
                audioService.OnSoundPlayed += HandleSoundPlayed;
                isListeningForSound = true;
                logger?.Log($"QuestParticleEffectsController subscribed to audio events for {gameObject.name}");
            }

            // Validate setup
            if (questCompletionParticles == null)
            {
                logger?.LogWarning($"No quest completion particles assigned for {gameObject.name}. " +
                                  "Either assign directly or create a child GameObject named 'Complete Quest Particles'");
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (audioService != null && isListeningForSound)
            {
                audioService.OnSoundPlayed -= HandleSoundPlayed;
            }
        }

        /// <summary>
        /// Handle sound events from the audio service
        /// </summary>
        private void HandleSoundPlayed(Core.Interfaces.SoundType soundType)
        {
            // Only respond to quest completion sounds
            if (soundType == Core.Interfaces.SoundType.QuestComplete)
            {
                // Check if this NPC's quest is completed
                if (questState != null && questState.questCompleted)
                {
                    ActivateParticles();
                }
            }
        }

        /// <summary>
        /// Manually activate the quest completion particles
        /// </summary>
        public void ActivateParticles()
        {
            if (questCompletionParticles != null && !particlesActivated)
            {
                questCompletionParticles.SetActive(true);
                particlesActivated = true;
                logger?.Log($"Activated quest completion particles for {gameObject.name}");
            }
        }

        /// <summary>
        /// Manually deactivate the quest completion particles
        /// </summary>
        public void DeactivateParticles()
        {
            if (questCompletionParticles != null && particlesActivated)
            {
                questCompletionParticles.SetActive(false);
                particlesActivated = false;
                logger?.Log($"Deactivated quest completion particles for {gameObject.name}");
            }
        }
    }
}