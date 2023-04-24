using UnityEngine;

[RequireComponent(typeof(Collider))]
public class InteractableCollider : MonoBehaviour, ILinkable
{
    [field:SerializeReference] public Interactable interactable { get; private set; }

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

    public void OnLinkStarted()
    {
        throw new System.NotImplementedException();
    }

    public void OnLinkConnected()
    {
        throw new System.NotImplementedException();
    }
}
