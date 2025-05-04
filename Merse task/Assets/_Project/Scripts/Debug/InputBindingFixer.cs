using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class InputBindingFixer : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    [Header("Binding Fixes")]
    [SerializeField] private bool addXRBindingsAtRuntime = true;
    [SerializeField] private bool fixAllButtonBindings = true;

    [Header("Controller Binding Paths")]
    [SerializeField] private string primaryButtonPath = "<XRController>{RightHand}/primaryButton";
    [SerializeField] private string secondaryButtonPath = "<XRController>{RightHand}/secondaryButton";
    [SerializeField] private string triggerPath = "<XRController>{RightHand}/trigger";
    [SerializeField] private string triggerPressedPath = "<XRController>{RightHand}/triggerPressed";
    [SerializeField] private string gripPath = "<XRController>{RightHand}/grip";
    [SerializeField] private string gripPressedPath = "<XRController>{RightHand}/gripPressed";
    [SerializeField] private string menuButtonPath = "<XRController>{RightHand}/menu";
    [SerializeField] private string primary2DAxisPath = "<XRController>{RightHand}/primary2DAxis";
    [SerializeField] private string primary2DAxisClickPath = "<XRController>{RightHand}/primary2DAxisClick";
    [SerializeField] private string primary2DAxisTouchPath = "<XRController>{RightHand}/primary2DAxisTouch";
    [SerializeField] private string secondary2DAxisPath = "<XRController>{RightHand}/secondary2DAxis";
    [SerializeField] private string secondary2DAxisClickPath = "<XRController>{RightHand}/secondary2DAxisClick";
    [SerializeField] private string secondary2DAxisTouchPath = "<XRController>{RightHand}/secondary2DAxisTouch";
    [SerializeField] private string primaryTouchPath = "<XRController>{RightHand}/primaryTouch";
    [SerializeField] private string secondaryTouchPath = "<XRController>{RightHand}/secondaryTouch";

    private Dictionary<string, string> actionBindingMap = new Dictionary<string, string>();

    void Awake()
    {
        if (inputActions == null)
        {
            Debug.LogError("Input Actions asset not assigned!");
            return;
        }

        // Initialize binding map
        InitializeBindingMap();

        if (addXRBindingsAtRuntime)
        {
            AddMissingBindings();
        }
    }

    private void InitializeBindingMap()
    {
        actionBindingMap.Clear();

        // Add mapping for all known controller actions
        actionBindingMap.Add("Primary Button", primaryButtonPath);
        actionBindingMap.Add("Secondary Button", secondaryButtonPath);
        actionBindingMap.Add("Trigger", triggerPressedPath);
        actionBindingMap.Add("Grip", gripPressedPath);
        actionBindingMap.Add("Menu", menuButtonPath);
        actionBindingMap.Add("Primary 2D Axis Click", primary2DAxisClickPath);
        actionBindingMap.Add("Secondary 2D Axis Click", secondary2DAxisClickPath);
        actionBindingMap.Add("Primary 2D Axis Touch", primary2DAxisTouchPath);
        actionBindingMap.Add("Secondary 2D Axis Touch", secondary2DAxisTouchPath);
        actionBindingMap.Add("Primary Touch", primaryTouchPath);
        actionBindingMap.Add("Secondary Touch", secondaryTouchPath);

        // Add continuous value actions
        actionBindingMap.Add("Axis 2D", primary2DAxisPath);
        actionBindingMap.Add("Resting Hand Axis 2D", primary2DAxisPath);
    }

    private void AddMissingBindings()
    {
        var controllerMap = inputActions.FindActionMap("Controller");
        if (controllerMap != null)
        {
            Debug.Log("Found Controller action map, fixing bindings...");

            if (fixAllButtonBindings)
            {
                // Fix all action bindings in the Controller map
                foreach (var action in controllerMap.actions)
                {
                    if (actionBindingMap.TryGetValue(action.name, out string bindingPath))
                    {
                        AddBindingIfMissing(controllerMap, action.name, bindingPath);
                    }
                    else
                    {
                        Debug.LogWarning($"No binding path defined for action: {action.name}");
                    }
                }
            }
            else
            {
                // Just fix specific buttons based on serialized fields
                AddBindingIfMissing(controllerMap, "Secondary Button", secondaryButtonPath);
                AddBindingIfMissing(controllerMap, "Primary Button", primaryButtonPath);
            }
        }
        else
        {
            Debug.LogError("Could not find Controller action map!");
        }
    }

    private void AddBindingIfMissing(InputActionMap actionMap, string actionName, string bindingPath)
    {
        var action = actionMap.FindAction(actionName);
        if (action != null)
        {
            bool hasBinding = false;

            // Check if binding already exists
            foreach (var binding in action.bindings)
            {
                if (binding.path == bindingPath)
                {
                    hasBinding = true;
                    Debug.Log($"Binding {bindingPath} already exists for {actionName}");
                    break;
                }
            }

            // Add binding if it doesn't exist
            if (!hasBinding)
            {
                action.AddBinding(bindingPath, groups: "Generic XR Controller");
                Debug.Log($"Added binding {bindingPath} to {actionName}");
            }
        }
        else
        {
            Debug.LogWarning($"Could not find {actionName} action in {actionMap.name}");
        }
    }

    // For manual triggering from Inspector/UI button
    public void FixBindingsNow()
    {
        AddMissingBindings();
    }

    // For debugging in the editor
    public void LogAllAvailableActions()
    {
        if (inputActions == null) return;

        Debug.Log("=== All Available Actions ===");
        foreach (var actionMap in inputActions.actionMaps)
        {
            Debug.Log($"Action Map: {actionMap.name}");
            foreach (var action in actionMap.actions)
            {
                Debug.Log($"  - Action: {action.name} (Type: {action.type})");
            }
        }
    }
}