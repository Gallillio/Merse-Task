using UnityEngine;
using UnityEngine.InputSystem;

public class InputDebugger : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    private void Awake()
    {
        if (inputActions == null)
        {
            Debug.LogError("Input Actions asset not assigned!");
            return;
        }
    }

    private void OnEnable()
    {
        if (inputActions == null) return;

        // Get the Controller action map
        var controllerMap = inputActions.FindActionMap("Controller");

        if (controllerMap != null)
        {
            // Enable the action map
            controllerMap.Enable();

            // Find and subscribe to button actions
            SubscribeToAction(controllerMap, "Trigger");
            SubscribeToAction(controllerMap, "Primary Button");
            SubscribeToAction(controllerMap, "Secondary Button");
            SubscribeToAction(controllerMap, "Menu");
            SubscribeToAction(controllerMap, "Primary 2D Axis Click");
            SubscribeToAction(controllerMap, "Secondary 2D Axis Click");
            SubscribeToAction(controllerMap, "Primary 2D Axis Touch");
            SubscribeToAction(controllerMap, "Secondary 2D Axis Touch");
            SubscribeToAction(controllerMap, "Primary Touch");
            SubscribeToAction(controllerMap, "Secondary Touch");

            Debug.Log("Input Debugger initialized - watching Controller action map");
        }
        else
        {
            Debug.LogError("Could not find Controller action map!");
        }

        // Also check XRI action maps if available
        TrySubscribeToXRIActions("XRI LeftHand Interaction");
        TrySubscribeToXRIActions("XRI RightHand Interaction");
    }

    private void TrySubscribeToXRIActions(string actionMapName)
    {
        var actionMap = inputActions.FindActionMap(actionMapName);
        if (actionMap != null)
        {
            actionMap.Enable();

            // Common XR actions
            SubscribeToAction(actionMap, "Select");
            SubscribeToAction(actionMap, "Activate");
            SubscribeToAction(actionMap, "UI Press");

            Debug.Log($"Watching {actionMapName} actions");
        }
    }

    private void SubscribeToAction(InputActionMap actionMap, string actionName)
    {
        InputAction action = actionMap.FindAction(actionName);

        if (action != null)
        {
            action.performed += ctx => OnActionPerformed(actionName, ctx);
            action.started += ctx => OnActionStarted(actionName, ctx);
            action.canceled += ctx => OnActionCanceled(actionName, ctx);
            Debug.Log($"Subscribed to {actionName} action");
        }
        else
        {
            Debug.LogWarning($"Could not find {actionName} action in {actionMap.name}");
        }
    }

    private void OnActionPerformed(string actionName, InputAction.CallbackContext context)
    {
        // For button actions, this will be called when the button is pressed
        if (context.control != null)
        {
            Debug.Log($"<color=green>PERFORMED</color>: {actionName} with {context.control.name} - Value: {context.ReadValueAsObject()}");
        }
        else
        {
            Debug.Log($"<color=green>PERFORMED</color>: {actionName} - Value: {context.ReadValueAsObject()}");
        }
    }

    private void OnActionStarted(string actionName, InputAction.CallbackContext context)
    {
        if (context.control != null)
        {
            Debug.Log($"<color=yellow>STARTED</color>: {actionName} with {context.control.name}");
        }
        else
        {
            Debug.Log($"<color=yellow>STARTED</color>: {actionName}");
        }
    }

    private void OnActionCanceled(string actionName, InputAction.CallbackContext context)
    {
        if (context.control != null)
        {
            Debug.Log($"<color=red>CANCELED</color>: {actionName} with {context.control.name}");
        }
        else
        {
            Debug.Log($"<color=red>CANCELED</color>: {actionName}");
        }
    }

    private void OnDisable()
    {
        if (inputActions == null) return;

        // Disable and unsubscribe from all action maps
        foreach (var actionMap in inputActions.actionMaps)
        {
            actionMap.Disable();
        }
    }
}