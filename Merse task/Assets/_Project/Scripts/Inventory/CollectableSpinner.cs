using UnityEngine;
using System.Collections;
using Core.Interfaces;
using Core.Services;

namespace Inventory
{
    /// <summary>
    /// Makes collectible items spin continuously and stops spinning when collected
    /// </summary>
    public class CollectableSpinner : MonoBehaviour
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

        [Header("Item Settings")]
        [Tooltip("Should the spinner disable automatically when picked up?")]
        [SerializeField] private bool disableOnPickup = true;

        private Vector3 startPosition;
        private bool isSpinning = true;
        private ILoggingService logger;

        private void Awake()
        {
            logger = ServiceLocator.Get<ILoggingService>();
            startPosition = transform.position;
        }

        private void Start()
        {
            // Find any socket interactor or item component to subscribe to its events
            SetupPickupListeners();
        }

        private void SetupPickupListeners()
        {
            // Check for ItemSocketInteractor component on parent or in children
            var socketInteractor = GetComponentInParent<ItemSocketInteractor>();
            if (socketInteractor == null)
            {
                socketInteractor = GetComponentInChildren<ItemSocketInteractor>();
            }

            if (socketInteractor != null && disableOnPickup)
            {
                socketInteractor.OnItemPickedUp += OnItemPickedUp;
                logger?.Log($"CollectableSpinner on {gameObject.name} registered with ItemSocketInteractor");
            }

            // Could add more event subscriptions for other collection mechanisms here
        }

        private void OnItemPickedUp(GameObject item)
        {
            if (item == gameObject || item.transform.IsChildOf(transform) || transform.IsChildOf(item.transform))
            {
                StopSpinning();
                logger?.Log($"Item {gameObject.name} picked up, stopped spinning");
            }
        }

        private void Update()
        {
            if (isSpinning)
            {
                // Spin the object
                transform.Rotate(spinAxis, spinSpeed * Time.deltaTime);

                // Optional bobbing motion
                if (enableBobbing)
                {
                    float bobbingOffset = Mathf.Sin(Time.time * bobbingSpeed * Mathf.PI * 2) * bobbingHeight;
                    transform.position = startPosition + Vector3.up * bobbingOffset;
                }
            }
        }

        /// <summary>
        /// Stop the spinning animation
        /// </summary>
        public void StopSpinning()
        {
            isSpinning = false;
            transform.position = startPosition; // Reset position to avoid stopping mid-bob
        }

        /// <summary>
        /// Resume the spinning animation
        /// </summary>
        public void StartSpinning()
        {
            isSpinning = true;
        }

        private void OnDestroy()
        {
            // Clean up event listeners
            var socketInteractor = GetComponentInParent<ItemSocketInteractor>();
            if (socketInteractor != null)
            {
                socketInteractor.OnItemPickedUp -= OnItemPickedUp;
            }
        }
    }
}