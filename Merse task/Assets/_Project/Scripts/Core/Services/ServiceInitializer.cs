using UnityEngine;
using Core.Interfaces;

namespace Core.Services
{
    /// <summary>
    /// Initializes and registers all services in the application
    /// </summary>
    public class ServiceInitializer : MonoBehaviour
    {
        [Header("Services")]
        [SerializeField] private MonoBehaviour audioServiceImplementation;
        [SerializeField] private MonoBehaviour dialogueProviderImplementation;
        [SerializeField] private MonoBehaviour questServiceImplementation;
        [SerializeField] private MonoBehaviour inventoryServiceImplementation;
        [SerializeField] private MonoBehaviour activeConversationManagerImplementation;
        [SerializeField] private MonoBehaviour questCompletionTrackerImplementation;

        [Header("Debug")]
        [SerializeField] private bool logServiceRegistration = true;

        /// <summary>
        /// Make sure this component gets loaded before others
        /// </summary>
        private void Awake()
        {
            InitializeServices();
        }

        /// <summary>
        /// Initialize and register all services
        /// </summary>
        private void InitializeServices()
        {
            // Initialize the service locator
            ServiceLocator.Initialize();

            // Register services
            RegisterService<IAudioService>(audioServiceImplementation);
            RegisterService<IDialogueProvider>(dialogueProviderImplementation);
            RegisterService<IQuestService>(questServiceImplementation);
            RegisterService<IInventoryService>(inventoryServiceImplementation);
            RegisterService<IActiveConversationManager>(activeConversationManagerImplementation);
            RegisterService<IQuestCompletionTracker>(questCompletionTrackerImplementation);

            // Create and register logging service
            LoggingService loggingService = new LoggingService(logServiceRegistration);
            ServiceLocator.Register<ILoggingService>(loggingService);

            if (logServiceRegistration)
            {
                Debug.Log("All services registered successfully");
            }
        }

        /// <summary>
        /// Register a service with the service locator
        /// </summary>
        private void RegisterService<T>(MonoBehaviour implementation) where T : class
        {
            if (implementation != null)
            {
                T service = implementation as T;
                if (service != null)
                {
                    ServiceLocator.Register<T>(service);
                    if (logServiceRegistration)
                    {
                        Debug.Log($"Registered service: {typeof(T).Name}");
                    }
                }
                else
                {
                    Debug.LogError($"Implementation of {typeof(T).Name} is not valid. Check your references.");
                }
            }
            else if (logServiceRegistration)
            {
                Debug.LogWarning($"No implementation provided for {typeof(T).Name}");
            }
        }
    }
}