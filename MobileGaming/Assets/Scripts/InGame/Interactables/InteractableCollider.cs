using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractableCollider : MonoBehaviour
{
    [Header("Settings (autosets to this or parent if null)")]
    [SerializeReference] private Interactable interactable;

    private void Start()
    {
        if (interactable == null) interactable = GetComponent<Interactable>();
        if (interactable == null && transform.parent != null) interactable = transform.parent.GetComponent<Interactable>();
        if(interactable == null) Debug.LogWarning($"parent of {gameObject.name} is null",this);
    }

    private void OnTriggerEnter(Collider other)
    {
        interactable.EnterRange();
    }

    private void OnTriggerExit(Collider other)
    {
        interactable.ExitRange();
    }
}
