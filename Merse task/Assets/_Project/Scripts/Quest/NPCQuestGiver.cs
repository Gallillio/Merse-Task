using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class NPCQuestGiver : MonoBehaviour
{
    [System.Serializable]
    public class QuestRequirement
    {
        public string requiredItemID;
        public string questID; // Unique ID for this quest
        public string questDescription;
        public bool isCompleted = false;
    }

    [SerializeField] private QuestRequirement[] questRequirements;
    [SerializeField] private string npcName;
    [SerializeField] private string incompleteDialogue = "I need a specific item. Bring it to me.";
    [SerializeField] private string completeDialogue = "Thank you for bringing me what I needed!";
    [SerializeField] private bool checkQuestManagerForCompletion = true;

    private bool hasCheckedInventory = false;

    private void Start()
    {
        // Check if quests are already completed in QuestManager
        if (checkQuestManagerForCompletion)
        {
            foreach (var quest in questRequirements)
            {
                quest.isCompleted = QuestManager.Instance.IsQuestCompleted(quest.questID);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CheckPlayerInventory(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            hasCheckedInventory = false;
        }
    }

    private void CheckPlayerInventory(GameObject player)
    {
        if (hasCheckedInventory) return;
        hasCheckedInventory = true;

        // Find all sockets on the player (assumes sockets are direct children with PutItemInInventory component)
        var inventorySockets = player.GetComponentsInChildren<PutItemInInventory>();

        foreach (var requirement in questRequirements)
        {
            if (requirement.isCompleted)
            {
                Debug.Log($"Quest '{requirement.questID}' already completed for NPC: {npcName}");
                continue;
            }

            bool hasRequiredItem = false;

            // Check each socket for the required item
            foreach (var socket in inventorySockets)
            {
                // Get socket's transform to check its children
                Transform socketTransform = socket.transform;

                // Look through all children of the socket
                foreach (Transform child in socketTransform)
                {
                    // Check if child has QuestItem component and if it matches our required ID
                    QuestItem questItem = child.GetComponent<QuestItem>();
                    if (questItem != null && questItem.ItemID == requirement.requiredItemID)
                    {
                        hasRequiredItem = true;
                        requirement.isCompleted = true;

                        // Record quest completion in QuestManager
                        QuestManager.Instance.CompleteQuest(requirement.questID);

                        // Quest completed logic
                        QuestCompleted(requirement);
                        break;
                    }
                }

                if (hasRequiredItem) break;
            }

            if (!hasRequiredItem)
            {
                // Quest incomplete logic
                QuestIncomplete(requirement);
            }
        }
    }

    private void QuestCompleted(QuestRequirement quest)
    {
        Debug.Log($"[{npcName}] Quest '{quest.questID}' completed! Player has brought the required item: {quest.requiredItemID}");
        Debug.Log($"[{npcName}] says: {completeDialogue}");

        // Here you would typically trigger animations, dialogue, rewards, etc.
    }

    private void QuestIncomplete(QuestRequirement quest)
    {
        Debug.Log($"[{npcName}] Quest '{quest.questID}' incomplete. Player doesn't have the required item: {quest.requiredItemID}");
        Debug.Log($"[{npcName}] says: {incompleteDialogue}");
    }

    // For debugging/testing
    public bool AreAllQuestsCompleted()
    {
        return questRequirements.All(q => q.isCompleted);
    }
}