using UnityEngine;

// Attach this script to XR Sockets that can hold quest items
public class QuestSocket : MonoBehaviour
{
    [Tooltip("If true, this socket will automatically register with the QuestManager")]
    public bool autoRegister = true;

    void Start()
    {
        if (autoRegister)
        {
            RegisterWithQuestManager();
        }
    }

    public void RegisterWithQuestManager()
    {
        // This will create the QuestManager if it doesn't exist
        QuestManager.Instance.RegisterInventorySocket(transform);
    }
} 