using UnityEngine;

public class WalkState : BaseCharacterGroundMotionState
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
    
    protected override float StateMoveSpeed =>
        _controlCharacter.GetComponent<CharacterMovementStats>().WalkSpeed;


    // Constructor.
    public WalkState(
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
            if (!_controlCharacter.PlayerInputActions.Game.Walk.IsPressed())
            {
                SetState(_controlCharacter.IdleState);
                return;
            }

            if (_controlCharacter.PlayerInputActions.Game.Sprint.WasPerformedThisFrame())
            {
                SetState(_controlCharacter.SprintState);
                return;
            }
        }
        else
        {
            if (_controlCharacter.NavMeshController.IsFarFromTarget)
            {
                SetState(_controlCharacter.SprintState);
                return;
            }

            if (_controlCharacter.NavMeshController.HasReachedTarget)
            {
                SetState(_controlCharacter.IdleState);
                return;
            }
        }

        base.OnHandleStateTransitionsTick();
    }

    public override void OnRotateTick()
    {
        bool isTargetingATarget = 
            _controlCharacter.GetComponent<CharacterCombatStats>().CurrentTargetTransform;

        if (_controlCharacter.IsPlayer)
        {
            if (isTargetingATarget)
            {
                Vector3 currentTargetPosition = 
                    _controlCharacter.GetComponent<CharacterCombatStats>().CurrentTargetTransform.position;
                RotateTowardsTarget(currentTargetPosition);
            }
            else
            {
                RotateFree();
            }
        }
        else
        {
            if (isTargetingATarget)
            {
                Vector3 currentTargetPosition = 
                    _controlCharacter.GetComponent<CharacterCombatStats>().CurrentTargetTransform.position;
                RotateTowardsTarget(currentTargetPosition);
            }
        }
    }
}
