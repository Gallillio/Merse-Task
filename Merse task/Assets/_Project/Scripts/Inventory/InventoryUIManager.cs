using UnityEngine;
using Core.Interfaces;
using Core.Services;
using System.Collections;

namespace Inventory
{
    /// <summary>
    /// Manages the inventory UI and visual feedback
    /// </summary>
    public class InventoryUIManager : MonoBehaviour
    {
        [Header("Socket Hover Visuals")]
        [SerializeField] private bool enableHoverFeedback = true;
        [SerializeField] private Color hoverColor = Color.red;
        [SerializeField] private Color defaultColor = Color.white;

        [Header("Settings")]
        [SerializeField] private float setupDelay = 0.5f;

        private ILoggingService logger;

        private void Start()
        {
            // Get required services
            logger = ServiceLocator.Get<ILoggingService>();

            // Setup hover visuals after a short delay to ensure all sockets are initialized
            if (enableHoverFeedback)
            {
                StartCoroutine(SetupSocketVisualsDelayed());
            }
        }

        /// <summary>
        /// Apply hover visuals to all sockets with a delay to ensure everything is loaded
        /// </summary>
        private IEnumerator SetupSocketVisualsDelayed()
        {
            // Wait for other components to initialize
            yield return new WaitForSeconds(setupDelay);

            // Add hover visual feedback
            SetupSocketHoverVisuals();
        }

        /// <summary>
        /// Add hover visual feedback to all inventory sockets in the scene
        /// </summary>
        public void SetupSocketHoverVisuals()
        {
            ItemSocketInteractorExtensions.ConfigureHoverVisualForAllSockets(hoverColor, defaultColor);
            logger?.Log("Added hover visual feedback to all inventory sockets");
        }

        /// <summary>
        /// Add hover visual feedback to a specific socket
        /// </summary>
        /// <param name="socket">The socket to add hover feedback to</param>
        public void ConfigureSocketHoverVisual(ItemSocketInteractor socket)
        {
            if (socket != null)
            {
                socket.ConfigureHoverVisual(hoverColor, defaultColor);
                logger?.Log($"Configured hover visual on socket {socket.name}");
            }
            else
            {
                logger?.LogWarning("Cannot configure hover visual for null socket");
            }
        }
    }
}