using System;

public class CharacterStateMachine : ICharacterStateMachineContext
{
    private ICharacterState _currentState;
    public event EventHandler OnEnteredState;
    public event EventHandler OnExitedState;

    public void Initialize(ICharacterState startingState)
    {
        _currentState = startingState;
        OnEnteredState?.Invoke(_currentState, EventArgs.Empty);
        _currentState.OnEnter();
    }

    public void DoTicks()
    {
        _currentState.OnTick();
        _currentState.OnHandleStateTransitionsTick();
        _currentState.OnMoveTick();
        _currentState.OnRotateTick();
    }

    void ICharacterStateMachineContext.SetState(ICharacterState newState)
    {
        _currentState.OnExit();
        OnExitedState?.Invoke(_currentState, EventArgs.Empty);

        _currentState = newState;
        OnEnteredState?.Invoke(newState, EventArgs.Empty);
        _currentState.OnEnter();
    }
}
