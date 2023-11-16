using UnityEngine;

public abstract class BaseCharacterAirMotionState : BaseCharacterState
{
    // Variables.
    /*
    Indicates whether combat mode has been activated during this air motion to ensure its physics 
    enforcement if the player disables it. This mirrors the mechanics observed in other Soulslike games,
    where the combat rotation does not deactivate if the player untargets their target while airborne.
    Note: If code readability starts decreasing too quick due to this variable, implementing combat air 
    motion states should be considered. This approach might even allow unifying rotation and direction code
    into a single method.
    */
    private bool _hasUsedCombatMode;


    // Properties.
    private bool ShouldUseCombatRotate()
    {
        float airCombatRotationSpeedLimit = 
            _controlCharacter.GetComponent<CharacterMovementStats>().AirCombatRotationSpeedLimit;
        float horizontalVelocityMagnitude = 
            _controlCharacter.GetComponent<CharacterMovementStats>().HorizontalVelocityMagnitude;
        bool isCharacterTargetingATarget = 
            _controlCharacter.GetComponent<CharacterCombatStats>().CurrentTargetTransform;

        return isCharacterTargetingATarget && 
            Mathf.Round(horizontalVelocityMagnitude) <= airCombatRotationSpeedLimit;
    }

    protected virtual Vector3 GetMovementDirection()
    {
        Vector3 characterTransformForward = 
            _controlCharacter.GetComponent<CharacterMovementStats>().transform.forward;
        bool isCollidingWithTargetInXZAxes = 
            _controlCharacter.GetComponent<CharacterCombatStats>().IsCollidingWithTargetedTargetInXZAxes;

        /*
        Enables movement direction control ("strafe") when rotating towards target and not being right
        above/below it. 
        */
        if ((ShouldUseCombatRotate() || _hasUsedCombatMode) && !isCollidingWithTargetInXZAxes)
        {
            _hasUsedCombatMode = true;

            PlayerCharacterInputActionsProcessor playerCharacterInputActionsProcessor = 
                _controlCharacter.PlayerInputActionsProcessor;
            
            // Prevents character from controlling his jump direction in combat rotate.
            Vector2 airMotionOnEnterInputDirection = 
                playerCharacterInputActionsProcessor.AirMotionOnEnterInputDirection;

            // Calculate strafe and forward directions as before
            Vector3 rightDir = Vector3.Cross(Vector3.up, characterTransformForward);
            Vector3 strafeDirection = airMotionOnEnterInputDirection.x * rightDir;
            Vector3 forwardDirection = airMotionOnEnterInputDirection.y * characterTransformForward;

            // Combine the forward and strafe movement directions
            return forwardDirection + strafeDirection;
        }

        return characterTransformForward;
    }


    // Constructor.
    protected BaseCharacterAirMotionState(
        ControlCharacter controlCharacter, 
        ICharacterStateMachineContext characterStateMachineContext) 
    : base(controlCharacter, characterStateMachineContext)
    {
    }


    // State Methods.
    public override void OnEnter()
    {
        _hasUsedCombatMode = false;

        base.OnEnter();
    }

    public override void OnExit()
    {
        if (_isGrounded)
        {
            float airHorizontalTargetSpeedControlDuration = 
                _controlCharacter.GetComponent<CharacterMovementStats>().AirHorizontalTargetSpeedControlDuration;
            _controlCharacter.GetComponent<CharacterMovementStats>().AirHorizontalTargetSpeedControlDurationDelta = airHorizontalTargetSpeedControlDuration;
        }

        base.OnExit();
    }

