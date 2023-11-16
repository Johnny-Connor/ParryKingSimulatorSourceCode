using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class MainMenuSelectable : MonoBehaviour, IPointerEnterHandler
{
    // Variables.
    [Tooltip("The selector that appears on top of this selectable when selected.")]
    [SerializeField] private GameObject _intendedSelector;
    public GameObject IntendedSelector => _intendedSelector;

    [Tooltip("The audio clip that plays when this selectable is interacted with.")]
    [SerializeField] private AudioClip _interactionAudioClip;
    public AudioClip InteractionAudioClip => _interactionAudioClip;


    // Events.
    public event EventHandler OnInteracted;
    public event Action<Selectable> OnPointerEnterEvent;


    // Non-MonoBehaviour.
    protected void RaiseOnInteractedEvent() => OnInteracted?.Invoke(this, EventArgs.Empty);


    // Event Handlers.
    public virtual void OnPointerEnter(PointerEventData eventData) => 
        OnPointerEnterEvent?.Invoke(GetComponent<Selectable>());
}
