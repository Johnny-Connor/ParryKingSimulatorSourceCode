using UnityEngine;

public class AttackState : BaseCharacterTemporaryState
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
    public AttackState(
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

        WeaponHitBoxController hitBoxWeaponController = 
            _controlCharacter.GetComponentInChildren<WeaponHitBoxController>();
        hitBoxWeaponController.OnGotParried += HitBoxWeaponController_OnGotParried;

        base.OnEnter();
    }

    public override void OnExit()
    {
        WeaponHitBoxController hitBoxWeaponController = 
            _controlCharacter.GetComponentInChildren<WeaponHitBoxController>();
        hitBoxWeaponController.OnGotParried -= HitBoxWeaponController_OnGotParried;

        base.OnExit();
    }

    public override void OnRotateTick()
    {
        bool isTargetingATarget = 
            _controlCharacter.GetComponent<CharacterCombatStats>().CurrentTargetTransform;

        if (isTargetingATarget)
        {
            Vector3 currentTargetPosition = 
                _controlCharacter.GetComponent<CharacterCombatStats>().CurrentTargetTransform.position;
            RotateTowardsTarget(currentTargetPosition);
        }
    }


    // Event Handlers.
    private void HitBoxWeaponController_OnGotParried() => SetState(_controlCharacter.ParriedState);
}
