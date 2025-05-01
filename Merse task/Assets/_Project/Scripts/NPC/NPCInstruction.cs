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
            // Try to find a child GameObject named "GPT Response Text"
            Transform responseTextObject = transform.Find("GPT Response Text");

            if (responseTextObject != null)
            {
                // Get the TMP_Text component
                responseText = responseTextObject.GetComponent<TMP_Text>();
            }
            else
            {
                // Try to find the TMP_Text component on any child
                responseText = GetComponentInChildren<TMP_Text>();
            }
        }
    }
}