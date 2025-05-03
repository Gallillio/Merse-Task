using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    [Header("Player Inventory")]
    [Tooltip("References to XR Sockets that function as player inventory")]
    public List<Transform> playerInventorySockets = new List<Transform>();

    // Singleton instance
    private static QuestManager _instance;
    public static QuestManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<QuestManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("_Quest Manager");
                    _instance = obj.AddComponent<QuestManager>();
                    Debug.Log("Created new QuestManager instance");
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        // Singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(this.gameObject);

        // Auto-find inventory sockets if not assigned
        if (playerInventorySockets.Count == 0)
        {
            Debug.LogWarning("No inventory sockets assigned to QuestManager. Searching for XR socket tags...");
            // You would add auto-detection logic here if needed
        }
    }

    // Check if player has an item with the specified name in any inventory socket
    public bool HasItem(string itemName)
    {
        if (string.IsNullOrEmpty(itemName))
        {
            Debug.LogWarning("Trying to check for item with empty name");
            return false;
        }

        foreach (Transform socket in playerInventorySockets)
        {
            if (socket == null) continue;

            // Check all children of this socket
            foreach (Transform child in socket)
            {
                // Compare by name (case insensitive)
                if (child.name.ToLower().Contains(itemName.ToLower()))
                {
                    Debug.Log($"Found quest item '{itemName}' in player inventory: {child.name}");
                    return true;
                }
            }
        }

        Debug.Log($"Quest item '{itemName}' not found in player inventory");
        return false;
    }

    // Utility method to register an inventory socket
    public void RegisterInventorySocket(Transform socketTransform)
    {
        if (socketTransform != null && !playerInventorySockets.Contains(socketTransform))
        {
            playerInventorySockets.Add(socketTransform);
            Debug.Log($"Registered inventory socket: {socketTransform.name}");
        }
    }
}