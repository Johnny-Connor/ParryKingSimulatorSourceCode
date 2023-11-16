using System;
using UnityEngine;

public class MainMenuStateMachine : IMainMenuStateMachineContext
{
    // Variables.

    // States.
    private IMainMenuState _currentState;

    // Events.
    public event EventHandler OnEnteredState;
    public event EventHandler OnExitedState;
    public event Action OnReturnedToMenuState;


    // Methods.
    public void Initialize(IMainMenuState startingState)
    {
        _currentState = startingState;
        OnEnteredState?.Invoke(_currentState, EventArgs.Empty);
        _currentState.OnStateEnter();
    }

    public void DoAnyKey() => _currentState.OnAnyKeyPerformed();

    public void DoMove(Vector2 direction) => _currentState.OnMovePerformed(direction);
    
    public void DoConfirm() => _currentState.OnConfirmPerformed();

    public void DoBack() => _currentState.OnBackPerformed();

    void IMainMenuStateMachineContext.SetState(IMainMenuState newState)
    {
        // Checks if has returned to menu state.
        if (newState is MenuState && _currentState is not StartState) OnReturnedToMenuState?.Invoke();

        _currentState.OnStateExit();
        OnExitedState?.Invoke(_currentState, EventArgs.Empty);

        _currentState = newState;

        OnEnteredState?.Invoke(newState, EventArgs.Empty);
        _currentState.OnStateEnter();
    }
}
