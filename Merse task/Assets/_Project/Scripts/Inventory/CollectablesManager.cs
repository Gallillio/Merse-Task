using UnityEngine;
using System.Collections.Generic;
using Core.Interfaces;
using Core.Services;
using UnityEngine.XR.Interaction.Toolkit;

namespace Inventory
{
    /// <summary>
    /// Manager for multiple collectables - attach to the parent "Collectables" GameObject
    /// </summary>
    public class CollectablesManager : MonoBehaviour
    {
        [Header("Spin Settings")]
        [Tooltip("Speed of rotation in degrees per second")]
        [SerializeField] private float spinSpeed = 90f;

        [Tooltip("Axis to rotate around")]
        [SerializeField] private Vector3 spinAxis = Vector3.up;

        [Tooltip("Optional bobbing motion")]
        [SerializeField] private bool enableBobbing = true;

        [Tooltip("Bobbing height in units")]
        [SerializeField] private float bobbingHeight = 0.1f;

        [Tooltip("Bobbing speed in cycles per second")]
        [SerializeField] private float bobbingSpeed = 1f;

        // Dictionary to track collectables and their current positions
        private Dictionary<Transform, Vector3> collectables = new Dictionary<Transform, Vector3>();

        // Keep track of original children to detect when they're dropped
        private List<Transform> originalChildren = new List<Transform>();

        // Track items currently held by a player's hand
        private List<Transform> heldItems = new List<Transform>();

        private ILoggingService logger;

        private void Awake()
        {
            logger = ServiceLocator.Get<ILoggingService>();

            // Register all child objects as collectables
            RegisterAllChildren();
        }

        private void Start()
        {
            // Find all socket interactors in the scene and subscribe to their events
            FindAndSubscribeToInteractors();
        }

        private void RegisterAllChildren()
        {
            // Get all immediate children of this GameObject
            foreach (Transform child in transform)
            {
                // Store the child's current position
                collectables[child] = child.position;
                // Add to original children list for tracking
                originalChildren.Add(child);
                logger?.Log($"Registered collectable: {child.name}");
            }

            logger?.Log($"Registered {collectables.Count} collectables for spinning effect");
        }

        private void FindAndSubscribeToInteractors()
        {
            // Find all ItemSocketInteractors in the scene
            ItemSocketInteractor[] socketInteractors = FindObjectsOfType<ItemSocketInteractor>();

            foreach (var socketInteractor in socketInteractors)
            {
                socketInteractor.OnItemPickedUp += OnItemPickedUp;
                logger?.Log($"Subscribed to socket interactor: {socketInteractor.name}");
            }

            // Find all XR Direct interactors to detect when items are dropped
            var grabInteractors = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor>();
            foreach (var grabInteractor in grabInteractors)
            {
                grabInteractor.selectEntered.AddListener(OnItemGrabbed);
                grabInteractor.selectExited.AddListener(OnItemReleased);
                logger?.Log($"Subscribed to XR Grab Interactor: {grabInteractor.name}");
            }
        }

        private void OnItemPickedUp(GameObject item)
        {
            // Check if the item is one of our collectables
            if (collectables.ContainsKey(item.transform))
            {
                // Remove it from our spinning collectables
                collectables.Remove(item.transform);
                logger?.Log($"Item {item.name} picked up into inventory, removed from spinning collectables");
            }
        }

        private void OnItemGrabbed(SelectEnterEventArgs args)
        {
            if (args.interactableObject != null)
            {
                Transform itemTransform = args.interactableObject.transform;

                // Check if this is one of our original collectables
                if (originalChildren.Contains(itemTransform))
                {
                    // Remove from spinning items while it's being held
                    if (collectables.ContainsKey(itemTransform))
                    {
                        collectables.Remove(itemTransform);
                    }

                    // Add to held items list
                    if (!heldItems.Contains(itemTransform))
                    {
                        heldItems.Add(itemTransform);
                    }

                    logger?.Log($"Item {itemTransform.name} grabbed by player, temporarily removed from spinning");
                }
            }
        }

        private void OnItemReleased(SelectExitEventArgs args)
        {
            if (args.interactableObject != null)
            {
                Transform itemTransform = args.interactableObject.transform;

                // Check if this is one of our original collectables
                if (originalChildren.Contains(itemTransform))
                {
                    // Remove from held items list
                    heldItems.Remove(itemTransform);

                    // Wait a frame to ensure the item has settled at its new position
                    StartCoroutine(AddToSpinningItemsNextFrame(itemTransform));
                }
            }
        }

        private System.Collections.IEnumerator AddToSpinningItemsNextFrame(Transform item)
        {
            // Wait for physics to settle (increase this if needed for more stability)
            yield return new WaitForSeconds(0.1f);

            // Only add back to spinning items if not in an inventory socket
            if (!IsInInventory(item))
            {
                // Get current position and make a copy
                Vector3 currentPos = item.position;

                // Update with current position as the new rest position
                collectables[item] = currentPos;
                logger?.Log($"Item {item.name} released at position {currentPos}, resuming spinning at new location");
            }
        }

        private bool IsInInventory(Transform item)
        {
            // Check if this item is parented to any inventory socket
            ItemSocketInteractor[] sockets = FindObjectsOfType<ItemSocketInteractor>();
            foreach (var socket in sockets)
            {
                if (item.parent == socket.transform)
                {
                    return true;
                }
            }
            return false;
        }

        private void Update()
        {
            // Apply spinning to all registered collectables
            foreach (var entry in collectables)
            {
                Transform collectable = entry.Key;
                Vector3 currentRestPosition = entry.Value;

                // Spin the object
                collectable.Rotate(spinAxis, spinSpeed * Time.deltaTime);

                // Optional bobbing motion
                if (enableBobbing)
                {
                    // Calculate bobbing offset but preserve the current XZ position
                    float bobbingOffset = Mathf.Sin(Time.time * bobbingSpeed * Mathf.PI * 2) * bobbingHeight;

                    // Only modify the Y position for bobbing, keep current X and Z
                    Vector3 newPosition = collectable.position;
                    newPosition.y = currentRestPosition.y + bobbingOffset;
                    collectable.position = newPosition;
                }
            }
        }

        private void OnDestroy()
        {
            // Clean up socket event listeners
            ItemSocketInteractor[] socketInteractors = FindObjectsOfType<ItemSocketInteractor>();
            foreach (var socketInteractor in socketInteractors)
            {
                socketInteractor.OnItemPickedUp -= OnItemPickedUp;
            }

            // Clean up direct interactor event listeners
            var grabInteractors = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor>();
            foreach (var grabInteractor in grabInteractors)
            {
                grabInteractor.selectEntered.RemoveListener(OnItemGrabbed);
                grabInteractor.selectExited.RemoveListener(OnItemReleased);
            }
        }
    }
}