using UnityEngine;

/*
Reminder: The values of variables in parent objects do not persist across their child objects. The
role of a parent object is to serve as a blueprint or template for its children, rather than
being a separate static object that holds or "saves" values.
*/
public abstract class BaseCharacterState : ICharacterState
{
    // Variables.
    protected ControlCharacter _controlCharacter;
    private ICharacterStateMachineContext _characterStateMachineContext;
    protected bool _isGrounded => 
        _controlCharacter.GetComponent<CharacterMovementStats>().IsGrounded;
    protected virtual float StateStaminaConsumption => 0;
    private bool _hasDied;


    // Constructor.
    protected BaseCharacterState(
        ControlCharacter controlCharacter, 
        ICharacterStateMachineContext characterStateMachineContext)
    {
        _controlCharacter = controlCharacter;
        _characterStateMachineContext = characterStateMachineContext;
    }


    // State Methods.
    public virtual void OnEnter()
    {
        void SubscribeToEvents()
        {
            CharacterStats characterStats = _controlCharacter.GetComponent<CharacterStats>();
            characterStats.OnDeath += CharacterStats_OnDeath;

            CharacterHitBoxController characterHitBoxBodyController = 
                _controlCharacter.GetComponent<CharacterHitBoxController>();
            characterHitBoxBodyController.OnHitTaken += HitBoxBodyController_OnHitTaken;
        }

        if (_isGrounded)
        {
            float actionRotationDirectionChoiceTime = 
                _controlCharacter.GetComponent<CharacterMovementStats>().ActionRotationDirectionChoiceTime;
            _controlCharacter.GetComponent<CharacterMovementStats>().ActionRotationDirectionChoiceTimeDelta = 
                actionRotationDirectionChoiceTime;
        }

        SubscribeToEvents();
    }

    public virtual void OnExit()
    {
        void UnsubscribeFromEvents()
        {
            CharacterStats characterStats = _controlCharacter.GetComponent<CharacterStats>();
            characterStats.OnDeath -= CharacterStats_OnDeath;

            CharacterHitBoxController characterHitBoxBodyController = 
                _controlCharacter.GetComponent<CharacterHitBoxController>();
            characterHitBoxBodyController.OnHitTaken -= HitBoxBodyController_OnHitTaken;
        }

        UnsubscribeFromEvents();
    }

    public virtual void OnTick()
    {
        if (_isGrounded)
        {
            _controlCharacter.GetComponent<CharacterMovementStats>().JumpCooldownDelta -= Time.deltaTime;
        }
    }

    public abstract void OnHandleStateTransitionsTick();
    public abstract void OnMoveTick();
    public abstract void OnRotateTick();


    // Class Methods.
    protected void SetState(BaseCharacterState newState)
    {
        if (newState is AttackState && !_controlCharacter.EnableAttack) return;

        float currentStamina = _controlCharacter.GetComponent<CharacterStats>().CurrentStamina;

        if (currentStamina == 0 && newState.StateStaminaConsumption > 0)
        {
            GoToAStaminaRecoveryState();
        }
        else
        {
            _characterStateMachineContext.SetState(newState);
        }
    }

    protected abstract void RotateFree();

    protected void RotateTowardsTarget(Vector3 targetPosition)
    {
        CharacterCombatStats characterCombatStats = _controlCharacter.GetComponent<CharacterCombatStats>();

        // Avoids getting stuck above/below target due to spinning around the target's Transform point.
        if (characterCombatStats.IsCollidingWithTargetedTargetInXZAxes) return;

        CharacterMovementStats characterMovementStats = _controlCharacter.GetComponent<CharacterMovementStats>();
        Transform characterTransform = characterMovementStats.transform;

        // Gets the direction from the character to the target.
        Vector3 targetDir = targetPosition - characterTransform.position;

        // Calculates the target rotation angle on the y-axis.
        float targetRotation = Mathf.Atan2(targetDir.x, targetDir.z) * Mathf.Rad2Deg;

        // Smoothly rotates towards the target.
        float rotationSmoothness = characterMovementStats.RotationSmoothness;
        float newRotation = Mathf.SmoothDampAngle(
            characterTransform.eulerAngles.y, 
            targetRotation, 
            ref _controlCharacter.GetComponent<CharacterMovementStats>().RotationVelocity, 
            rotationSmoothness
        );
        characterTransform.rotation = Quaternion.Euler(0f, newRotation, 0f);
    }

    protected void GoToAStaminaRecoveryState()
    {
        if (_controlCharacter.IsPlayer)
        {
            if (!_controlCharacter.PlayerInputActions.Game.Walk.IsPressed())
            {
                _characterStateMachineContext.SetState(_controlCharacter.IdleState);
                return;
            }
            
            if (_controlCharacter.PlayerInputActions.Game.Walk.IsPressed())
            {
                _characterStateMachineContext.SetState(_controlCharacter.WalkState);
                return;
            }
        }
        else
        {
            _characterStateMachineContext.SetState(_controlCharacter.WalkState);
            return;
        }        
    }

    protected void HandleStateToGroundMotionStateInputTransitions()
    {
        if (_controlCharacter.IsPlayer)
        {
            if (!_controlCharacter.PlayerInputActions.Game.Walk.IsPressed())
            {
                SetState(_controlCharacter.IdleState);
                return;
            }

            if (_controlCharacter.PlayerInputActions.Game.Sprint.IsPressed())
            {
                SetState(_controlCharacter.SprintState);
                return;
            }
            
            if (_controlCharacter.PlayerInputActions.Game.Walk.IsPressed())
            {
                SetState(_controlCharacter.WalkState);
                return;
            }
        }
        else
        {
            if (_controlCharacter.NavMeshController.HasReachedTarget)
            {
                SetState(_controlCharacter.IdleState);
                return;
            }
            else
            {
                SetState(_controlCharacter.WalkState);
                return;
            }
        }
    }

    protected void UseAirGravity()
    {
        float fallSpeedLimit = _controlCharacter.GetComponent<CharacterMovementStats>().FallSpeedLimit;
        float airGravity = _controlCharacter.GetComponent<CharacterMovementStats>().AirGravity;

        _controlCharacter.GetComponent<CharacterMovementStats>().VerticalVelocity = 
            Mathf.Clamp(
                _controlCharacter.GetComponent<CharacterMovementStats>().VerticalVelocity += 
                    airGravity * Time.deltaTime,
                fallSpeedLimit,
                _controlCharacter.GetComponent<CharacterMovementStats>().VerticalVelocity);
    }

    protected void UseGroundGravity()
    {
        float groundGravity = _controlCharacter.GetComponent<CharacterMovementStats>().GroundGravity;
        
        _controlCharacter.GetComponent<CharacterMovementStats>().VerticalVelocity = groundGravity;
    }


    // Events Handlers.
    private void CharacterStats_OnDeath() => _hasDied = true;

    private void HitBoxBodyController_OnHitTaken(HitInformation hitInformation)
    {
        if (_hasDied)
        {
            SetState(_controlCharacter.DeathState);
            return;
        }

        if (_isGrounded) SetState(_controlCharacter.HitState);
    }
}
