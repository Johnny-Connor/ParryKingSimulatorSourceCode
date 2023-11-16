using UnityEngine;
using UnityEngine.InputSystem;

public class CursorController : MonoBehaviour
{
    // Variables.
    [Tooltip("The cursor is displayed when the mouse is used.")]
    [SerializeField] private bool _enableCursor;

    [Tooltip("If enabled, the cursor's visibility will not be updated when switching between mouse and keyboard devices.")]
    [SerializeField] private bool _treatMouseAndKeyboardAsSameDevice;

    // A timer to limit the frequency of Cursor.visible updates to avoid performance impact.
    private Timer _cursorVisibleUpdateCooldownTimer;
    private float _cursorVisibleUpdateCooldown = 0.1f;
    private bool _canUpdateCursorVisible;


    // MonoBehaviour.
    private void Awake()
    {
        Cursor.visible = false;

        _cursorVisibleUpdateCooldownTimer = new(() => _canUpdateCursorVisible = true);
        _cursorVisibleUpdateCooldownTimer.StartTimer(0);

        InputSystem.onActionChange += InputSystem_OnActionChange;
    }

    private void Update() => _cursorVisibleUpdateCooldownTimer.UpdateTimer();


    // Event Handlers.
    private void InputSystem_OnActionChange(object obj, InputActionChange inputActionChange)
    {
        if (!_enableCursor) return;

        if (obj is not InputAction inputAction || inputAction.activeControl == null) return;

        if (inputAction.activeControl.device is InputDevice newDevice)
        {
            bool isVisible = _treatMouseAndKeyboardAsSameDevice
                ? newDevice.displayName == "Mouse" || newDevice.displayName == "Keyboard"
                : newDevice.displayName == "Mouse";

            if (_canUpdateCursorVisible)
            {
                Cursor.visible = isVisible;
                _cursorVisibleUpdateCooldownTimer.StartTimer(_cursorVisibleUpdateCooldown);
                _canUpdateCursorVisible = false;
            }
        }
    }
}
