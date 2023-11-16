using UnityEngine;

public class RollState : BaseCharacterTemporaryState
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

    private bool _canControlDirection;
    private bool _hasAirMomentumEnded;


    // Constructor.
    public RollState(
        ControlCharacter controlCharacter, 
        ICharacterStateMachineContext characterStateMachineContext) 
    : base(controlCharacter, characterStateMachineContext)
    {
    }


    // State Methods.
    public override void OnEnter()
    {
        _canControlDirection = false;
        _hasAirMomentumEnded = false;

        CharacterStats characterStats = _controlCharacter.GetComponent<CharacterStats>();
        characterStats.UpdateCurrentStamina(StateStaminaConsumption);

        CharacterAnimatorController characterAnimatorController = 
            _controlCharacter.GetComponent<CharacterAnimatorController>();
        characterAnimatorController.AnimationEventOnCanControlDirection += 
            CharacterAnimatorController_AnimationEventOnCanControlDirection;
        characterAnimatorController.AnimationEventOnAirMomentumEnd += 
            CharacterAnimatorController_AnimationEventOnAirMomentumEnd;

        base.OnEnter();
    }

    public override void OnExit()
    {
        CharacterAnimatorController characterAnimatorController = 
            _controlCharacter.GetComponent<CharacterAnimatorController>();
        characterAnimatorController.AnimationEventOnCanControlDirection -= 
            CharacterAnimatorController_AnimationEventOnCanControlDirection;
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
            PlayerInputActions playerInputActions = _controlCharacter.PlayerInputActions;
            float targetSpeed = 0;

            if (_controlCharacter.PlayerInputActions.Game.Sprint.IsPressed())
            {
                float sprintSpeed = _controlCharacter.GetComponent<CharacterMovementStats>().SprintSpeed;
                targetSpeed = sprintSpeed;
            }
            else if (_controlCharacter.PlayerInputActions.Game.Walk.IsPressed())
            {
                float walkSpeed = _controlCharacter.GetComponent<CharacterMovementStats>().WalkSpeed;
                targetSpeed = walkSpeed;
            }

            float horizontalVelocityMagnitude = 
                _controlCharacter.GetComponent<CharacterMovementStats>().HorizontalVelocityMagnitude;
            float groundSpeedChangeRate = 
                _controlCharacter.GetComponent<CharacterMovementStats>().GroundSpeedChangeRate;
            
            horizontalSpeed = Mathf.Lerp(
                horizontalVelocityMagnitude, 
                targetSpeed, 
                groundSpeedChangeRate * Time.deltaTime
            );
        }
        else
        {
            float rollSpeed = _controlCharacter.GetComponent<CharacterMovementStats>().RollSpeed;

            horizontalSpeed = rollSpeed;
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
            moveDirection.normalized * horizontalSpeed * Time.deltaTime +
            new Vector3(0.0f, _controlCharacter.GetComponent<CharacterMovementStats>().VerticalVelocity, 0.0f) * Time.deltaTime    
        );
    }

    public override void OnRotateTick() => RotateFree();


    // Class Methods.
    protected override void RotateFree()
    {
        float newRotationSmoothness;

        if (_controlCharacter.GetComponent<CharacterMovementStats>().ActionRotationDirectionChoiceTimeDelta > 0)
        {
            newRotationSmoothness = 0;

            _controlCharacter.GetComponent<CharacterMovementStats>().ActionRotationDirectionChoiceTimeDelta -= Time.deltaTime;
        }
        else
        {
            newRotationSmoothness = 
                _canControlDirection ? 
                    _controlCharacter.GetComponent<CharacterMovementStats>().RotationSmoothness : 
                    Mathf.Infinity;
        }

        /*
        Using the last non-zero input direction to prevent the character from rotating towards the camera
        view when no buttons are being pressed.
        */
        Vector2 lastNonZeroInputDirection = 
            _controlCharacter.PlayerInputActionsProcessor.LastNonZeroInputDirection;
        Transform camera = _controlCharacter.Camera;
        float targetRotation = 
            Mathf.Atan2(lastNonZeroInputDirection.x, lastNonZeroInputDirection.y) * Mathf.Rad2Deg + 
            camera.eulerAngles.y;

        Transform characterTransform = _controlCharacter.GetComponent<CharacterMovementStats>().transform;
        float newRotation = 
            Mathf.SmoothDampAngle(
                characterTransform.eulerAngles.y, 
                targetRotation, 
                ref _controlCharacter.GetComponent<CharacterMovementStats>().RotationVelocity, 
                newRotationSmoothness);

        characterTransform.rotation = Quaternion.Euler(0f, newRotation, 0f);
    }


    // Event Handlers.
    private void CharacterAnimatorController_AnimationEventOnCanControlDirection() => 
        _canControlDirection = true;
    private void CharacterAnimatorController_AnimationEventOnAirMomentumEnd() => 
        _hasAirMomentumEnded = true;
}
