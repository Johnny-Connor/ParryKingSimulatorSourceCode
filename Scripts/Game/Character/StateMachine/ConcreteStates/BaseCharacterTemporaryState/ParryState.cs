public class ParryState : BaseCharacterTemporaryState
{
    // Variables.
    protected override float StateStaminaConsumption
    {
        get
        {
            float staminaConsumptionValue = 
                _controlCharacter.GetComponent<CharacterStats>().StaminaConsumptionValue;
            return staminaConsumptionValue;
        }
    }


    // Constructor.
    public ParryState(
        ControlCharacter controlCharacter, 
        ICharacterStateMachineContext characterStateMachineContext) 
    : base(controlCharacter, characterStateMachineContext)
    {
    }


    // State Methods.
    public override void OnEnter()
    {
        CharacterStats characterStats = _controlCharacter.GetComponent<CharacterStats>();
        characterStats.UpdateCurrentStamina(StateStaminaConsumption);

        base.OnEnter();
    }
}
