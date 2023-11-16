using UnityEngine;

public class DeathState : BaseCharacterState
{
    public DeathState(
        ControlCharacter controlCharacter, 
        ICharacterStateMachineContext characterStateMachineContext)
    : base(controlCharacter, characterStateMachineContext)
    {
    }

    public override void OnHandleStateTransitionsTick(){}

    public override void OnMoveTick()
    {
        if (!_controlCharacter.IsPlayer) return; // Handled by AI's NavMesh.

        // Horizontal Movement.
        Vector3 movementDirection = 
            _controlCharacter.GetComponent<CharacterMovementStats>().transform.forward;
        float horizontalVelocityMagnitude = 
            _controlCharacter.GetComponent<CharacterMovementStats>().HorizontalVelocityMagnitude;
        float targetSpeed = 0;
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
            movementDirection.normalized * horizontalSpeed * Time.deltaTime +
            new Vector3(0.0f, _controlCharacter.GetComponent<CharacterMovementStats>().VerticalVelocity, 0.0f) * Time.deltaTime
        );
    }

    public override void OnRotateTick(){}

    protected override void RotateFree(){}
}
