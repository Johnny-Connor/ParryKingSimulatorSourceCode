using UnityEngine;

public class IdleState : BaseCharacterGroundMotionState
{
    // Variables.
    protected override float StateStaminaConsumption
    {
        get
        {
            float staminaRecoveryRate = 
                _controlCharacter.GetComponent<CharacterStats>().StaminaRecoveryRate;
            return staminaRecoveryRate;
        }
    }
    
    protected override float StateMoveSpeed { get => 0; }


    // Constructor.
    public IdleState(
        ControlCharacter controlCharacter, 
        ICharacterStateMachineContext characterStateMachineContext)
    : base(controlCharacter, characterStateMachineContext)
    {
    }


    // State Methods.
    public override void OnTick()
    {
        CharacterStats characterStats = _controlCharacter.GetComponent<CharacterStats>();
        characterStats.UpdateCurrentStamina(StateStaminaConsumption);

        base.OnTick();
    }

    public override void OnHandleStateTransitionsTick()
    {
        if (_controlCharacter.IsPlayer)
        {
            if (_controlCharacter.PlayerInputActions.Game.Walk.IsPressed())
            {
                SetState(_controlCharacter.WalkState);
                return;
            }

            if (_controlCharacter.PlayerInputActions.Game.BackStep.WasPerformedThisFrame())
            {
                SetState(_controlCharacter.BackStepState);
                return;
            }
        }
        else
        {
            if (!_controlCharacter.NavMeshController.HasReachedTarget)
            {
                SetState(_controlCharacter.WalkState);
                return;
            }
        }

        base.OnHandleStateTransitionsTick();
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
}
