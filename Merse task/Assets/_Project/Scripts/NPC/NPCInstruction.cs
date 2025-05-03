using UnityEngine;
using TMPro;

public class NPCInstruction : MonoBehaviour
{
    [Header("NPC Dialogue Settings")]
    [TextArea(2, 5)]
    public string npcInstruction; // Initial instruction prompt

    [Header("Quest Settings")]
    public bool hasQuest = false;
    public string questItemName; // Name of the item NPC is looking for

    [Header("Quest Prompts")]
    [TextArea(2, 5)]
    public string questInProgressPrompt; // Used when player returns without the item
    [TextArea(2, 5)]
    public string completedQuestPrompt; // Used when player returns with the item

    [Header("Quest State")]
    [HideInInspector]
    public bool questActive = false; // Becomes true after first interaction
    [HideInInspector]
    public bool questCompleted = false; // Becomes true after completing the quest

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
                // Look specifically in the _Spatial Panel Manipulator Model if it exists
                Transform spatialPanel = transform.Find("_Spatial Panel Manipulator Model");

                if (spatialPanel != null)
                {
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

    // Get the appropriate instruction based on quest state
    public string GetCurrentInstruction()
    {
        if (!hasQuest)
        {
            // NPC doesn't have a quest, use default instruction
            return npcInstruction;
        }
        else if (!questActive)
        {
            // First interaction - use initial instruction
            // Debug.Log($"[QUEST INSTRUCTION] Using initial instruction for first interaction with {gameObject.name}");
            return npcInstruction;
        }
        else if (questActive && !questCompleted)
        {
            // Quest is active but not completed
            // Debug.Log($"[QUEST INSTRUCTION] Using in-progress prompt for {gameObject.name}");
            return string.IsNullOrEmpty(questInProgressPrompt) ? npcInstruction : questInProgressPrompt;
        }
        else if (questCompleted)
        {
            // Quest is completed
            // Debug.Log($"[QUEST INSTRUCTION] Using completion prompt for {gameObject.name}");
            return string.IsNullOrEmpty(completedQuestPrompt) ? npcInstruction : completedQuestPrompt;
        }

        // Default fallback
        return npcInstruction;
    }
}