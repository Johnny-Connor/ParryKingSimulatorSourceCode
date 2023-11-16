using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCharacterTemporaryState : BaseCharacterState
{
    // Variables.
    private bool _canMovementCancel;
    private bool _canTemporaryStateCancel;
    private bool _hasAnimationEnded;


    // Properties.
    protected virtual bool IsInputBufferEnabled => true; 


    // Constructor.
    protected BaseCharacterTemporaryState(
        ControlCharacter controlCharacter, 
        ICharacterStateMachineContext characterStateMachineContext) 
    : base(controlCharacter, characterStateMachineContext)
    {
    }


    // State Methods.
    public override void OnEnter()
    {
        void SetupVariables()
        {
            _canMovementCancel = false;
            _canTemporaryStateCancel = false;
            _hasAnimationEnded = false;
        }

        void SubscribeToEvents()
        {
            CharacterAnimatorController characterAnimatorController = 
                _controlCharacter.GetComponent<CharacterAnimatorController>();

            characterAnimatorController.AnimationEventOnToTemporaryStateCancelAvailable += 
                CharacterAnimatorController_AnimationEventOnToTemporaryStateCancelAvailable;
                
            characterAnimatorController.AnimationEventOnToGroundMotionStateCancelAvailable += 
                CharacterAnimatorController_AnimationEventOnToGroundMotionStateCancelAvailable;

            characterAnimatorController.AnimationEventOnTemporaryStateAnimationEnd += 
                CharacterAnimatorController_AnimationEventOnTemporaryStateAnimationEnd;
        }

        void SetInitialRotation()
        {
            bool isTargetingATarget = 
                _controlCharacter.GetComponent<CharacterCombatStats>().CurrentTargetTransform;

            if (isTargetingATarget)
            {
                Transform characterTransform = 
                    _controlCharacter.GetComponent<CharacterMovementStats>().transform;
                Vector3 currentTargetPosition = 
                    _controlCharacter.GetComponent<CharacterCombatStats>().CurrentTargetTransform.position;
                Vector3 dirToTarget = currentTargetPosition - characterTransform.position;

                // Calculate the rotation needed to look at the target.
                Quaternion targetRotation = 
                    Quaternion.LookRotation(new Vector3(dirToTarget.x, 0f, dirToTarget.z));

                // Apply the rotation to the character.
                characterTransform.rotation = targetRotation;
            }
        }

        SetupVariables();
        SubscribeToEvents();
        SetInitialRotation();

        base.OnEnter();
    }

    public override void OnExit()
    {
        void UnsubscribeFromEvents()
        {
            CharacterAnimatorController characterAnimatorController = 
                _controlCharacter.GetComponent<CharacterAnimatorController>();

            characterAnimatorController.AnimationEventOnToTemporaryStateCancelAvailable -= 
                CharacterAnimatorController_AnimationEventOnToTemporaryStateCancelAvailable;

            characterAnimatorController.AnimationEventOnToGroundMotionStateCancelAvailable -= 
                CharacterAnimatorController_AnimationEventOnToGroundMotionStateCancelAvailable;

            characterAnimatorController.AnimationEventOnTemporaryStateAnimationEnd -= 
                CharacterAnimatorController_AnimationEventOnTemporaryStateAnimationEnd;
        }

        UnsubscribeFromEvents(); 

        base.OnExit();
    }

    public override void OnHandleStateTransitionsTick()
    {
        void TimeTransitions()
        {
            if (_controlCharacter.IsPlayer)
            {
                // Time Transitions.
                if (_hasAnimationEnded)
                {
                    SetState(_controlCharacter.IdleState);
                    return;
                }

                if (_canMovementCancel) HandleMovementCancelTransitions();

                if (_canTemporaryStateCancel) HandleTemporaryStateCancelTransitions();
            }
            else
            {
                if (_canMovementCancel) HandleStateToGroundMotionStateInputTransitions();
            }
        }

        void GravityTransitions()
        {
            if (_isGrounded)
            {
                float fallTimeout = 
                    _controlCharacter.GetComponent<CharacterMovementStats>().FallTimeout;

                _controlCharacter.GetComponent<CharacterMovementStats>().FallTimeoutDelta = fallTimeout;
            }
            else
            {
                if (_controlCharacter.GetComponent<CharacterMovementStats>().FallTimeoutDelta <= 0)
                {
                    SetState(_controlCharacter.FallState);
                    return;
                }
                else
                {
                    _controlCharacter.GetComponent<CharacterMovementStats>().FallTimeoutDelta -= Time.deltaTime;
                }
            }
        }

        TimeTransitions();
        GravityTransitions();
    }

    public override void  OnMoveTick()
    {
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
            new Vector3(0.0f, _controlCharacter.GetComponent<CharacterMovementStats>().VerticalVelocity, 0.0f) * Time.deltaTime    
        );
    }
    
    public override void OnRotateTick(){} // Do not rotate.


    // Class Methods.
    private void HandleTemporaryStateCancelTransitions()
    {
        void HandleTemporaryStateTransitionsWithInputBuffer()
        {
            // Define a dictionary to map input buffer to state references. A dictionary converts an entry to a defined output.
            Dictionary<PlayerCharacterInputActionsProcessor.InputBuffer, BaseCharacterTemporaryState> selectState = new()
            {
                { PlayerCharacterInputActionsProcessor.InputBuffer.None, null }, // Map None to null to handle the default case.
                { PlayerCharacterInputActionsProcessor.InputBuffer.Attack, _controlCharacter.AttackState },
                { PlayerCharacterInputActionsProcessor.InputBuffer.BackStep, _controlCharacter.BackStepState },
                { PlayerCharacterInputActionsProcessor.InputBuffer.Parry, _controlCharacter.ParryState },
                { PlayerCharacterInputActionsProcessor.InputBuffer.Roll, _controlCharacter.RollState }
            };

            PlayerCharacterInputActionsProcessor playerInputActionsProcessor = 
                _controlCharacter.PlayerInputActionsProcessor;

            PlayerCharacterInputActionsProcessor.InputBuffer inputBufferNextAction = 
                playerInputActionsProcessor.InputBufferNextAction;

            // Get the state reference from the dictionary based on the input buffer.
            BaseCharacterTemporaryState inputBuffState = selectState[inputBufferNextAction];

            // If the inputBuffState is null, transition to ground motion state.
            if (inputBuffState != null)
            {
                if (inputBuffState == _controlCharacter.AttackState)
                {
                    CharacterCombatStats characterCombatStats =
                        _controlCharacter.GetComponent<CharacterCombatStats>();

                    if (characterCombatStats.IsAReachableParriedTargetAvailable())
                    {
                        SetState(_controlCharacter.ParryStabState);
                        return;
                    }
                }

                // Set the state based on the inputBuffState reference.
                SetState(inputBuffState);
            }
        }

        void HandleTemporaryStateTransitionsWithNoInputBuffer()
        {
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

            if (_controlCharacter.PlayerInputActions.Game.Attack.WasPerformedThisFrame())
            {
                SetState(_controlCharacter.AttackState);
                return;
            }
            
            if (_controlCharacter.PlayerInputActions.Game.BackStep.WasPerformedThisFrame())
            {
                SetState(_controlCharacter.BackStepState);
                return;
            }

            if (_controlCharacter.PlayerInputActions.Game.Parry.WasPerformedThisFrame())
            {
                SetState(_controlCharacter.ParryState);
                return;
            }
        }

        if (IsInputBufferEnabled)
        {
            HandleTemporaryStateTransitionsWithInputBuffer();
        }
        else
        {
            HandleTemporaryStateTransitionsWithNoInputBuffer();
        }
    }

    private void HandleMovementCancelTransitions()
    {
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

    protected override void RotateFree(){} // Do not rotate.


    // Event Handlers.
    private void CharacterAnimatorController_AnimationEventOnToTemporaryStateCancelAvailable() => 
        _canTemporaryStateCancel = true;

    private void CharacterAnimatorController_AnimationEventOnToGroundMotionStateCancelAvailable() => 
        _canMovementCancel = true;
        
    private void CharacterAnimatorController_AnimationEventOnTemporaryStateAnimationEnd() => 
        _hasAnimationEnded = true;
}
