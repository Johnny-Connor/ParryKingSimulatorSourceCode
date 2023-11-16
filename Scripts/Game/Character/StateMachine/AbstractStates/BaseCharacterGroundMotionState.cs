using UnityEngine;

public abstract class BaseCharacterGroundMotionState : BaseCharacterState
{
    // Properties. 
    protected abstract float StateMoveSpeed { get; }
    protected virtual Vector3 StateMovementDirection
    {
        get
        {
            Vector3 characterTransformForward = 
                _controlCharacter.GetComponent<CharacterMovementStats>().transform.forward;
            bool isCharacterTargetingTarget = 
                _controlCharacter.GetComponent<CharacterCombatStats>().CurrentTargetTransform;

            if (isCharacterTargetingTarget)
            {
                Vector2 inputDirection = _controlCharacter.PlayerInputActions.Game.Walk.ReadValue<Vector2>();

                /*
                Calculate the strafe movement direction based on the input direction and character's right
                direction.
                */
                Vector3 rightDir = Vector3.Cross(Vector3.up, characterTransformForward);
                Vector3 strafeDirection = inputDirection.x * rightDir;

                // Calculate the forward movement direction based on the input direction.
                Vector3 forwardDirection = inputDirection.y * characterTransformForward;

                // Combine the forward movement direction and the strafe movement direction.
                return forwardDirection + strafeDirection;
            }
            else
            {
                return characterTransformForward;
            }
        }
    }


    // Constructor.
    protected BaseCharacterGroundMotionState(
        ControlCharacter controlCharacter, 
        ICharacterStateMachineContext characterStateMachineContext)
    : base(controlCharacter, characterStateMachineContext)
    {
    }


    // State Methods.
    public override void OnEnter()
    {
        if (!_controlCharacter.IsPlayer)
        {
            _controlCharacter.TryGetComponent(out CombatDifficultyController combatDifficultyController);
            combatDifficultyController.OnAttackDelayTimerEnded += 
                CombatDifficultyController_OnAttackDelayTimerEnded;
        }

        base.OnEnter();
    }

    public override void OnExit()
    {
        if (!_controlCharacter.IsPlayer)
        {
            _controlCharacter.TryGetComponent(out CombatDifficultyController combatDifficultyController);
            combatDifficultyController.OnAttackDelayTimerEnded -= 
                CombatDifficultyController_OnAttackDelayTimerEnded;
        }

        base.OnExit();
    }

    public override void OnHandleStateTransitionsTick()
    {
        if (!_controlCharacter.IsPlayer) return; // Handled by AI states.
            
        if (_isGrounded)
        {
            float fallTimeout = _controlCharacter.GetComponent<CharacterMovementStats>().FallTimeout;
            _controlCharacter.GetComponent<CharacterMovementStats>().FallTimeoutDelta = fallTimeout;

            if (_controlCharacter.PlayerInputActions.Game.Roll.WasPerformedThisFrame())
            {
                SetState(_controlCharacter.RollState);
                return;
            }

            /*
            Prevents character from executing other actions when roll action starts. In practice, this
            prioritize roll over the actions below when they are pressed at the same time.
            */
            if (_controlCharacter.PlayerInputActions.Game.Roll.IsInProgress()) return;

            if (_controlCharacter.PlayerInputActions.Game.Jump.WasPerformedThisFrame() && 
                _controlCharacter.GetComponent<CharacterMovementStats>().JumpCooldownDelta <= 0)
            {
                SetState(_controlCharacter.JumpState);
                return;
            }

            if (_controlCharacter.PlayerInputActions.Game.Parry.WasPerformedThisFrame())
            {
                SetState(_controlCharacter.ParryState);
                return;
            }

            if (_controlCharacter.PlayerInputActions.Game.Attack.WasPerformedThisFrame())
            {
                CharacterCombatStats characterCombatStats = 
                    _controlCharacter.GetComponent<CharacterCombatStats>();

                if (characterCombatStats.IsAReachableParriedTargetAvailable())
                {
                    SetState(_controlCharacter.ParryStabState);
                    return;
                }
                else
                {
                    SetState(_controlCharacter.AttackState);
                    return;
                }
            }
        }
        else
        {
            void HandleGravityTransitions()
            {
                if (_controlCharacter.GetComponent<CharacterMovementStats>().FallTimeoutDelta <= 0)
                {
                    SetState(_controlCharacter.FallState);
                    return;
                }
                else
                {
                    _controlCharacter.GetComponent<CharacterMovementStats>().FallTimeoutDelta -= 
                        Time.deltaTime;
                }
            }

            HandleGravityTransitions();
        }
    }

    public override void OnMoveTick()
    {
        if (!_controlCharacter.IsPlayer) return; // Handled by AI's NavMesh.

        // Horizontal Movement.
        float horizontalVelocityMagnitude = 
            _controlCharacter.GetComponent<CharacterMovementStats>().HorizontalVelocityMagnitude;

        float normalizedInputMagnitude = 
            _controlCharacter.PlayerInputActions.Game.Walk.ReadValue<Vector2>().normalized.magnitude;
        float targetSpeed = StateMoveSpeed * normalizedInputMagnitude;

        float groundSpeedChangeRate = 
            _controlCharacter.GetComponent<CharacterMovementStats>().GroundSpeedChangeRate;

        float horizontalSpeed = Mathf.Lerp(
            horizontalVelocityMagnitude, 
            targetSpeed, 
            groundSpeedChangeRate * Time.deltaTime
        );
        

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
            StateMovementDirection.normalized * horizontalSpeed * Time.deltaTime +
            new Vector3(0.0f, _controlCharacter.GetComponent<CharacterMovementStats>().VerticalVelocity, 0.0f) * Time.deltaTime    
        );
    }

    public override abstract void OnRotateTick();


    // Class Methods.
    protected override void RotateFree()
    {
        Vector2 inputDirection = _controlCharacter.PlayerInputActions.Game.Walk.ReadValue<Vector2>();

        if (inputDirection == Vector2.zero) return;

        Transform camera = _controlCharacter.Camera;
        Transform character = _controlCharacter.GetComponent<CharacterMovementStats>().transform;
        float rotationSmoothness = 
            _controlCharacter.GetComponent<CharacterMovementStats>().RotationSmoothness;

        float targetRotation = 
            Mathf.Atan2(inputDirection.x, inputDirection.y) * Mathf.Rad2Deg + camera.eulerAngles.y;
        float newRotation = 
            Mathf.SmoothDampAngle(
                character.eulerAngles.y, 
                targetRotation, 
                ref _controlCharacter.GetComponent<CharacterMovementStats>().RotationVelocity, 
                rotationSmoothness);
        character.rotation = Quaternion.Euler(0f, newRotation, 0f);
    }


    // Event Handlers.
    private void CombatDifficultyController_OnAttackDelayTimerEnded() => SetState(_controlCharacter.AttackState);
}
