using UnityEngine;
using UnityEngine.InputSystem;

public class FistInventory : MonoBehaviour
{
    public InputActionAsset inputAction;

    private Canvas fistUICanvas;
    private InputAction menu;

    void Start()
    {
        fistUICanvas = GetComponent<Canvas>();
        menu = inputAction.FindActionMap("Controller").FindAction("Menu");
        menu.Enable();
        menu.performed += ToggleInventory;
    }

    private void OnDestroy()
    {
        menu.performed -= ToggleInventory;
    }

    public void ToggleInventory(InputAction.CallbackContext context)
    {
        if (transform.childCount > 0)
        {
            var child = transform.GetChild(0).gameObject;
            child.SetActive(!child.activeSelf);
        }
    }
}
