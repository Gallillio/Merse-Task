using System;
using System.Collections.Generic;
using UnityEngine;
using Core.Interfaces;
using Core.Services;

namespace Inventory
{
    /// <summary>
    /// Service that manages item attachment and detachment to inventory sockets
    /// </summary>
    public class InventoryService : MonoBehaviour, IInventoryService
    {
        [Header("References")]
        [SerializeField] private Transform collectablesParent;

        [Header("Debug")]
        [SerializeField] private bool verboseLogging = true;

        private IAudioService audioService;
        private ILoggingService logger;
        private Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();

        /// <summary>
        /// Event triggered when an item is attached to a socket
        /// </summary>
        public event Action<Transform, Transform> OnItemAttached;

        private void Awake()
        {
            // Find the collectables parent if not assigned
            if (collectablesParent == null)
            {
                collectablesParent = GameObject.Find("Collectables")?.transform;
                if (collectablesParent == null)
                {
                    Debug.LogWarning("Collectables GameObject not found in scene!");
                }
            }
        }

        private void Start()
        {
            // Get required services
            audioService = ServiceLocator.Get<IAudioService>();
            logger = ServiceLocator.Get<ILoggingService>();

            if (audioService == null)
            {
                logger?.LogWarning("AudioService not available in InventoryService");
            }
        }

        /// <summary>
        /// Attach an item to a socket
        /// </summary>
        /// <param name="item">The item transform</param>
        /// <param name="socket">The socket transform</param>
        public void AttachItem(Transform item, Transform socket)
        {
            if (item == null || socket == null)
            {
                logger?.LogWarning("Cannot attach item: item or socket is null");
                return;
            }

            // VERBOSE DEBUG: Log item's scale before any operations
            Debug.Log($"DEBUG [BEFORE ATTACH] Item: {item.name}, Current scale: {item.localScale}, World scale: {item.lossyScale}");

            // Remember current scale for debugging
            Vector3 currentScale = item.localScale;
            Vector3 scaleBeforeParenting = item.localScale;
            Transform parentBeforeAttach = item.parent;
            Debug.Log($"DEBUG [PRE-PARENT] Item: {item.name}, Scale: {scaleBeforeParenting}, Parent: {(parentBeforeAttach ? parentBeforeAttach.name : "null")}");

            // Store the world position and rotation before changing parent
            Vector3 worldPosition = item.position;
            Quaternion worldRotation = item.rotation;

            // Parent to the socket WITHOUT preserving world position/rotation
            item.SetParent(socket, false);

            // Restore world position and rotation
            item.position = worldPosition;
            item.rotation = worldRotation;

            // Preserve the scale that was set prior to parenting
            item.localScale = scaleBeforeParenting;

            Debug.Log($"DEBUG [POST-PARENT] Item: {item.name}, Scale before: {scaleBeforeParenting}, Scale after: {item.localScale}, Parent: {socket.name}");

            // Trigger event
            OnItemAttached?.Invoke(item, socket);

            logger?.Log($"Item {item.name} attached to socket {socket.name}");
        }

        /// <summary>
        /// Detach an item from its socket and return it to collectables
        /// </summary>
        /// <param name="item">The item transform</param>
        public void DetachItem(Transform item)
        {
            if (item == null)
            {
                logger?.LogWarning("Cannot detach null item");
                return;
            }

            // VERBOSE DEBUG: Log item's scale before any operations
            Debug.Log($"DEBUG [BEFORE DETACH] Item: {item.name}, Current scale: {item.localScale}, Parent: {(item.parent ? item.parent.name : "null")}");

            // Store the item's world position and rotation before reparenting
            Vector3 worldPosition = item.position;
            Quaternion worldRotation = item.rotation;

            // Always use Vector3.one when detaching items
            Debug.Log($"DEBUG [SCALE DETACH] Item: {item.name}, Current: {item.localScale}, Setting to Vector3.one");
            Vector3 scaleBeforeDetach = item.localScale;
            item.localScale = Vector3.one;

            // Move the item back to collectables if available
            if (collectablesParent != null)
            {
                // Reparent to collectables WITHOUT preserving world position
                item.SetParent(collectablesParent, false);

                // Restore the world position and rotation
                item.position = worldPosition;
                item.rotation = worldRotation;

                // Ensure scale is exactly (1,1,1)
                item.localScale = Vector3.one;

                Debug.Log($"DEBUG [AFTER REPARENT] Item: {item.name}, Scale before: {scaleBeforeDetach}, Scale after: {item.localScale}, Parent: {collectablesParent.name}");

                // Keep the original scale in memory in case we need it again
                logger?.Log($"Item {item.name} detached and returned to collectables");
            }
            else
            {
                logger?.LogWarning("No collectables parent available for detached item");
            }
        }

        /// <summary>
        /// Play sound effect for item pickup
        /// </summary>
        /// <param name="item">The item being picked up</param>
        public void PlayPickupSound(Transform item)
        {
            if (item == null)
            {
                logger?.LogWarning("Cannot play pickup sound for null item");
                return;
            }

            audioService?.Play(Core.Interfaces.SoundType.ItemPickup);
            logger?.Log($"Played pickup sound for {item.name}");
        }

        /// <summary>
        /// Get the stored original scale for an item
        /// </summary>
        /// <param name="item">The item to get the scale for</param>
        /// <returns>The original scale, or Vector3.one if not found</returns>
        public Vector3 GetOriginalScale(Transform item)
        {
            if (item == null)
                return Vector3.one;

            if (originalScales.TryGetValue(item, out Vector3 scale))
            {
                return scale;
            }

            Debug.LogWarning($"DEBUG [NO STORED SCALE] Item: {item.name}, Returning Vector3.one as fallback");
            return Vector3.one;
        }
    }
}