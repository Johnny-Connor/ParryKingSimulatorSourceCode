using UnityEngine;

public class TargetInfoDisplayController : MonoBehaviour
{
    // Components.
    [Header("Components")]
    [SerializeField] private Transform _targetDotLocation;
    private Canvas _canvas;


    // Variables.
    [Header("Variables")]

    [Tooltip("The delay in seconds before display becomes invisible after the character is untargeted or hit while untargeted.")]
    [SerializeField] private float _outOfCombatCountdownTime = 3f;
    public float OutOfCombatCountdownTime => _outOfCombatCountdownTime;
    
    private Timer _untargetedTimer;
    private bool _isBeingTargeted;
    private bool _hasDied;


    // MonoBehaviour.
    private void Awake()
    {
        void SetupVariables()
        {
            _canvas = GetComponent<Canvas>();
            _untargetedTimer = new(() => SetDisplayVisibility(false));
        }

        SetupVariables();

        SetDisplayVisibility(false);
    }

    private void Start()
    {
        void SubscribeToEvents()
        {
            CombatCameraController.OnAnyLookAtTargetUpdated += 
                CharacterCombatStats_OnAnyLookAtTargetUpdated;

            CharacterHitBoxController characterHitBoxBodyController = 
                GetComponentInParent<CharacterHitBoxController>();
            characterHitBoxBodyController.OnHitTaken += HitBoxBodyController_OnHitTaken;

            CharacterStats characterStats = GetComponentInParent<CharacterStats>();
            characterStats.OnDeath += () => _hasDied = true;

            ParriedStabbedState parriedStabbedState = 
                GetComponentInParent<ControlCharacter>().ParriedStabbedState;
            parriedStabbedState.OnHitTaken += ParriedStabbedState_OnHitTaken;
        }

        SubscribeToEvents();
    }

    private void Update()
    {
        if (!_isBeingTargeted) _untargetedTimer.UpdateTimer();
    }

    private void OnDisable() =>
        CombatCameraController.OnAnyLookAtTargetUpdated -= CharacterCombatStats_OnAnyLookAtTargetUpdated;


    // Non-MonoBehaviour.
    private void SetDisplayVisibility(bool isVisible) => _canvas.enabled = isVisible;


    // Event Handlers.
    private void CharacterCombatStats_OnAnyLookAtTargetUpdated(Transform targetedTargetDotLocation)
    {
        if (_targetDotLocation == targetedTargetDotLocation && !_hasDied)
        {
            SetDisplayVisibility(true);
            _isBeingTargeted = true;
        }
        else
        {
            _isBeingTargeted = false;
            _untargetedTimer.StartTimer(_outOfCombatCountdownTime, true);
        }
    }

    private void HitBoxBodyController_OnHitTaken(HitInformation hitInformation)
    {
        SetDisplayVisibility(true);
        if (!_isBeingTargeted) _untargetedTimer.StartTimer(_outOfCombatCountdownTime, true);
    }

    private void ParriedStabbedState_OnHitTaken(int damage)
    {
        SetDisplayVisibility(true);
        if (!_isBeingTargeted) _untargetedTimer.StartTimer(_outOfCombatCountdownTime, true);
    }
}
