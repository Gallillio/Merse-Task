using UnityEngine;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
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
                    GameObject go = new GameObject("QuestManager");
                    _instance = go.AddComponent<QuestManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    // Dictionary to store quest completion states by quest ID
    private Dictionary<string, bool> questStatus = new Dictionary<string, bool>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Mark a quest as completed
    public void CompleteQuest(string questID)
    {
        questStatus[questID] = true;
        Debug.Log($"Quest '{questID}' completed and recorded in quest manager");
    }

    // Check if a quest is completed
    public bool IsQuestCompleted(string questID)
    {
        return questStatus.ContainsKey(questID) && questStatus[questID];
    }

    // Reset a quest status (for testing or specific quest logic)
    public void ResetQuest(string questID)
    {
        if (questStatus.ContainsKey(questID))
        {
            questStatus[questID] = false;
        }
    }
}