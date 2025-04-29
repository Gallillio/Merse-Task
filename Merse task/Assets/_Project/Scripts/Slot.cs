using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Slot : MonoBehaviour
{
    public GameObject ItemInSlot;
    public Image slotImage;
    public InputActionProperty releaseAction;
    Color originalColor;

    void Start()
    {
        slotImage = GetComponent<Image>();
        originalColor = slotImage.color;
    }

    void OnEnable()
    {
        if (releaseAction != null && releaseAction.action != null)
            releaseAction.action.Enable();
    }

    void OnDisable()
    {
        if (releaseAction != null && releaseAction.action != null)
            releaseAction.action.Disable();
    }

    private void OnTriggerStay(Collider other)
    {
        if (ItemInSlot != null) return;
        GameObject obj = other.gameObject;
        if (releaseAction != null && releaseAction.action != null && releaseAction.action.WasReleasedThisFrame())
        {
            InsertItem(obj);
        }
    }

    bool IsItem(GameObject obj)
    {
        return obj.GetComponent<Item>();
    }

    void InsertItem(GameObject obj)
    {
        obj.GetComponent<Rigidbody>().isKinematic = true;
        obj.transform.SetParent(gameObject.transform, true);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localEulerAngles = obj.GetComponent<Item>().slotRotation;
        obj.GetComponent<Item>().inSlot = true;
        obj.GetComponent<Item>().currentSlot = this;
        ItemInSlot = obj;
        slotImage.color = Color.green;
    }

    public void ResetColor()
    {
        slotImage.color = originalColor;
    }
}
