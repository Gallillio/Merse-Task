using UnityEngine;

public class QuestItem : MonoBehaviour
{
    [SerializeField] private string itemID;
    [SerializeField] private string itemName;
    [SerializeField] private string itemDescription;

    public string ItemID => itemID;
    public string ItemName => itemName;
    public string ItemDescription => itemDescription;
}