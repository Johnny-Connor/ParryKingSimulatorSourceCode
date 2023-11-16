using System;
using UnityEngine;

public class CharacterAnimatorController : MonoBehaviour
{
    #region Variables
    // Components.
    private Animator _animator;
    private CharacterCombatStats _characterCombatStats;
    private CharacterMovementStats _characterMovementStats;
    private ControlCharacter _controlCharacter;


    // Animator IDs.

    // Bool Parameters.
    private int _animIDIsGrounded = Animator.StringToHash("IsGrounded");
    private int _animIDGroundState = Animator.StringToHash("GroundState"); // Did not work as a trigger.

    // Float Parameters.
    private int _animIDSpeed = Animator.StringToHash("Speed");
    private int _animIDCombatMoveX = Animator.StringToHash("CombatMoveX");
    private int _animIDCombatMoveZ = Animator.StringToHash("CombatMoveZ");
    private int _animIDCombatBlend = Animator.StringToHash("CombatBlend");

    // Trigger Parameters.
    private int _animIDAttackState = Animator.StringToHash("AttackState");
    private int _animIDBackStepState = Animator.StringToHash("BackStepState");
    private int _animIDDeathStateAir = Animator.StringToHash("DeathStateAir");
    private int _animIDDeathStateHitBack = Animator.StringToHash("DeathStateHitBack");
    private int _animIDDeathStateHitFront = Animator.StringToHash("DeathStateHitFront");
    private int _animIDDeathStateParriedStabbed = Animator.StringToHash("DeathStateParriedStabbed");
    private int _animIDFallState = Animator.StringToHash("FallState");
    private int _animIDHitStateBack = Animator.StringToHash("HitStateBack");
    private int _animIDHitStateFront = Animator.StringToHash("HitStateFront");
    private int _animIDJumpState = Animator.StringToHash("JumpState");
    private int _animIDParriedStabbedState = Animator.StringToHash("ParriedStabbedState");
    private int _animIDParriedState = Animator.StringToHash("ParriedState");
    private int _animIDParryStabState = Animator.StringToHash("ParryStabState");
    private int _animIDParryState = Animator.StringToHash("ParryState");
    private int _animIDRollState = Animator.StringToHash("RollState");


    // By Value Variables.
    private bool _hasDied;
    private bool _shouldDetectGround = true;

    // Keeps track of the damp time used for blending the speed parameter.
    private float _speedBlendDampTime;
    
    // private enum AnimationSpeed
    // {
    //     Slow, 
    //     Default = 1,
    //     Fast
    // }


    // Constants.
    // Damp time used for blending the combat parameter.
    private const float COMBAT_BLEND_DAMP_TIME = 0.1f;

    // Damp time used for blending the combat movement parameters.
    private const float COMBAT_MOVE_X_Z_DAMP_TIME = 0.1f;

    private const float CLOSE_TO_ZERO_THRESHOLD = 0.1f;
    #endregion Variables


    #region Events
    // Events.

    // Death State Events.
    public event Action AnimationEventOnDeathStateAnimationStart;
    public event Action AnimationEventOnDeathStateAnimationEnd;

    // Base Temporary State Events.
    public event Action AnimationEventOnToTemporaryStateCancelAvailable;
    public event Action AnimationEventOnToGroundMotionStateCancelAvailable;
    public event Action AnimationEventOnTemporaryStateAnimationEnd;

    // Specific Temporary State Events.
    public event Action AnimationEventOnAirMomentumEnd;
    public event Action AnimationEventOnAttackHitFramesStart;
    public event Action AnimationEventOnAttackHitFramesEnd;
    public event Action AnimationEventOnCanControlDirection;
    public event Action AnimationEventOnCheckIfParriedStabbedDied;
    public event Action AnimationEventOnHitInflicted;
    public event Action AnimationEventOnHitTaken;
    public event Action AnimationEventOnParryStabberPushMotionStart;
    public event Action AnimationEventOnParryStabberPushMotionEnd;
    public event Action AnimationEventOnStopRotatingTowardsParryStabber;
    #endregion Events


    #region MonoBehaviour
    // MonoBehaviour.
    private void Awake()
    {
        void AssignComponents()
        {
            _animator = GetComponent<Animator>();
            _characterCombatStats = GetComponent<CharacterCombatStats>();
            _characterMovementStats = GetComponent<CharacterMovementStats>();
            _controlCharacter = GetComponent<ControlCharacter>();
        }

        AssignComponents();

        CharacterStats characterStats = GetComponent<CharacterStats>();
        characterStats.OnDeath += CharacterStats_OnDeath;
    }

