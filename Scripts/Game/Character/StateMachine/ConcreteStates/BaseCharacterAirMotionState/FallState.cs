public class FallState : BaseCharacterAirMotionState
{
    // Constructor.
    public FallState(
        ControlCharacter controlCharacter, 
        ICharacterStateMachineContext characterStateMachineContext) 
    : base(controlCharacter, characterStateMachineContext)
    {
    }


    // State Methods.
    public override void OnHandleStateTransitionsTick()
    {
        if (!_isGrounded) return;

        HandleStateToGroundMotionStateInputTransitions();
        return;
    }
}
