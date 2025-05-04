using UnityEngine;
using UnityEngine.InputSystem;
using System.Text;

public class ButtonComparisonDebugger : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private bool showDetailedControlInfo = true;

    private InputAction primaryButtonAction;
    private InputAction secondaryButtonAction;

    private StringBuilder logBuilder = new StringBuilder();

    void Start()
    {
        if (inputActions == null)
        {
            Debug.LogError("Input Actions asset not assigned!");
            return;
        }

        // Find the Controller action map and the button actions
        var controllerMap = inputActions.FindActionMap("Controller");
        if (controllerMap != null)
        {
            primaryButtonAction = controllerMap.FindAction("Primary Button");
            secondaryButtonAction = controllerMap.FindAction("Secondary Button");

            if (primaryButtonAction != null && secondaryButtonAction != null)
            {
                Debug.Log("Successfully found both button actions!");

                // Enable the actions
                primaryButtonAction.Enable();
                secondaryButtonAction.Enable();

                // Subscribe to events
                primaryButtonAction.started += ctx => OnButtonEvent("Primary Button", "started", ctx);
                primaryButtonAction.performed += ctx => OnButtonEvent("Primary Button", "performed", ctx);
                primaryButtonAction.canceled += ctx => OnButtonEvent("Primary Button", "canceled", ctx);

                secondaryButtonAction.started += ctx => OnButtonEvent("Secondary Button", "started", ctx);
                secondaryButtonAction.performed += ctx => OnButtonEvent("Secondary Button", "performed", ctx);
                secondaryButtonAction.canceled += ctx => OnButtonEvent("Secondary Button", "canceled", ctx);

                // Log all available controls and bindings
                if (showDetailedControlInfo)
                {
                    LogActionDetails("Primary Button", primaryButtonAction);
                    LogActionDetails("Secondary Button", secondaryButtonAction);
                }
            }
            else
            {
                Debug.LogError("Could not find Primary or Secondary Button actions!");
            }
        }
        else
        {
            Debug.LogError("Could not find Controller action map!");
        }

        // Also check for XRI action maps
        CheckXRIBindings();
    }

    private void CheckXRIBindings()
    {
        Debug.Log("Checking XRI bindings...");

        string[] actionMapNames = new[] {
            "XRI LeftHand Interaction",
            "XRI RightHand Interaction"
        };

        foreach (string mapName in actionMapNames)
        {
            var actionMap = inputActions.FindActionMap(mapName);
            if (actionMap != null)
            {
                Debug.Log($"Found action map: {mapName}");

                // List all actions in this map
                foreach (var action in actionMap.actions)
                {
                    logBuilder.Clear();
                    logBuilder.AppendLine($"Action: {action.name}");

                    // List bindings
                    foreach (var binding in action.bindings)
                    {
                        logBuilder.AppendLine($"  - Path: {binding.path}");
                    }

                    Debug.Log(logBuilder.ToString());
                }
            }
        }
    }

    private void LogActionDetails(string actionName, InputAction action)
    {
        logBuilder.Clear();
        logBuilder.AppendLine($"---- {actionName} Details ----");
        logBuilder.AppendLine($"Type: {action.type}");
        logBuilder.AppendLine($"Controls ({action.controls.Count}):");

        foreach (var control in action.controls)
        {
            logBuilder.AppendLine($"  - {control.name} (Device: {control.device.name})");
        }

        logBuilder.AppendLine($"Bindings ({action.bindings.Count}):");
        foreach (var binding in action.bindings)
        {
            logBuilder.AppendLine($"  - Path: {binding.path}, Groups: {binding.groups}");
        }

        Debug.Log(logBuilder.ToString());
    }

    private void OnButtonEvent(string buttonName, string eventType, InputAction.CallbackContext context)
    {
        string controlInfo = context.control != null ? $" - Control: {context.control.name}" : "";
        string deviceInfo = context.control != null ? $" - Device: {context.control.device.name}" : "";
        Debug.Log($"{buttonName} {eventType}{controlInfo}{deviceInfo}");
    }

    private void OnDestroy()
    {
        // Unsubscribe and disable actions
        if (primaryButtonAction != null)
        {
            primaryButtonAction.Disable();
        }

        if (secondaryButtonAction != null)
        {
            secondaryButtonAction.Disable();
        }
    }
}