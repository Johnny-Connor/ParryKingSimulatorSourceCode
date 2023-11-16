using UnityEngine;

public class BackStepState : BaseCharacterTemporaryState
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

    private bool _hasAirMomentumEnded;


    // Constructor.
    public BackStepState(
        ControlCharacter controlCharacter, 
        ICharacterStateMachineContext characterStateMachineContext) 
    : base(controlCharacter, characterStateMachineContext)
    {
    }


    // State Methods.
    public override void OnEnter()
    {
        _hasAirMomentumEnded = false;

        CharacterStats characterStats = _controlCharacter.GetComponent<CharacterStats>();
        characterStats.UpdateCurrentStamina(StateStaminaConsumption);

        CharacterAnimatorController characterAnimatorController = 
            _controlCharacter.GetComponent<CharacterAnimatorController>();
        characterAnimatorController.AnimationEventOnAirMomentumEnd += 
            CharacterAnimatorController_AnimationEventOnAirMomentumEnd;

        base.OnEnter();
    }

    public override void OnExit()
    {
        CharacterAnimatorController characterAnimatorController = 
            _controlCharacter.GetComponent<CharacterAnimatorController>();
        characterAnimatorController.AnimationEventOnAirMomentumEnd -= 
            CharacterAnimatorController_AnimationEventOnAirMomentumEnd;

        base.OnExit();
    }

    public override void OnMoveTick()
    {
        // Horizontal Movement.
        Vector3 moveDirection = _controlCharacter.GetComponent<CharacterMovementStats>().transform.forward;

        float horizontalSpeed;

        if (_hasAirMomentumEnded)
        {
            float horizontalVelocityMagnitude = 
                _controlCharacter.GetComponent<CharacterMovementStats>().HorizontalVelocityMagnitude;
            float groundSpeedChangeRate = 
                _controlCharacter.GetComponent<CharacterMovementStats>().GroundSpeedChangeRate;
            
            horizontalSpeed = Mathf.Lerp(
                horizontalVelocityMagnitude, 
                0, 
                groundSpeedChangeRate * Time.deltaTime
            );
        }
        else
        {
            float backStepSpeed = _controlCharacter.GetComponent<CharacterMovementStats>().BackStepSpeed;

            horizontalSpeed = backStepSpeed;
        }


        // Vertical Movement.
        if (_isGrounded)
        {
            UseGroundGravity();
        }
        else
        {
            UseAirGravity();
        }


        // Applies Movement.
        CharacterController characterController = 
            _controlCharacter.GetComponent<CharacterMovementStats>().CharacterController;

        characterController.Move(
            moveDirection.normalized * -1 * horizontalSpeed * Time.deltaTime +
            new Vector3(0.0f, _controlCharacter.GetComponent<CharacterMovementStats>().VerticalVelocity, 0.0f) * Time.deltaTime    
        );
    }


    // Event Handlers.
    private void CharacterAnimatorController_AnimationEventOnAirMomentumEnd() => _hasAirMomentumEnded = true;
}
