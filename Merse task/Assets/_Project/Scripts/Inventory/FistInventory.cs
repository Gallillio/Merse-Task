using UnityEngine;

public class FistInventory : MonoBehaviour
{
    private Canvas fistUICanvas;
    private bool isInRotationRange = false;
    private GameObject inventoryChild;

    void Start()
    {
        fistUICanvas = GetComponent<Canvas>();

        // Get the child inventory object if it exists
        if (transform.childCount > 0)
        {
            inventoryChild = transform.GetChild(0).gameObject;
        }
    }

    void Update()
    {
        CheckParentRotation();
    }

    private void CheckParentRotation()
    {
        if (inventoryChild == null) return;

        // Get parent's rotation, specifically Z axis
        float zRotation = transform.parent.rotation.eulerAngles.z;

        // Check if rotation is between 70 and 145 degrees
        bool currentlyInRange = (zRotation >= 70f && zRotation <= 145f);

        // Toggle inventory only when entering or leaving the rotation range
        if (currentlyInRange != isInRotationRange)
        {
            isInRotationRange = currentlyInRange;

            if (currentlyInRange)
            {
                // Enable inventory when entering the range
                inventoryChild.SetActive(true);
            }
            else
            {
                // Disable inventory when leaving the range
                inventoryChild.SetActive(false);
            }
        }
    }
}
