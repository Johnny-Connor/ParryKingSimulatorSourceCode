using UnityEngine;

public class JumpState : BaseCharacterAirMotionState
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
    
    private float _jumpGroundDetectionTimeoutDelta;


    // Constructor.
    public JumpState(
        ControlCharacter controlCharacter, 
        ICharacterStateMachineContext characterStateMachineContext)
    : base(controlCharacter, characterStateMachineContext)
    {
    }


    // State Methods.
    public override void OnEnter()
    {
        float jumpCooldown = _controlCharacter.GetComponent<CharacterMovementStats>().JumpCooldown;
        _controlCharacter.GetComponent<CharacterMovementStats>().JumpCooldownDelta = jumpCooldown;

        _jumpGroundDetectionTimeoutDelta = 
            _controlCharacter.GetComponent<CharacterMovementStats>().JumpGroundDetectionTimeout;

        float jumpHeight = _controlCharacter.GetComponent<CharacterMovementStats>().JumpHeight;
        float gravity =  _controlCharacter.GetComponent<CharacterMovementStats>().AirGravity;

        CharacterStats characterStats = _controlCharacter.GetComponent<CharacterStats>();
        characterStats.UpdateCurrentStamina(StateStaminaConsumption);

        // The square root of H * -2 * G = how much velocity needed to reach desired height.
        _controlCharacter.GetComponent<CharacterMovementStats>().VerticalVelocity = 
            Mathf.Sqrt(jumpHeight * -2f * gravity);

        base.OnEnter();
    }

    public override void OnHandleStateTransitionsTick()
    {
        if (_jumpGroundDetectionTimeoutDelta < 0 && _isGrounded)
        {
            HandleStateToGroundMotionStateInputTransitions();
            return;
        }

        _jumpGroundDetectionTimeoutDelta -= Time.deltaTime;
    }
}
