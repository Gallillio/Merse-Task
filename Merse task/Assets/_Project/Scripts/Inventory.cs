// Script name: Inventory
// Script purpose: attaching a gameobject to a certain anchor and having the ability to enable and disable it.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Inventory : MonoBehaviour
{
    public GameObject InventoryUI;
    public GameObject Anchor;
    public InputActionProperty menuAction; // Assign the Menu action from the input actions asset in the Inspector
    bool UIActive;

    private void Start()
    {
        InventoryUI.SetActive(false);
        UIActive = false;
    }

    private void OnEnable()
    {
        if (menuAction != null && menuAction.action != null)
            menuAction.action.Enable();
    }

    private void OnDisable()
    {
        if (menuAction != null && menuAction.action != null)
            menuAction.action.Disable();
    }

    private void Update()
    {
        if (menuAction != null && menuAction.action != null && menuAction.action.WasPressedThisFrame())
        {
            UIActive = !UIActive;
            InventoryUI.SetActive(UIActive);
        }
        if (UIActive)
        {
            InventoryUI.transform.position = Anchor.transform.position;
            InventoryUI.transform.eulerAngles = new Vector3(Anchor.transform.eulerAngles.x + 15, Anchor.transform.eulerAngles.y, 0);
        }
    }
}
