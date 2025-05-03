using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using Core.Interfaces;
using Core.Services;

namespace Inventory
{
    /// <summary>
    /// Socket-based inventory system that handles attaching items to sockets
    /// </summary>
    [RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor))]
    public class ItemSocketInteractor : MonoBehaviour
    {
        private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socketInteractor;
        private IInventoryService inventoryService;
        private IQuestService questService;
        private ILoggingService logger;

        // Track the currently held item
        private Transform currentHeldItem;
        private bool wasManuallyGrabbed = false;

        // Track items that have already played their pickup sound
        private HashSet<Transform> itemsPlayedSound = new HashSet<Transform>();

        /// <summary>
        /// Initialize components and find references
        /// </summary>
        private void Awake()
        {
            socketInteractor = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();

            if (socketInteractor == null)
            {
                Debug.LogError($"Missing XRSocketInteractor component on {gameObject.name}");
                enabled = false;
                return;
            }
        }

        private void Start()
        {
            // Get services
            inventoryService = ServiceLocator.Get<IInventoryService>();
            questService = ServiceLocator.Get<IQuestService>();
            logger = ServiceLocator.Get<ILoggingService>();

            if (inventoryService == null)
            {
                logger?.LogError($"InventoryService not available in {gameObject.name}");
                enabled = false;
                return;
            }

            // Register this socket with the quest service
            questService?.RegisterSocket(transform);

            logger?.Log($"Socket {gameObject.name} initialized and registered");
        }

        /// <summary>
        /// Set up event listeners when socket is enabled
        /// </summary>
        private void OnEnable()
        {
            socketInteractor.selectEntered.AddListener(OnSelectEntered);
            socketInteractor.selectExited.AddListener(OnSelectExited);

            // Re-attach the item if we have one and it's not being held by something else
            if (currentHeldItem != null)
            {
                // Check if the item is not being held by another interactor
                var interactable = currentHeldItem.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
                if (interactable != null && !interactable.isSelected)
                {
                    // Re-attach using inventory service
                    inventoryService.AttachItem(currentHeldItem, transform);

                    // Try to re-select it using the socket interactor
                    socketInteractor.StartManualInteraction(interactable as UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable);
                }
            }
        }

        /// <summary>
        /// Clean up event listeners when socket is disabled
        /// </summary>
        private void OnDisable()
        {
            socketInteractor.selectEntered.RemoveListener(OnSelectEntered);
            socketInteractor.selectExited.RemoveListener(OnSelectExited);

            // Reset item scale when socket is disabled to prevent scaling issues
            if (currentHeldItem != null && inventoryService != null)
            {
                // Get the original scale from inventory service and apply it
                Vector3 originalScale = inventoryService.GetOriginalScale(currentHeldItem);

                // Only apply if different to avoid unnecessary operations
                if (currentHeldItem.localScale != originalScale)
                {
                    currentHeldItem.localScale = originalScale;
                    logger?.Log($"Reset scale on disable for {currentHeldItem.name}: {originalScale}");
                }
            }
        }

        /// <summary>
        /// Handle when an item enters the socket
        /// </summary>
        private void OnSelectEntered(SelectEnterEventArgs args)
        {
            if (args.interactableObject == null)
                return;

            Transform selected = args.interactableObject.transform;

            // Only play sound if this is a new item or was manually grabbed before
            if (!itemsPlayedSound.Contains(selected) || wasManuallyGrabbed)
            {
                inventoryService.PlayPickupSound(selected);
                itemsPlayedSound.Add(selected);
            }

            // Reset manual grab flag when new item enters socket
            wasManuallyGrabbed = false;

            // Track the current held item
            currentHeldItem = selected;

            // Use inventory service to handle attachment
            inventoryService.AttachItem(selected, transform);
        }

        /// <summary>
        /// Handle when an item leaves the socket
        /// </summary>
        private void OnSelectExited(SelectExitEventArgs args)
        {
            if (args.interactableObject == null)
                return;

            Transform selected = args.interactableObject.transform;

            // Check if we're being deactivated
            bool isSocketDeactivation = !gameObject.activeInHierarchy;

            // Check if this is a manual grab by examining the args.interactorObject
            // If the interactor is not null and it's not this socket, it's likely a manual grab
            bool isManualGrab = args.interactorObject != null &&
                               args.interactorObject.transform != socketInteractor.transform;

            // Log what interactor is taking the item
            if (isManualGrab)
            {
                wasManuallyGrabbed = true;
                // Remove from played sound list when manually grabbed
                itemsPlayedSound.Remove(selected);
            }
            else if (!isSocketDeactivation)
            {
                // If no interactor is currently selecting it, but it's not deactivation
                // Likely grabbed by something else outside XR system, mark as manually grabbed
                wasManuallyGrabbed = true;
                // Remove from played sound list when manually grabbed
                itemsPlayedSound.Remove(selected);
            }

            // Return to Collectables if it was manually grabbed by a hand or controller
            if (wasManuallyGrabbed)
            {
                // Use inventory service to handle detachment
                inventoryService.DetachItem(selected);

                // Clear reference and flag
                if (currentHeldItem == selected)
                {
                    currentHeldItem = null;
                    wasManuallyGrabbed = false;
                }
            }
        }
    }
}