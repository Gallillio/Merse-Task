using UnityEngine;
using TMPro;
using Core.Interfaces;
using Core.Services;

/// <summary>
/// Manages UI elements for NPC interactions, including spatial panel and listening icon.
/// </summary>
public class NPCInstructionUI : MonoBehaviour
{
    [SerializeField] private GameObject spatialPanelModel;
    [SerializeField] private GameObject listeningIcon;

    [HideInInspector] public TMP_Text responseText;

    private ILoggingService loggingService;

    private void Awake()
    {
        loggingService = ServiceLocator.Get<ILoggingService>();

        // Auto-find the spatial panel if not assigned
        if (spatialPanelModel == null)
        {
            spatialPanelModel = transform.Find("_Spatial Panel Manipulator Model")?.gameObject;
            if (spatialPanelModel == null)
            {
                loggingService.LogWarning("Could not find '_Spatial Panel Manipulator Model' child GameObject");
            }
        }

        // Auto-find the listening icon if not assigned
        if (listeningIcon == null)
        {
            listeningIcon = transform.parent.Find("_Actively Listening Feedback Icon")?.gameObject;
            if (listeningIcon == null)
            {
                loggingService.LogWarning("Could not find '_Actively Listening Feedback Icon' sibling GameObject");
            }
        }

        // Auto-find the response text
        responseText = GetComponentInChildren<TMP_Text>();
        if (responseText == null)
        {
            loggingService.LogWarning("Could not find TMP_Text component in children");
        }

        // Hide UI elements initially
        HideSpatialPanel();
        HideListeningIcon();
    }

    public void ShowSpatialPanel()
    {
        if (spatialPanelModel != null)
        {
            spatialPanelModel.SetActive(true);
        }
    }

    public void HideSpatialPanel()
    {
        if (spatialPanelModel != null)
        {
            spatialPanelModel.SetActive(false);
        }
    }

    public void ShowListeningIcon()
    {
        if (listeningIcon != null)
        {
            listeningIcon.SetActive(true);
        }
    }

    public void HideListeningIcon()
    {
        if (listeningIcon != null)
        {
            listeningIcon.SetActive(false);
        }
    }
}