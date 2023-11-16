using System;
using UnityEngine;

public class CombatDifficultyController : MonoBehaviour
{
    // Variables.
    [Header("Components")]
    [Tooltip("The attack delay timer updates based on some conditions, including the AI's distance from its target.")]
    private NavMeshController _navMeshController;


    [Header("Individual Settings")]

    [Tooltip("Delay before executing an attack at lowest or nonexistent difficulty level, in seconds.")]
    [SerializeField] private float _startingAttackDelay = 0;

    [Tooltip("Enable randomizing attack delay between 0 and current attack delay value.")]
    [SerializeField] private bool _isAttackDelayRandomized = false;

    [Tooltip("Determines if attack delay is reset when player moves away from attack range.")]
    [SerializeField] private bool _resetAttackDelayWhenOutOfAttackRange;


    [Header("Dynamic Difficulty Settings (Requires a DifficultyCalculator Instance)")]

    [Tooltip("Delay before executing an attack at highest difficulty level, in seconds.")]
    [SerializeField] private float _endingAttackDelay = 3;

    [Tooltip("Normalized difficulty level at which randomizing attack delay is enabled.")]
    [SerializeField] [Range(0, 1)] private float _attackDelayRandomizationThreshold = 0.8f;

    [Tooltip("Use locked normalized difficulty level, preventing behaviour changes when difficulty level updates during lifetime.")]
    [SerializeField] private bool _lockNormalizedDifficultyDuringLifetime;


    // Non-SerializeField variables.
    private Timer _attackDelayTimer;
    private bool _isInAGroundMotionState;
    private float _lockedNormalizedDifficultyLevel = -1;


    // Properties.
    private float NormalizedDifficultyLevel
    {
        get
        {
            if (_lockNormalizedDifficultyDuringLifetime)
            {
                if (_lockedNormalizedDifficultyLevel == -1) 
                {
                    string warningMessage = "_lockedNormalizedDifficultyLevel was accessed without being ";
                        warningMessage += "initialized! Returning it as -1.";
                    Debug.LogWarning(warningMessage);
                }

                return _lockedNormalizedDifficultyLevel;
            }
            else
            {
                return DifficultyCalculator.Instance.NormalizedDifficultyLevel;
            }
        }
    }                                                

    private float AttackDelay
    {
        get
        {
            if (DifficultyCalculator.Instance)
            {
                return Mathf.Lerp(
                        _startingAttackDelay,
                        _endingAttackDelay,
                        NormalizedDifficultyLevel);
            }
            else
            {
                return _startingAttackDelay;
            }
        }
    }

    private float GetNewAttackDelayValue
    {
        get
        {
            if (DifficultyCalculator.Instance)
            {
                return IsAttackDelayRandomized ? UnityEngine.Random.Range(0, AttackDelay) : AttackDelay;
            }
            else
            {
                return IsAttackDelayRandomized ? UnityEngine.Random.Range(0, _startingAttackDelay) : 
                                                _startingAttackDelay;
            }
        }
    }

    private bool IsAttackDelayRandomized
    {
        get
        {
            if (DifficultyCalculator.Instance)
            {
                _isAttackDelayRandomized = NormalizedDifficultyLevel >= 
                                            _attackDelayRandomizationThreshold;
                return _isAttackDelayRandomized;
            }
            else
            {
                return _isAttackDelayRandomized;
            }
        }
    }


    // Events
    public event Action OnAttackDelayTimerEnded;


    // MonoBehaviour.
    private void Awake()
    {
        _navMeshController = GetComponent<NavMeshController>();

        if (DifficultyCalculator.Instance)
            _lockedNormalizedDifficultyLevel = DifficultyCalculator.Instance.NormalizedDifficultyLevel;

        _attackDelayTimer = new(AttackDelayTimerCallback);
        _attackDelayTimer.StartTimer(GetNewAttackDelayValue);
    }

    private void Start()
    {
        CharacterStateMachine characterStateMachine = 
            GetComponent<ControlCharacter>().CharacterStateMachine;
        characterStateMachine.OnEnteredState += CharacterStateMachine_OnEnteredState;
    }

    private void Update()
    {
        if (_navMeshController.HasReachedTarget && _isInAGroundMotionState)
        {
            _attackDelayTimer.UpdateTimer();
        }
        else if (!_navMeshController.HasReachedTarget && _resetAttackDelayWhenOutOfAttackRange)
        {
            _attackDelayTimer.StartTimer(GetNewAttackDelayValue, true);
        }
    }


    // Event Handlers.
    private void AttackDelayTimerCallback()
    {
        OnAttackDelayTimerEnded?.Invoke();
        _attackDelayTimer.StartTimer(GetNewAttackDelayValue);
    }

    private void CharacterStateMachine_OnEnteredState(object sender, EventArgs e) =>
        _isInAGroundMotionState = sender is BaseCharacterGroundMotionState;
}
