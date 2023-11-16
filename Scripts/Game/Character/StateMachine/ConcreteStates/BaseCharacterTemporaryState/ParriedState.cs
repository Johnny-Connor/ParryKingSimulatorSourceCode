using UnityEngine;

public class ParriedState : BaseCharacterTemporaryState
{
    // Constructor.
    public ParriedState(
        ControlCharacter controlCharacter, 
        ICharacterStateMachineContext characterStateMachineContext) : 
    base(controlCharacter, characterStateMachineContext)
    {
    }


    // Stats Methods.
    public override void OnEnter()
    {
        CharacterCombatStats characterCombatStats = _controlCharacter.GetComponent<CharacterCombatStats>();
        characterCombatStats.OnParryStabbed += CharacterCombatStats_OnParryStabbed;

        base.OnEnter();
    }

    public override void OnExit()
    {
        CharacterCombatStats characterCombatStats = _controlCharacter.GetComponent<CharacterCombatStats>();
        characterCombatStats.OnParryStabbed -= CharacterCombatStats_OnParryStabbed;

        base.OnExit();
    }


    // Event Handlers.
    private void CharacterCombatStats_OnParryStabbed(Transform stabberTransform) => 
        SetState(_controlCharacter.ParriedStabbedState);
}
