using UnityEngine;
using Core.Interfaces;
using Core.Services;

namespace Inventory
{
    /// <summary>
    /// Controls the visibility of the inventory UI based on hand/controller rotation
    /// Following SOLID principles with dependency injection
    /// </summary>
    public class FistInventoryController : MonoBehaviour
    {
        [Header("Configuration")]
        [Tooltip("Minimum rotation angle in degrees to show inventory")]
        [SerializeField] private float minRotationAngle = 70f;

        [Tooltip("Maximum rotation angle in degrees to show inventory")]
        [SerializeField] private float maxRotationAngle = 145f;

        [Header("References")]
        [Tooltip("The inventory UI GameObject to toggle (if null, will use first child)")]
        [SerializeField] private GameObject inventoryUI;

        private ILoggingService logger;
        private bool isInRotationRange = false;

        private void Start()
        {
            // Get the logging service
            logger = ServiceLocator.Get<ILoggingService>();

            // Find inventory UI if not set
            if (inventoryUI == null && transform.childCount > 0)
            {
                inventoryUI = transform.GetChild(0).gameObject;
                logger?.Log($"Auto-detected inventory UI: {inventoryUI.name}");
            }

            if (inventoryUI == null)
            {
                logger?.LogWarning("No inventory UI found for FistInventoryController");
            }
        }

        private void Update()
        {
            CheckRotationAndUpdateUI();
        }

        /// <summary>
        /// Check parent rotation and update UI visibility accordingly
        /// </summary>
        private void CheckRotationAndUpdateUI()
        {
            if (inventoryUI == null || transform.parent == null)
                return;

            // Get parent's Z rotation
            float zRotation = transform.parent.rotation.eulerAngles.z;

            // Check if rotation is within the configured range
            bool currentlyInRange = (zRotation >= minRotationAngle && zRotation <= maxRotationAngle);

            // Only toggle when state changes
            if (currentlyInRange != isInRotationRange)
            {
                isInRotationRange = currentlyInRange;
                inventoryUI.SetActive(currentlyInRange);

                logger?.Log($"Inventory UI {(currentlyInRange ? "shown" : "hidden")} at rotation {zRotation:F1}°");
            }
        }
    }
}