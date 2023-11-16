using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Scrollbar))]
public class MainMenuScrollbar : MainMenuSelectable
{
    // Other variables.
    private Scrollbar _scrollbar;
    private float _lastScrollbarValue;

    // Events.
    public event Action<bool> OnDidScrollTowardsStart;


    // MonoBehaviour.
    private void Awake()
    {
        _scrollbar = GetComponent<Scrollbar>();

        _lastScrollbarValue = _scrollbar.value;

        // Adds listeners to events.
        _scrollbar.onValueChanged.AddListener((float value) => OnValueChanged(value));
    }


    // Event Handlers.
    public void OnValueChanged(float value)
    {
        /*
        The event is firing twice, most likely because the scrollbar component rounds and updates its
        value right after it is altered through code. Since both the component and my algorithm use the 
        'Number Of Steps' variable to calculate the next scroll normalized value, they always obtain the 
        same result, resulting in this line preventing this event's logic from being executed twice.
        */
        if (value == _lastScrollbarValue) return;

        RaiseOnInteractedEvent();

        bool scrolledTowardsStart = value > _lastScrollbarValue;
        OnDidScrollTowardsStart?.Invoke(scrolledTowardsStart);

        _lastScrollbarValue = value;
    }
}