    private void Start()
    {
        CharacterStateMachine characterStateMachine = _controlCharacter.CharacterStateMachine;
        characterStateMachine.OnEnteredState += CharacterStateMachine_OnEnteredState;
        characterStateMachine.OnExitedState += CharacterStateMachine_OnExitedState;

        /*
        To ensure proper event sequencing, it is important to subscribe to the OnHitTaken event after the
        OnDeath event. This prevents OnHitTaken from being invoked before OnDeath, maintaining the
        intended behavior (_hasDied being set before OnHitTaken is called).
        */
        CharacterHitBoxController characterHitBoxBodyController = GetComponent<CharacterHitBoxController>();
        characterHitBoxBodyController.OnHitTaken += HitBoxBodyController_OnHitTaken;
    }

    private void Update()
    {
        CombatBlendController();
        GroundDetection();
        SpeedParameterController();
    }
    #endregion MonoBehaviour


    #region Non-MonoBehaviour
    // Non-MonoBehaviour.
    private void CombatBlendController()
    {
        bool shouldStrafe = 
            _characterCombatStats.CurrentTargetTransform && 
            Mathf.Round(_animator.GetFloat(_animIDSpeed)) <= 
            GetComponent<CharacterMovementStats>().WalkSpeed;
        
        Vector2 inputDirection = 
            _controlCharacter.IsPlayer ?
            _controlCharacter.PlayerInputActions.Game.Walk.ReadValue<Vector2>() :
            GetComponent<NavMeshController>().HorizontalDirectionLocal;

        if (shouldStrafe)
        {
            // Blending to combat mode.
            _animator.SetFloat(_animIDCombatBlend, 1, COMBAT_BLEND_DAMP_TIME, Time.deltaTime);

            // X movement.
            _animator.SetFloat(
                _animIDCombatMoveX, 
                inputDirection.x, 
                COMBAT_MOVE_X_Z_DAMP_TIME, 
                Time.deltaTime);
            if (Mathf.Abs(_animator.GetFloat(_animIDCombatMoveX)) < CLOSE_TO_ZERO_THRESHOLD)
            {
                _animator.SetFloat(_animIDCombatMoveX, 0);
            }

            // Z movement.
            _animator.SetFloat(
                _animIDCombatMoveZ, 
                inputDirection.y, 
                COMBAT_MOVE_X_Z_DAMP_TIME, 
                Time.deltaTime);
            if (Mathf.Abs(_animator.GetFloat(_animIDCombatMoveZ)) < CLOSE_TO_ZERO_THRESHOLD)
            {
                _animator.SetFloat(_animIDCombatMoveZ, 0);
            }
        }
        else
        {
            // Rounds the combat state blend to 0 if it falls below the threshold.
            if (_animator.GetFloat(_animIDCombatBlend) < CLOSE_TO_ZERO_THRESHOLD)
            {
                _animator.SetFloat(_animIDCombatBlend, 0);
            }

            // Blend the combat state parameter towards 0 over time.
            _animator.SetFloat(_animIDCombatBlend, 0, COMBAT_BLEND_DAMP_TIME, Time.deltaTime);
        }
    }

    private void SpeedParameterController()
    {
        float horizontalVelocityMagnitude;

        if (_controlCharacter.IsPlayer)
        {
            horizontalVelocityMagnitude =
                _controlCharacter.GetComponent<CharacterMovementStats>().HorizontalVelocityMagnitude;
        }
        else
        {
            horizontalVelocityMagnitude =
                _controlCharacter.NavMeshController.HorizontalVelocityMagnitude;
        }

        // Updates the speed blend damp time based on the current speed and the speed change rate.
        _speedBlendDampTime = Mathf.Lerp(
            _speedBlendDampTime,
            horizontalVelocityMagnitude,
            _characterMovementStats.GroundSpeedChangeRate * Time.deltaTime 
        );

        // Rounds speed blend to 0 if it falls below the threshold.
        if (_speedBlendDampTime < CLOSE_TO_ZERO_THRESHOLD) _speedBlendDampTime = 0f;
        // Set the speed blend parameter in the animator.
        _animator.SetFloat(_animIDSpeed, _speedBlendDampTime);
    }

    private void GroundDetection()
    {
        if (_shouldDetectGround && _characterMovementStats.IsGrounded)
        {
            _animator.SetBool(_animIDIsGrounded, true);
        }
        else
        {
            _animator.SetBool(_animIDIsGrounded, false);
        }
    }

    // public void RandomizeAnimatorSpeed()
    // {
    //     int enumLength = Enum.GetValues(typeof(AnimationSpeed)).Length;
    //     int randomIndex = UnityEngine.Random.Range(0, enumLength);

    //     switch ((AnimationSpeed)randomIndex)
    //     {
    //         case AnimationSpeed.Slow:
    //             _animator.speed = 0.9f;
    //             return;

    //         case AnimationSpeed.Default:
    //             _animator.speed = (int)AnimationSpeed.Default;
    //             return;

    //         case AnimationSpeed.Fast:
    //             _animator.speed = 1.1f;
    //             return;
    //     }
    // }

    // public void ResetAnimatorSpeed() => _animator.speed = (int) AnimationSpeed.Default;
    #endregion Non-MonoBehaviour


