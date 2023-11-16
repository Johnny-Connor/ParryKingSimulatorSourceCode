public class HitState : BaseCharacterTemporaryState
{
    public HitState(
        ControlCharacter controlCharacter, 
        ICharacterStateMachineContext characterStateMachineContext) 
    : base(controlCharacter, characterStateMachineContext)
    {
    }
}
