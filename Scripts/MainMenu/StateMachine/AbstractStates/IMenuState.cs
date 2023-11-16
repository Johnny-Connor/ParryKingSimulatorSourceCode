using UnityEngine;

public interface IMainMenuState
{
    public abstract bool IsPopUpScreen { get; }

    void OnStateEnter();
    void OnStateExit();

    void OnAnyKeyPerformed();
    void OnMovePerformed(Vector2 direction);
    void OnConfirmPerformed();
    void OnBackPerformed();

    void GoToState(IMainMenuState newState);
}
