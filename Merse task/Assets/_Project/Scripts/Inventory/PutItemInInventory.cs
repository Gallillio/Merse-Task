using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;

// [RequireComponent(typeof(UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor))]
public class PutItemInInventory : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor socketInteractor;

    // Store original local scales before reparenting
    private Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();

    private void Awake()
    {
        socketInteractor = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor>();
    }

    private void OnEnable()
    {
        socketInteractor.selectEntered.AddListener(OnSelectEntered);
        socketInteractor.selectExited.AddListener(OnSelectExited);
    }

    private void OnDisable()
    {
        socketInteractor.selectEntered.RemoveListener(OnSelectEntered);
        socketInteractor.selectExited.RemoveListener(OnSelectExited);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (args.interactableObject == null)
            return;

        Transform selected = args.interactableObject.transform;

        // Store original local scale before modifying
        if (!originalScales.ContainsKey(selected))
            originalScales[selected] = selected.localScale;

        // Store world scale before parenting
        Vector3 worldScale = selected.lossyScale;

        // Parent to the socket
        selected.SetParent(transform, true); // true = maintain world pos/rot

        selected.localScale *= 10f;

        // Compensate for canvas/small parent scaling
        // Vector3 parentScale = transform.lossyScale;
        // selected.localScale = new Vector3(
        //     worldScale.x / parentScale.x,
        //     worldScale.y / parentScale.y,
        //     worldScale.z / parentScale.z
        // );
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (args.interactableObject == null)
            return;

        Transform selected = args.interactableObject.transform;

        // Restore original scale if we stored it
        if (originalScales.TryGetValue(selected, out Vector3 originalScale))
        {
            selected.localScale = originalScale;
            originalScales.Remove(selected); // optional: clean up
        }
    }
}
