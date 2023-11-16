public interface ICharacterState
{
    void OnEnter();
    void OnExit();

    void OnTick();
    void OnHandleStateTransitionsTick();
    void OnMoveTick();
    void OnRotateTick();
}
