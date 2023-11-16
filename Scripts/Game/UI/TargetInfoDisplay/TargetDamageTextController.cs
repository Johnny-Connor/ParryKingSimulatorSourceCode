using TMPro;
using UnityEngine;

public class TargetDamageTextController : MonoBehaviour
{
    // Variables.
    private TMP_Text _damageText;
    private TargetInfoDisplayController _targetInfoDisplayController;
    private Timer _undamagedTimer;
    private int _accumulatedDamage;


    // Properties.
    
    // The delay in seconds before damage text becomes invisible after the character is hit.
    private float UndamagedCountdownTime => _targetInfoDisplayController.OutOfCombatCountdownTime / 2;


    // MonoBehaviour.
    private void Awake()
    {
        void SetupVariables()
        {
            _damageText = GetComponent<TMP_Text>();
            _undamagedTimer = new(() => DisableDamageTextVisibility());
        }

        SetupVariables();

        DisableDamageTextVisibility();
    }

    private void Start()
    {
        void SubscribeToEvents()
        {
            CharacterHitBoxController characterHitBoxBodyController = 
                GetComponentInParent<CharacterHitBoxController>();
            characterHitBoxBodyController.OnHitTaken += HitBoxBodyController_OnHitTaken;

            ParriedStabbedState parriedStabbedState = 
                GetComponentInParent<ControlCharacter>().ParriedStabbedState;
            parriedStabbedState.OnHitTaken += ParriedStabbedState_OnHitTaken;
        }

        _targetInfoDisplayController = GetComponentInParent<TargetInfoDisplayController>();

        SubscribeToEvents();
    }

    private void Update() => _undamagedTimer.UpdateTimer();


    // Non-MonoBehaviour.
    private void UpdateDamageText(int damage)
    {
        _accumulatedDamage += damage;
        _damageText.text = _accumulatedDamage.ToString();

        _undamagedTimer.StartTimer(UndamagedCountdownTime, true);
    }

    private void DisableDamageTextVisibility()
    {
        _damageText.text = string.Empty;
        _accumulatedDamage = 0;
    }


    // Event Handlers.
    private void HitBoxBodyController_OnHitTaken(HitInformation hitInformation) => 
        UpdateDamageText(hitInformation.hitDamage);
        
    private void ParriedStabbedState_OnHitTaken(int damage) => UpdateDamageText(damage);
}
