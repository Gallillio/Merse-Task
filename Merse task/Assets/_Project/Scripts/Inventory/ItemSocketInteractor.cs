using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;
using Core.Interfaces;
using Core.Services;
using System;

namespace Inventory
{
    /// <summary>
    /// Socket-based inventory system that handles attaching items to sockets
    /// </summary>
    [RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor))]
    public class ItemSocketInteractor : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField, Tooltip("Scale multiplier applied when socket is disabled")]
        private float scaleMultiplier = 4.04f;

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
        /// Event that fires when an item is picked up by this socket
        /// </summary>
        public event Action<GameObject> OnItemPickedUp;

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
                Debug.Log($"DEBUG [SOCKET ENABLE] Socket: {gameObject.name}, Item: {currentHeldItem.name}, Scale: {currentHeldItem.localScale}");

                // Check if the item is not being held by another interactor
                var interactable = currentHeldItem.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
                if (interactable != null && !interactable.isSelected)
                {
                    Vector3 scaleBefore = currentHeldItem.localScale;

                    // Re-attach using inventory service
                    inventoryService.AttachItem(currentHeldItem, transform);

                    // Log the scale change after reattachment
                    Debug.Log($"DEBUG [SOCKET REATTACH] Socket: {gameObject.name}, Item: {currentHeldItem.name}, Scale before: {scaleBefore}, Scale after: {currentHeldItem.localScale}");

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

            // Apply 10x scale multiplier when socket is disabled to compensate for small parent scale
            if (currentHeldItem != null)
            {
                Debug.Log($"DEBUG [SOCKET DISABLE] Socket: {gameObject.name}, Item: {currentHeldItem.name}, Current scale: {currentHeldItem.localScale}");

                // Apply configurable scale multiplier to compensate for parent scale
                Vector3 compensationScale = new Vector3(scaleMultiplier, scaleMultiplier, scaleMultiplier);
                currentHeldItem.localScale = compensationScale;

                Debug.Log($"DEBUG [SOCKET DISABLE APPLIED COMPENSATION] Item: {currentHeldItem.name}, New scale: {currentHeldItem.localScale}, Applied {scaleMultiplier}x multiplier");
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
            Debug.Log($"DEBUG [ITEM ENTER SOCKET] Socket: {gameObject.name}, Item: {selected.name}, Scale: {selected.localScale}, World scale: {selected.lossyScale}");

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

            Vector3 scaleBefore = selected.localScale;

            // Use inventory service to handle attachment
            inventoryService.AttachItem(selected, transform);

            Debug.Log($"DEBUG [AFTER SOCKET ATTACH] Socket: {gameObject.name}, Item: {selected.name}, Scale before: {scaleBefore}, Scale after: {selected.localScale}");

            // Trigger the OnItemPickedUp event
            OnItemPickedUp?.Invoke(selected.gameObject);
        }

        /// <summary>
        /// Handle when an item leaves the socket
        /// </summary>
        private void OnSelectExited(SelectExitEventArgs args)
        {
            if (args.interactableObject == null)
                return;

            Transform selected = args.interactableObject.transform;
            Debug.Log($"DEBUG [ITEM EXIT SOCKET] Socket: {gameObject.name}, Item: {selected.name}, Current scale: {selected.localScale}");

            // Check if we're being deactivated
            bool isSocketDeactivation = !gameObject.activeInHierarchy;

            // Check if this is a manual grab by examining the args.interactorObject
            // If the interactor is not null and it's not this socket, it's likely a manual grab
            bool isManualGrab = args.interactorObject != null &&
                               args.interactorObject.transform != socketInteractor.transform;

            Debug.Log($"DEBUG [EXIT TYPE] Socket: {gameObject.name}, Item: {selected.name}, Manual grab: {isManualGrab}, Socket deactivation: {isSocketDeactivation}");

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

            Vector3 scaleBeforeDetach = selected.localScale;

            // Return to Collectables if it was manually grabbed by a hand or controller
            if (wasManuallyGrabbed)
            {
                // Get original scale from the inventory service
                Vector3 originalScale = inventoryService.GetOriginalScale(selected);
                Debug.Log($"DEBUG [GETTING ORIGINAL SCALE] Item: {selected.name}, Original scale: {originalScale}");

                // Use inventory service to handle detachment
                inventoryService.DetachItem(selected);

                Debug.Log($"DEBUG [AFTER DETACH] Socket: {gameObject.name}, Item: {selected.name}, Scale before: {scaleBeforeDetach}, Scale after: {selected.localScale}");

                // Clear reference and flag
                if (currentHeldItem == selected)
                {
                    currentHeldItem = null;
                    wasManuallyGrabbed = false;
                }
            }
            else
            {
                Debug.Log($"DEBUG [NO DETACH] Socket: {gameObject.name}, Item: {selected.name}, Not manually grabbed, scale: {selected.localScale}");
            }
        }
    }
}