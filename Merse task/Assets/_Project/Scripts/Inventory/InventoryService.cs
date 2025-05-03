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

            // Store original scale before modifying
            if (!originalScales.ContainsKey(item))
            {
                originalScales[item] = item.localScale;
                logger?.Log($"Stored original scale for {item.name}");
            }

            // Parent to the socket
            item.SetParent(socket, true); // true = maintain world pos/rot

            // Apply socket-specific scale adjustment (commonly used in VR)
            item.localScale *= 10f;

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

            // Move the item back to collectables if available
            if (collectablesParent != null)
            {
                // First reparent to collectables
                item.SetParent(collectablesParent, true);

                // Restore original scale
                if (originalScales.TryGetValue(item, out Vector3 originalScale))
                {
                    item.localScale = originalScale;
                    originalScales.Remove(item);
                }
                else
                {
                    logger?.LogWarning($"Failed to find original scale for {item.name}");
                }

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
        /// Set the collectables parent
        /// </summary>
        /// <param name="collectablesTransform">The transform to use as collectables parent</param>
        public void SetCollectablesParent(Transform collectablesTransform)
        {
            if (collectablesTransform != null)
            {
                collectablesParent = collectablesTransform;
                logger?.Log($"Set collectables parent to {collectablesParent.name}");
            }
            else
            {
                logger?.LogWarning("Attempted to set null collectables parent");
            }
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

            return Vector3.one;
        }
    }
}