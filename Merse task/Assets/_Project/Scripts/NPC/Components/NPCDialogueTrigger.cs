using UnityEngine;
using Core.Interfaces;
using Core.Services;

/// <summary>
/// Handles trigger events for NPC dialogue interactions.
/// Detects when player enters or exits the NPC's interaction zone.
/// </summary>
public class NPCDialogueTrigger : MonoBehaviour
{
    private NPCInteractionController controller;
    private ILoggingService logger;

    private void Awake()
    {
        // Get logging service
        logger = ServiceLocator.Get<ILoggingService>();
        
        // Get the parent NPCInteractionController
        controller = GetComponentInParent<NPCInteractionController>();
        
        if (controller == null)
        {
            logger?.LogError("NPCDialogueTrigger requires an NPCInteractionController component in parent hierarchy!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && controller != null)
        {
            controller.OnPlayerEntered();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && controller != null)
        {
            controller.OnPlayerExited();
        }
    }
}