using System;
using UnityEngine;

public class ParriedStabbedState : BaseCharacterTemporaryState
{
    // Variables.
    private bool _isBeingPushed;
    private bool _shouldRotateTowardsTarget;
    private bool _hasDied;


    // Properties.
    protected override bool IsInputBufferEnabled { get => false; }


    // Events.
    public Action<int> OnHitTaken;


    // Constructor.
    public ParriedStabbedState(
        ControlCharacter controlCharacter, 
        ICharacterStateMachineContext characterStateMachineContext) 
    : base(controlCharacter, characterStateMachineContext)
    {
    }


    // State Methods.
    public override void OnEnter()
    {
        _isBeingPushed = false;
        _shouldRotateTowardsTarget = true;

        CharacterAnimatorController characterAnimatorController = 
            _controlCharacter.GetComponent<CharacterAnimatorController>();
        characterAnimatorController.AnimationEventOnCheckIfParriedStabbedDied += 
            CharacterAnimatorController_AnimationEventOnCheckIfParriedStabbedDied;
        characterAnimatorController.AnimationEventOnHitTaken += 
            CharacterAnimatorController_AnimationEventOnHitTaken;
        characterAnimatorController.AnimationEventOnParryStabberPushMotionStart += 
            CharacterAnimatorController_AnimationEventOnParryStabberPushMotionStart;
        characterAnimatorController.AnimationEventOnParryStabberPushMotionEnd += 
            CharacterAnimatorController_AnimationEventOnParryStabberPushMotionEnd;
        characterAnimatorController.AnimationEventOnStopRotatingTowardsParryStabber += 
            CharacterAnimatorController_AnimationEventOnStopRotatingTowardsParryStabber;

        CharacterStats characterStats = _controlCharacter.GetComponent<CharacterStats>();
        characterStats.OnDeath += CharacterStats_OnDeath;

        base.OnEnter();
    }

    public override void OnExit()
    {
        CharacterAnimatorController characterAnimatorController = 
            _controlCharacter.GetComponent<CharacterAnimatorController>();
        characterAnimatorController.AnimationEventOnHitTaken -= 
            CharacterAnimatorController_AnimationEventOnHitTaken;
        characterAnimatorController.AnimationEventOnParryStabberPushMotionStart -= 
            CharacterAnimatorController_AnimationEventOnParryStabberPushMotionStart;
        characterAnimatorController.AnimationEventOnParryStabberPushMotionEnd -= 
            CharacterAnimatorController_AnimationEventOnParryStabberPushMotionEnd;
        characterAnimatorController.AnimationEventOnStopRotatingTowardsParryStabber -= 
            CharacterAnimatorController_AnimationEventOnStopRotatingTowardsParryStabber;

        CharacterStats characterStats = _controlCharacter.GetComponent<CharacterStats>();
        characterStats.OnDeath -= CharacterStats_OnDeath;

        base.OnExit();
    }

    public override void OnMoveTick()
    {
        // Horizontal Movement.
        Vector3 moveDirection = 
            _controlCharacter.GetComponent<CharacterMovementStats>().transform.forward;

        float horizontalSpeed;
        float horizontalVelocityMagnitude = 
            _controlCharacter.GetComponent<CharacterMovementStats>().HorizontalVelocityMagnitude;
        float groundSpeedChangeRate = 
            _controlCharacter.GetComponent<CharacterMovementStats>().GroundSpeedChangeRate;

        if (_isBeingPushed)
        {
            float parriedStabbedPushMotionSpeed = 
                _controlCharacter.GetComponent<CharacterMovementStats>().ParriedStabbedPushMotionSpeed;
            horizontalSpeed = parriedStabbedPushMotionSpeed;
        }
        else
        {
            horizontalSpeed = Mathf.Lerp(
                horizontalVelocityMagnitude, 
                0, 
                groundSpeedChangeRate * Time.deltaTime
            );
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

    public override void OnRotateTick()
    {
        if (_shouldRotateTowardsTarget)
        {
            Vector3 _parryStabberPosition = 
                _controlCharacter.GetComponent<CharacterCombatStats>().ParryStabberTransform.position;
            RotateTowardsTarget(_parryStabberPosition);
        }
    }


    // Event Handlers.
    private void CharacterAnimatorController_AnimationEventOnCheckIfParriedStabbedDied()
    {
        if (_hasDied) SetState(_controlCharacter.DeathState);
    }
    private void CharacterAnimatorController_AnimationEventOnHitTaken()
    {
        Transform parryStabberTransform = 
            _controlCharacter.GetComponent<CharacterCombatStats>().ParryStabberTransform;
        int parryStabberParryStabDamage = 
            parryStabberTransform.GetComponent<CharacterStats>().ParryStabDamage;
        OnHitTaken?.Invoke(parryStabberParryStabDamage);
    }
    private void CharacterAnimatorController_AnimationEventOnParryStabberPushMotionStart() => 
        _isBeingPushed = true;
    private void CharacterAnimatorController_AnimationEventOnParryStabberPushMotionEnd() => 
        _isBeingPushed = false;
    private void CharacterAnimatorController_AnimationEventOnStopRotatingTowardsParryStabber() => 
        _shouldRotateTowardsTarget = false;
    private void CharacterStats_OnDeath() => _hasDied = true;
}
