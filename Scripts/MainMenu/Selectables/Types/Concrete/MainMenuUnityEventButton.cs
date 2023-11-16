using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MainMenuUnityEventButton : ConfirmableSelectable
{
    // Events.
    [SerializeField] private UnityEvent _onCommandExecutedUnityEvent;


    // MonoBehaviour.
    public void Awake()
    {
        if (_onCommandExecutedUnityEvent.GetPersistentEventCount() == 0)
            Debug.LogWarning(gameObject.name + "'s MainMenuButton has no attached UnityEvents.");
    }


    // Non-MonoBehaviour.
    public override void Confirm() => _onCommandExecutedUnityEvent.Invoke();
}
