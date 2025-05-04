using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

public class InputTester : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private Transform buttonIndicatorsParent;
    [SerializeField] private GameObject buttonIndicatorPrefab;

    // Dictionary to store action names and their UI indicators
    private Dictionary<string, Image> actionIndicators = new Dictionary<string, Image>();

    void Start()
    {
        if (inputActions == null)
        {
            Debug.LogError("Input Actions asset not assigned!");
            return;
        }

        // Create UI indicators for Controller actions
        CreateIndicatorsForActionMap("Controller");

        // Enable all action maps
        foreach (var actionMap in inputActions.actionMaps)
        {
            actionMap.Enable();
        }
    }

    private void CreateIndicatorsForActionMap(string actionMapName)
    {
        var actionMap = inputActions.FindActionMap(actionMapName);
        if (actionMap == null) return;

        // Create indicators for each action
        foreach (var action in actionMap.actions)
        {
            if (buttonIndicatorsParent != null && buttonIndicatorPrefab != null)
            {
                // Create indicator UI
                GameObject indicator = Instantiate(buttonIndicatorPrefab, buttonIndicatorsParent);

                // Set label text
                Text labelText = indicator.GetComponentInChildren<Text>();
                if (labelText != null)
                {
                    labelText.text = action.name;
                }

                // Get the indicator image
                Image indicatorImage = indicator.GetComponentInChildren<Image>();
                if (indicatorImage != null)
                {
                    // Store for later use
                    actionIndicators[action.name] = indicatorImage;

                    // Default color - not pressed
                    indicatorImage.color = Color.red;
                }

                // Subscribe to action events
                action.performed += ctx => OnActionPerformed(action.name);
                action.canceled += ctx => OnActionCanceled(action.name);
            }
            else
            {
                // Just subscribe to the actions for console logging if UI is not available
                action.performed += ctx => Debug.Log($"Action Performed: {action.name}");
                action.canceled += ctx => Debug.Log($"Action Canceled: {action.name}");
            }
        }
    }

    private void OnActionPerformed(string actionName)
    {
        Debug.Log($"Button pressed: {actionName}");

        // Update UI indicator if it exists
        if (actionIndicators.TryGetValue(actionName, out Image indicator))
        {
            indicator.color = Color.green;
        }
    }

    private void OnActionCanceled(string actionName)
    {
        Debug.Log($"Button released: {actionName}");

        // Update UI indicator if it exists
        if (actionIndicators.TryGetValue(actionName, out Image indicator))
        {
            indicator.color = Color.red;
        }
    }

    private void OnDestroy()
    {
        // Clean up by disabling all action maps
        foreach (var actionMap in inputActions.actionMaps)
        {
            actionMap.Disable();
        }
    }

    // Call this to create a prefab button indicator at runtime if needed
    public GameObject CreateButtonIndicatorPrefab()
    {
        if (buttonIndicatorPrefab != null) return buttonIndicatorPrefab;

        GameObject prefab = new GameObject("ButtonIndicator");

        // Create layout
        RectTransform rectTransform = prefab.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 50);

        // Background image
        Image bgImage = prefab.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // Create label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(prefab.transform);

        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0);
        labelRect.anchorMax = new Vector2(0.7f, 1);
        labelRect.offsetMin = new Vector2(10, 0);
        labelRect.offsetMax = new Vector2(-10, 0);

        Text labelText = labelObj.AddComponent<Text>();
        labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        labelText.fontSize = 14;
        labelText.alignment = TextAnchor.MiddleLeft;
        labelText.color = Color.white;

        // Create indicator
        GameObject indicatorObj = new GameObject("Indicator");
        indicatorObj.transform.SetParent(prefab.transform);

        RectTransform indicatorRect = indicatorObj.AddComponent<RectTransform>();
        indicatorRect.anchorMin = new Vector2(0.7f, 0.2f);
        indicatorRect.anchorMax = new Vector2(0.95f, 0.8f);
        indicatorRect.offsetMin = Vector2.zero;
        indicatorRect.offsetMax = Vector2.zero;

        Image indicatorImage = indicatorObj.AddComponent<Image>();
        indicatorImage.color = Color.red;

        return prefab;
    }
}