using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;

// [RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor))]
public class PutItemInInventory : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socketInteractor;
    private Transform collectablesParent;

    // Track the currently held item
    private Transform currentHeldItem;
    private bool wasManuallyGrabbed = false;

    // Store original local scales before reparenting
    private Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();

    private void Awake()
    {
        socketInteractor = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();

        // Find the Collectables GameObject
        collectablesParent = GameObject.Find("Collectables")?.transform;
        if (collectablesParent == null)
        {
            Debug.LogWarning("Collectables GameObject not found in scene!");
        }
    }

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
                // Re-attach it to the socket
                currentHeldItem.SetParent(transform, true);

                // Re-apply the scale
                if (originalScales.TryGetValue(currentHeldItem, out Vector3 originalScale))
                {
                    currentHeldItem.localScale = originalScale * 10f;
                }

                // Try to re-select it using the socket interactor
                socketInteractor.StartManualInteraction(interactable as UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable);
            }
        }
    }

    private void OnDisable()
    {
        socketInteractor.selectEntered.RemoveListener(OnSelectEntered);
        socketInteractor.selectExited.RemoveListener(OnSelectExited);

        // Don't return the item to Collectables when socket is disabled
        // The item will be re-attached when the socket is re-enabled
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (args.interactableObject == null)
            return;

        Transform selected = args.interactableObject.transform;

        // Reset manual grab flag when new item enters socket
        wasManuallyGrabbed = false;

        // Track the current held item
        currentHeldItem = selected;

        // Store original local scale before modifying
        if (!originalScales.ContainsKey(selected))
        {
            originalScales[selected] = selected.localScale;
        }

        // Store world scale before parenting
        Vector3 worldScale = selected.lossyScale;

        // Parent to the socket
        selected.SetParent(transform, true); // true = maintain world pos/rot

        selected.localScale *= 10f;
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (args.interactableObject == null)
            return;

        Transform selected = args.interactableObject.transform;

        // Identify if exited by a hand/controller grab or by socket deactivation
        var interactable = selected.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();

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
        }
        else if (!isSocketDeactivation)
        {
            // If no interactor is currently selecting it, but it's not deactivation
            // Likely grabbed by something else outside XR system, mark as manually grabbed
            wasManuallyGrabbed = true;
        }

        // Return to Collectables if it was manually grabbed by a hand or controller
        if (collectablesParent != null && wasManuallyGrabbed)
        {
            // First reparent to Collectables
            selected.SetParent(collectablesParent, true);

            // THEN restore original scale AFTER reparenting
            if (originalScales.TryGetValue(selected, out Vector3 originalScale))
            {
                selected.localScale = originalScale;
            }
            else
            {
                Debug.LogWarning("Failed to find original scale for " + selected.name);
            }

            // Clear reference and flag
            if (currentHeldItem == selected)
            {
                currentHeldItem = null;
                wasManuallyGrabbed = false;
            }

            // Also remove from scale dictionary if we're done with it
            originalScales.Remove(selected);
        }
        else
        {
            // For non-manual exits, still restore scale but don't reparent
            if (originalScales.TryGetValue(selected, out Vector3 originalScale))
            {
                selected.localScale = originalScale;
            }
        }
    }
}
