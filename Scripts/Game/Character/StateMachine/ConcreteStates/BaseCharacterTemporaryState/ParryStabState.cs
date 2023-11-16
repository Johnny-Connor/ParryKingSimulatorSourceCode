using UnityEngine;

public class ParryStabState : BaseCharacterTemporaryState
{
    // Variables.
    private Vector3 _parryStabbedPosition;


    // Properties.
    protected override float StateStaminaConsumption
    {
        get
        {
            float staminaConsumptionValue = 
                _controlCharacter.GetComponent<CharacterStats>().StaminaConsumptionValue;
            return staminaConsumptionValue;
        }
    }

    protected override bool IsInputBufferEnabled => false;


    // Constructor.
    public ParryStabState(
        ControlCharacter controlCharacter, 
        ICharacterStateMachineContext characterStateMachineContext) 
    : base(controlCharacter, characterStateMachineContext)
    {
    }


    // State Methods.
    public override void OnEnter()
    {
        CharacterStats characterStats = _controlCharacter.GetComponent<CharacterStats>();
        characterStats.UpdateCurrentStamina(StateStaminaConsumption);

        Transform parriedStabbedTransform = 
            _controlCharacter.GetComponent<CharacterCombatStats>().ParryStabbedTransform;
        CharacterCombatStats _parriedStabbedCharacterCombatStats = 
            parriedStabbedTransform.GetComponentInParent<CharacterCombatStats>();
        
        Transform transform = _controlCharacter.transform;
        _parriedStabbedCharacterCombatStats.OnParryStabbed(transform);

        _parryStabbedPosition = parriedStabbedTransform.position;

        base.OnEnter();
    }

    public override void OnMoveTick()
    {
        // Horizontal Movement.

        // Positioning Variables.
        Vector3 position = _controlCharacter.GetComponent<CharacterMovementStats>().transform.position;
        float distanceFromTarget = Vector3.Distance(position, _parryStabbedPosition);

        float stoppingDistance =
            _controlCharacter.GetComponent<CharacterCombatStats>().ParryStabStoppingDistance;

        // Movement Variables.
        float horizontalVelocityMagnitude = 
            _controlCharacter.GetComponent<CharacterMovementStats>().HorizontalVelocityMagnitude;
        float groundSpeedChangeRate = 
            _controlCharacter.GetComponent<CharacterMovementStats>().GroundSpeedChangeRate;
        float moveSpeed = _controlCharacter.GetComponent<CharacterMovementStats>().WalkSpeed;
        float horizontalSpeed = 0;

        if (distanceFromTarget > stoppingDistance)
        {
            horizontalSpeed = Mathf.Lerp(
                horizontalVelocityMagnitude, 
                moveSpeed, 
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
        Vector3 characterTransformForward = 
            _controlCharacter.GetComponent<CharacterMovementStats>().transform.forward;

        characterController.Move(
            characterTransformForward.normalized * horizontalSpeed * Time.deltaTime +
            new Vector3(0.0f, _controlCharacter.GetComponent<CharacterMovementStats>().VerticalVelocity, 0.0f) * Time.deltaTime    
        );
    }

    public override void OnRotateTick() => RotateTowardsTarget(_parryStabbedPosition);
}