    public override void OnMoveTick()
    {
        // Horizontal Movement.
        float horizontalVelocityMagnitude = 
            _controlCharacter.GetComponent<CharacterMovementStats>().HorizontalVelocityMagnitude;

        float targetSpeed;

        if (_controlCharacter.GetComponent<CharacterMovementStats>().AirHorizontalTargetSpeedControlDurationDelta > 0)
        {
            float minimumHorizontalAirSpeedLimit = 
                _controlCharacter.GetComponent<CharacterMovementStats>().MinimumHorizontalAirSpeedLimit;
            float normalizedInputMagnitude = 
                _controlCharacter.PlayerInputActions.Game.Walk.ReadValue<Vector2>().normalized.magnitude;

            targetSpeed = 
                Mathf.Max(horizontalVelocityMagnitude, minimumHorizontalAirSpeedLimit) * 
                normalizedInputMagnitude;

            _controlCharacter.GetComponent<CharacterMovementStats>().AirHorizontalTargetSpeedControlDurationDelta -= Time.deltaTime;
        }
        else
        {
            targetSpeed = 0;
        }

        float airSpeedChangeRate = 
            _controlCharacter.GetComponent<CharacterMovementStats>().AirSpeedChangeRate;

        if (_controlCharacter.GetComponent<CharacterMovementStats>().ShouldBeSliding)
        {
            float slideSpeed = _controlCharacter.GetComponent<CharacterMovementStats>().SlideSpeed;

            _controlCharacter.GetComponent<CharacterMovementStats>().AirSlideVelocity = Mathf.Lerp(
                horizontalVelocityMagnitude, 
                slideSpeed, 
                airSpeedChangeRate * Time.time
            );
        }
        else
        {
            _controlCharacter.GetComponent<CharacterMovementStats>().AirSlideVelocity = 0;
        }

        targetSpeed += _controlCharacter.GetComponent<CharacterMovementStats>().AirSlideVelocity;

        float horizontalSpeed = Mathf.Lerp(
            horizontalVelocityMagnitude, 
            targetSpeed, 
            airSpeedChangeRate * Time.deltaTime
        );


        // Vertical Movement.
        UseAirGravity();


        // Applies Movement.
        CharacterController characterController = 
            _controlCharacter.GetComponent<CharacterMovementStats>().CharacterController;

        characterController.Move(
            GetMovementDirection().normalized * horizontalSpeed * Time.deltaTime +
            new Vector3(0.0f, _controlCharacter.GetComponent<CharacterMovementStats>().VerticalVelocity, 0.0f) * Time.deltaTime
        );
    }

    public override void OnRotateTick()
    {
        if (ShouldUseCombatRotate())
        {
            Vector3 currentTargetPosition = 
                _controlCharacter.GetComponent<CharacterCombatStats>().CurrentTargetTransform.position;

            RotateTowardsTarget(currentTargetPosition);
        }
        else if (!_controlCharacter.GetComponent<CharacterMovementStats>().ShouldBeSliding && !_hasUsedCombatMode)
        {
            RotateFree();
        }
    }


    // Class Methods.
    protected override void RotateFree()
    {
        float rotationSmoothness;

        if (_controlCharacter.GetComponent<CharacterMovementStats>().ActionRotationDirectionChoiceTimeDelta > 0)
        {
            rotationSmoothness = 0;
            _controlCharacter.GetComponent<CharacterMovementStats>().ActionRotationDirectionChoiceTimeDelta -= Time.deltaTime;
        }
        else
        {
            float airRotationSmoothnessMultiplier = 
                _controlCharacter.GetComponent<CharacterMovementStats>().AirRotationSmoothnessMultiplier;
            float airRotationSpeedInfluenceFactor = 
                _controlCharacter.GetComponent<CharacterMovementStats>().AirRotationSpeedInfluenceFactor;
            float horizontalVelocityMagnitude = 
                _controlCharacter.GetComponent<CharacterMovementStats>().HorizontalVelocityMagnitude;

            rotationSmoothness = 
                airRotationSmoothnessMultiplier + 
                horizontalVelocityMagnitude * airRotationSpeedInfluenceFactor;
        }

        Vector2 inputDirection = _controlCharacter.PlayerInputActions.Game.Walk.ReadValue<Vector2>();
        float targetRotation;

        if (inputDirection == Vector2.zero)
        {
            targetRotation = _controlCharacter.transform.eulerAngles.y;
        }
        else
        {
            Transform camera = _controlCharacter.Camera;
            targetRotation = 
            Mathf.Atan2(inputDirection.x, inputDirection.y) * Mathf.Rad2Deg + camera.eulerAngles.y;
        }

        Transform character = _controlCharacter.GetComponent<CharacterMovementStats>().transform;
        float newRotation = 
            Mathf.SmoothDampAngle(
                character.eulerAngles.y, 
                targetRotation, 
                ref _controlCharacter.GetComponent<CharacterMovementStats>().RotationVelocity, 
                rotationSmoothness);

        character.rotation = Quaternion.Euler(0f, newRotation, 0f);
    }
}