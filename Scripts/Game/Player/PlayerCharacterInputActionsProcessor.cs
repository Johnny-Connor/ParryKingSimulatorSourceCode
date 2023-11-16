using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacterInputActionsProcessor : MonoBehaviour
{
    // Variables.
    private Vector2 _airMotionOnEnterInputDirection;
    public Vector2 AirMotionOnEnterInputDirection => _airMotionOnEnterInputDirection;

    private Vector2 _lastNonZeroInputDirection;
    public Vector2 LastNonZeroInputDirection => _lastNonZeroInputDirection;

    public enum InputBuffer
    {
        None, 
        Attack,
        BackStep,
        Parry,
        Roll
    }
    private InputBuffer _inputBufferNextAction;
    public InputBuffer InputBufferNextAction => _inputBufferNextAction;

    private bool _wasInAnAirMotionState;


    // Components.
    private ControlCharacter _controlCharacter;
    private PlayerInputActions _playerInputActions;


    // MonoBehaviour.
    private void Awake() => _controlCharacter = GetComponent<ControlCharacter>();

    private void Start()
    {
        _playerInputActions = _controlCharacter.PlayerInputActions;    

        _playerInputActions.Game.Walk.performed += UpdateLastNonZeroInputDirection;

        _playerInputActions.Game.Attack.performed += (
            InputAction.CallbackContext context) => _inputBufferNextAction = InputBuffer.Attack;
        _playerInputActions.Game.BackStep.performed += 
            (InputAction.CallbackContext context) => _inputBufferNextAction = InputBuffer.BackStep;
        _playerInputActions.Game.Parry.performed += 
            (InputAction.CallbackContext context) => _inputBufferNextAction = InputBuffer.Parry;
        _playerInputActions.Game.Roll.performed += 
            (InputAction.CallbackContext context) => _inputBufferNextAction = InputBuffer.Roll;

        CharacterStateMachine characterStateMachine = _controlCharacter.CharacterStateMachine;
        characterStateMachine.OnEnteredState += CharacterStateMachine_OnEnteredState;
    }


    // Non-MonoBehaviour.
    private void UpdateLastNonZeroInputDirection(InputAction.CallbackContext context)
    {
        Vector2 inputDirection = _playerInputActions.Game.Walk.ReadValue<Vector2>();
        _lastNonZeroInputDirection = inputDirection;
    }


    // Event Handlers.
    private void CharacterStateMachine_OnEnteredState(object sender, EventArgs e)
    {
        if (sender is BaseCharacterAirMotionState)
        {
            if (!_wasInAnAirMotionState)
            {
                Vector2 inputDirection = _playerInputActions.Game.Walk.ReadValue<Vector2>();
                _airMotionOnEnterInputDirection = inputDirection;
            }

            _wasInAnAirMotionState = true;
        }
        else
        {
            _wasInAnAirMotionState = false;
        }

        // Resets input buffer processor.
        _inputBufferNextAction = InputBuffer.None;
    }
}