    #region Event Handlers
    // Event Handlers.
    private void CharacterStateMachine_OnEnteredState(object sender, EventArgs e)
    {
        if (sender is BaseCharacterGroundMotionState)
        {
            _animator.SetBool(_animIDGroundState, true);
            return;
        }
        else
        {
            _animator.SetBool(_animIDGroundState, false);
        }

        switch (sender.GetType().Name)
        {
            case nameof(AttackState):
                _animator.SetTrigger(_animIDAttackState);
                break;
            case nameof(BackStepState):
                _animator.SetTrigger(_animIDBackStepState);
                break;
            case nameof(FallState):
                _animator.SetTrigger(_animIDFallState);
                break;
            case nameof(JumpState):
                _shouldDetectGround = false;
                _animator.SetTrigger(_animIDJumpState);
                break;
            case nameof(ParriedStabbedState):
                _animator.SetTrigger(_animIDParriedStabbedState);
                break;
            case nameof(ParriedState):
                _animator.SetTrigger(_animIDParriedState);
                break;
            case nameof(ParryStabState):
                _animator.SetTrigger(_animIDParryStabState);
                break;
            case nameof(ParryState):
                _animator.SetTrigger(_animIDParryState);
                break;
            case nameof(RollState):
                _animator.SetTrigger(_animIDRollState);
                break;
        }
    }

    private void CharacterStateMachine_OnExitedState(object sender, EventArgs e)
    {
        if (sender is JumpState) _shouldDetectGround = true;
    }

    private void CharacterStats_OnDeath() => _hasDied = true;

    private void HitBoxBodyController_OnHitTaken(HitInformation hitInformation)
    {
        if (_animator.GetBool(_animIDIsGrounded))
        {
            if (hitInformation.hitDirection == HitInformation.HitDirection.Front)
            {
                _animator.SetTrigger(_hasDied ? _animIDDeathStateHitFront : _animIDHitStateFront);
            }
            else
            {
                _animator.SetTrigger(_hasDied ? _animIDDeathStateHitBack : _animIDHitStateBack);
            }
        }
        else if (_hasDied) _animator.SetTrigger(_animIDDeathStateAir);
    }

    // Base Events.
    private void OnAnimationStart(string animationStateType)
    {
        switch (animationStateType)
        {
            case "DeathState":
                AnimationEventOnDeathStateAnimationStart?.Invoke();
                return;
            default:
                Debug.LogWarning("animationStateType string not found.");
                return;
        }
    }

    private void OnAnimationEnd(string animationStateType)
    {
        switch (animationStateType)
        {
            case "DeathState":
                AnimationEventOnDeathStateAnimationEnd?.Invoke();
                return;
            case "TemporaryState":
                AnimationEventOnTemporaryStateAnimationEnd?.Invoke();
                return;
            default:
                Debug.LogWarning("animationStateType string not found.");
                return;
        }
    }

    // Base Temporary State Events.
    private void OnToTemporaryStateCancelAvailable(AnimationEvent animationEvent) => 
        AnimationEventOnToTemporaryStateCancelAvailable?.Invoke();
    private void OnToGroundMotionStateCancelAvailable(AnimationEvent animationEvent) => 
        AnimationEventOnToGroundMotionStateCancelAvailable?.Invoke();

    // Specific Temporary State Events.
    private void OnAirMomentumEnd(AnimationEvent animationEvent) => 
        AnimationEventOnAirMomentumEnd?.Invoke();
    private void OnAttackHitFramesStart(AnimationEvent animationEvent) => 
        AnimationEventOnAttackHitFramesStart?.Invoke();
    private void OnAttackHitFramesEnd(AnimationEvent animationEvent) => 
        AnimationEventOnAttackHitFramesEnd?.Invoke();
    private void OnCanControlDirection(AnimationEvent animationEvent) => 
        AnimationEventOnCanControlDirection?.Invoke();
    private void OnCheckIfParriedStabbedDied(AnimationEvent animationEvent)
    {
        AnimationEventOnCheckIfParriedStabbedDied?.Invoke();

        if (_hasDied) _animator.SetTrigger(_animIDDeathStateParriedStabbed);
    }
    private void OnHitInflicted(AnimationEvent animationEvent) => AnimationEventOnHitInflicted?.Invoke();
    private void OnHitTaken(AnimationEvent animationEvent) => AnimationEventOnHitTaken?.Invoke();
    private void OnParryStabberPushMotionStart(AnimationEvent animationEvent) => 
        AnimationEventOnParryStabberPushMotionStart?.Invoke();
    private void OnParryStabberPushMotionEnd(AnimationEvent animationEvent) => 
        AnimationEventOnParryStabberPushMotionEnd?.Invoke();
    private void OnStopRotatingTowardsParryStabber(AnimationEvent animationEvent) => 
        AnimationEventOnStopRotatingTowardsParryStabber?.Invoke();
    #endregion Event Handlers
}
