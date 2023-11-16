using UnityEngine;

public class SprintState : BaseCharacterGroundMotionState
{
    // Variables.
    protected override float StateStaminaConsumption
    {
        get
        {
            float staminaConsumptionRate = 
                _controlCharacter.GetComponent<CharacterStats>().StaminaConsumptionRate;
            return staminaConsumptionRate;
        }
    }
    protected override float StateMoveSpeed => 
        _controlCharacter.GetComponent<CharacterMovementStats>().SprintSpeed;
    protected override Vector3 StateMovementDirection => 
        _controlCharacter.GetComponent<CharacterMovementStats>().transform.forward;


    // Constructor.
    public SprintState(
        ControlCharacter controlCharacter, 
        ICharacterStateMachineContext characterStateMachineContext)
    : base(controlCharacter, characterStateMachineContext)
    {
    }


    // State Methods.
    public override void OnEnter()
    {
        CharacterStats characterStats = _controlCharacter.GetComponent<CharacterStats>();
        characterStats.OnStaminaExhaustion += CharacterStats_OnRanOutOfStamina;

        base.OnEnter();
    }

    public override void OnExit()
    {
        CharacterStats characterStats = _controlCharacter.GetComponent<CharacterStats>();
        characterStats.OnStaminaExhaustion -= CharacterStats_OnRanOutOfStamina;

        base.OnExit();
    }

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

            if (_controlCharacter.PlayerInputActions.Game.Walk.IsPressed() &&
            !_controlCharacter.PlayerInputActions.Game.Sprint.IsPressed())
            {
                SetState(_controlCharacter.WalkState);
                return;
            }
        }
        else
        {
            if (!_controlCharacter.NavMeshController.IsFarFromTarget)
            {
                SetState(_controlCharacter.WalkState);
                return;
            }
        }

        base.OnHandleStateTransitionsTick();
    }

    public override void OnRotateTick()
    {
        if (_controlCharacter.IsPlayer) RotateFree();
        else
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


    // Event Handlers.
    protected void CharacterStats_OnRanOutOfStamina() => GoToAStaminaRecoveryState();
}
