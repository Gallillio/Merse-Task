using UnityEngine;
using TMPro;

public class NPCInstruction : MonoBehaviour
{
    [TextArea(2, 5)]
    public string npcInstruction; // Set per-NPC instructions in Inspector

    [Header("UI References")]
    [Tooltip("Will be automatically assigned if not set")]
    [HideInInspector] public TMP_Text responseText; // Each NPC has its own response text component

    private void Awake()
    {
        // Auto-find the response text if not assigned
        if (responseText == null)
        {
            Debug.Log($"[{gameObject.name}] NPCInstruction.Awake - Looking for responseText component");

            // Try to find a child GameObject named "GPT Response Text"
            Transform responseTextObject = transform.Find("GPT Response Text");

            if (responseTextObject != null)
            {
                // Get the TMP_Text component
                responseText = responseTextObject.GetComponent<TMP_Text>();
                Debug.Log($"[{gameObject.name}] Found responseText in 'GPT Response Text' child");
            }
            else
            {
                // Look specifically in the _Spatial Panel Manipulator Model if it exists
                Transform spatialPanel = transform.Find("_Spatial Panel Manipulator Model");

                if (spatialPanel != null)
                {
                    Debug.Log($"[{gameObject.name}] Found _Spatial Panel Manipulator Model, searching in it");
                    responseText = spatialPanel.GetComponentInChildren<TMP_Text>(true); // true to include inactive GameObjects

                    if (responseText != null)
                    {
                        Debug.Log($"[{gameObject.name}] Found responseText inside Spatial Panel: {responseText.gameObject.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"[{gameObject.name}] No TMP_Text component found in Spatial Panel");
                    }
                }
                else
                {
                    Debug.Log($"[{gameObject.name}] No 'GPT Response Text' or Spatial Panel found, looking for TMP_Text in all children");

                    // Try to find the TMP_Text component on any child
                    responseText = GetComponentInChildren<TMP_Text>(true); // true to include inactive GameObjects

                    if (responseText != null)
                    {
                        Debug.Log($"[{gameObject.name}] Found responseText on child: {responseText.gameObject.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"[{gameObject.name}] Failed to find any TMP_Text component in children");
                    }
                }
            }
        }
        else
        {
            Debug.Log($"[{gameObject.name}] responseText already assigned in inspector");
        }
    }
}