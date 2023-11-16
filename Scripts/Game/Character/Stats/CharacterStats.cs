using System;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    #region Stats Values
    // Stats Values.
    [Header("STATS VALUES")]

    [Header("Health")]

    [Tooltip("Sets the initial maximum health for players. For AI, it represents the inital maximum health at lowest or nonexistent difficulty level.")]
    [SerializeField] private float _maxHealth = 100;
    public float MaxHealth => _maxHealth;

    [SerializeField] private float _currentHealth;
    public float CurrentHealth => _currentHealth;


    [Header("Focus")]

    [Tooltip("Sets the initial maximum focus for players. For AI, it represents the inital maximum focus at lowest or nonexistent difficulty level.")]
    [SerializeField] private float _maxFocus = 50;
    public float MaxFocus => _maxFocus;

    [SerializeField] private float _currentFocus;
    public float CurrentFocus => _currentFocus;


    [Header("Stamina")]
    
    [Tooltip("Sets the initial maximum stamina for players. For AI, it represents the inital maximum stamina at lowest or nonexistent difficulty level.")]
    [SerializeField] private float _maxStamina = 75;
    public float MaxStamina => _maxStamina;

    [SerializeField] private float _currentStamina;
    public float CurrentStamina => _currentStamina;


    [Header("Damage")]

    [Tooltip("Sets the initial damage for players. For AI, it represents the inital damage at lowest or nonexistent difficulty level.")]
    [SerializeField] private int _damage = 20;

    [SerializeField] private int _currentDamage;
    public int CurrentDamage => _currentDamage;

    [Tooltip("Damage multiplier for parry stabbing.")]
    [SerializeField] private float _parryStabMultiplier = 2f;

    public int ParryStabDamage => Mathf.RoundToInt(_currentDamage * _parryStabMultiplier);
    #endregion Stats Values


    #region Stats Parameters
    // Stats Parameters.
    [Header("STATS PARAMETERS")]

    [Header("Stamina")]

    [Tooltip("The cooldown in seconds applied before stamina recovery is enabled again after exhausting stamina.")]
    [SerializeField] private float _staminaRecoveryAfterExhaustionCooldown = 1f;

    // The cooldown in seconds applied before stamina recovery is enabled again after consuming stamina.
    private float StaminaRecoveryCooldown => _staminaRecoveryAfterExhaustionCooldown / 2;

    [Tooltip("The amount of stamina that is recovered in a resting rate state per second.")]
    [SerializeField] private float _staminaRecoveryRateModifier = 20;
    public float StaminaRecoveryRate => _staminaRecoveryRateModifier * Time.deltaTime * -1;

    [Tooltip("The amount of stamina that is consumed in a consumption rate state per second.")]
    [SerializeField] private float _staminaConsumptionRateModifier = 10;
    public float StaminaConsumptionRate => _staminaConsumptionRateModifier * Time.deltaTime;

    [Tooltip("The amount of stamina that is consumed in a consumption value state.")]
    [SerializeField] private float _staminaConsumptionModifier = 20;
    public float StaminaConsumptionValue => _staminaConsumptionModifier;
    #endregion Stats Parameters

    #region AI Parameters
    [Header("AI PARAMETERS")]

    [Header("Dynamic Difficulty Settings (Requires a DifficultyCalculator Instance)")]

    [Tooltip("Starting maximum health at highest difficulty level.")]
    [SerializeField] private float _endingMaxHealth;

    [Tooltip("Starting maximum focus at highest difficulty level.")]
    [SerializeField] private float _endingMaxFocus;

    [Tooltip("Starting maximum stamina at highest difficulty level.")]
    [SerializeField] private float _endingMaxStamina;

    [Tooltip("Starting damage at highest difficulty level.")]
    [SerializeField] private float _endingDamage;
    #endregion AI Parameters


    // Variables.
    private Timer _staminaRecoveryAfterExhaustionTimer;
    private Timer _staminaRecoveryTimer;
    private bool _canRecoverStamina = true;


    // Events.
    public static event Action OnAnyEnemyDeath;
    public event Action OnStatUpdated;
    public event Action OnDeath;
    public event Action OnStaminaExhaustion;


    // MonoBehaviour.
    private void Awake()
    {
        if (!GetComponent<ControlCharacter>().IsPlayer && DifficultyCalculator.Instance)
        {
            static float CalculateStatDifficulty(float maxStat, float endingStat)
            {
                return Mathf.Lerp(maxStat, 
                        endingStat, 
                        DifficultyCalculator.Instance.NormalizedDifficultyLevel);
            }

            _maxHealth = CalculateStatDifficulty(_maxHealth, _endingMaxHealth);
            _maxFocus = CalculateStatDifficulty(_maxFocus, _endingMaxFocus);
            _maxStamina = CalculateStatDifficulty(_maxStamina, _endingMaxStamina);
            _damage = Mathf.RoundToInt(CalculateStatDifficulty(_damage, _endingDamage));
        }

        _currentHealth = _maxHealth;
        _currentFocus = _maxFocus;
        _currentStamina = _maxStamina;
        _currentDamage = _damage;

        _staminaRecoveryAfterExhaustionTimer = new(() => _canRecoverStamina = true);
        _staminaRecoveryTimer = new(() => 
        {
            // Do not enable stamina recovery if _staminaRecoveryAfterExhaustionTimer is running.
            if (_staminaRecoveryAfterExhaustionTimer.TimeLeft == 0) _canRecoverStamina = true;
        });

        GetComponent<CharacterHitBoxController>().OnHitTaken += HitBoxBodyController_OnHitTaken;
    }

    private void Start()
    {
        ParriedStabbedState parriedStabbedState = GetComponent<ControlCharacter>().ParriedStabbedState;
        parriedStabbedState.OnHitTaken += ParriedStabbedState_OnHitTaken;
    }

    private void Update()
    {
        _staminaRecoveryAfterExhaustionTimer.UpdateTimer();
        _staminaRecoveryTimer.UpdateTimer();
    }


    // Non-MonoBehaviour.
    private void UpdateCurrentHealth(float healthModifier)
    {
        float newCurrentHealth = Mathf.Clamp(_currentHealth - healthModifier, 0, _maxHealth);

        if (newCurrentHealth == _currentHealth) return;

        _currentHealth = newCurrentHealth;
        OnStatUpdated?.Invoke();

        if (_currentHealth == 0)
        {
            OnDeath?.Invoke();

            if (!GetComponent<ControlCharacter>().IsPlayer) OnAnyEnemyDeath?.Invoke();
        }
    }

    public void UpdateCurrentStamina(float staminaModifier)
    {
        if (staminaModifier == 0)
        {
            Debug.LogWarning("An attempt to update stamina with a modifier of 0 was canceled.");
            return;
        }

        // Calculates new current stamina.
        float newCurrentStamina = Mathf.Clamp(_currentStamina - staminaModifier, 0, _maxStamina);
        // Checks if the character is trying to recover stamina.
        bool isTryingToRecoverStamina = _currentStamina < newCurrentStamina;

        // If the value has not changed (due to clamp), no further action is needed.
        if (_currentStamina == newCurrentStamina) return;
        // If trying to recover stamina and not allowed to, no further action is needed.
        if (isTryingToRecoverStamina && !_canRecoverStamina) return;

        // If trying to consume stamina.
        if (!isTryingToRecoverStamina)
        {
            if (newCurrentStamina == 0)
            {
                // If stamina reached 0, trigger stamina exhaustion event and timer.
                OnStaminaExhaustion?.Invoke();
                _staminaRecoveryAfterExhaustionTimer.StartTimer(_staminaRecoveryAfterExhaustionCooldown);
            }
            else
            {
                // Otherwise, restart (override) stamina recovery timer.
                _staminaRecoveryTimer.StartTimer(StaminaRecoveryCooldown, true);
            }

            _canRecoverStamina = false;
        }

        // Update stamina value and trigger the event to notify of its update.
        _currentStamina = newCurrentStamina;
        OnStatUpdated?.Invoke();
    }


    // Event Handlers.
    private void HitBoxBodyController_OnHitTaken(HitInformation hitInformation) =>
        UpdateCurrentHealth(hitInformation.hitDamage);

    private void ParriedStabbedState_OnHitTaken(int hitDamage) => UpdateCurrentHealth(hitDamage);
}
