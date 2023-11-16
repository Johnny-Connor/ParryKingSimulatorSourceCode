using System;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshController : MonoBehaviour
{
    #region Variables
    // Components.
    private CharacterCombatStats _characterCombatStats;
    private NavMeshAgent _navMeshAgent;
    private NavMeshObstacle _navMeshObstacle;

    // By Value Variables.
    [Header("NavMesh Switching Parameters")]

    [Tooltip("Enables AI to switch between its NavMeshAgent and NavMeshObstacle components for local avoidance. Transitions occasionally results in jittery movements.")]
    [SerializeField] private bool _enableNavMeshSwitching = false;

    [Tooltip("The time delay in seconds for transitioning between NavMeshAgent and NavMeshObstacle modes. Reduces the frequency of transitions.")]
    [SerializeField] private float _navMeshSwitchTime = 0.1f;

    [Header("Distance Parameters")]

    [Tooltip("The distance in meters for the AI to be considered far from its target at lowest or nonexistent difficulty level.")]
    [SerializeField] private float _startingFarDistance = 6;

    [Tooltip("The distance in meters for the AI to be considered far from its target at highest difficulty level.")]
    [SerializeField] private float _endingFarDistance = 4;

    [Tooltip("Random margin added to the far distance.")]
    [SerializeField] private float _farDistanceRandomMargin = 1;

    [Tooltip("Additional offset distance in meters added to AI radius for reach distance calculations.")]
    [SerializeField] private float _reachDistanceOffset = 0.05f;

    private Timer _transitionToNavMeshAgentTimer;
    private Timer _transitionToNavMeshObstacleTimer;
    private float _farDistance;
    private bool _hasDied;

    #region Properties
    // Properties.
    public float HorizontalVelocityMagnitude
    {
        get
        {
            Vector2 velocityXZ = new(
                _navMeshAgent.velocity.x, 
                _navMeshAgent.velocity.z
            );

            return velocityXZ.magnitude;
        }
    }

    public Vector2 HorizontalDirectionLocal => transform.InverseTransformDirection(Vector2.up).normalized;

    public bool HasReachedTarget
    {
        get
        {
            if (_characterCombatStats.CurrentTargetTransform)
            {
                return (_characterCombatStats.CurrentTargetTransform.position - transform.position).magnitude <= 
                    _navMeshAgent.stoppingDistance + _navMeshAgent.radius + _reachDistanceOffset;
            }
            else
            {
                return false;
            }
        }
    }

    public bool IsFarFromTarget
    {
        get
        {
            if (_characterCombatStats.CurrentTargetTransform && !HasReachedTarget)
            {
                return (_characterCombatStats.CurrentTargetTransform.position - transform.position).magnitude >= 
                    _farDistance + _navMeshAgent.radius + _reachDistanceOffset;
            }
            else
            {
                return false;
            }
        }
    }
    #endregion Properties
    #endregion Variables


    // MonoBehaviour.
    private void Awake()
    {
        _characterCombatStats = GetComponent<CharacterCombatStats>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshObstacle = GetComponent<NavMeshObstacle>();

        if (_enableNavMeshSwitching)
        {
            _transitionToNavMeshAgentTimer = new(SwitchToNavMeshAgent);
            _transitionToNavMeshAgentTimer.StartTimer(_navMeshSwitchTime);

            _transitionToNavMeshObstacleTimer = new(SwitchToNavMeshObstacle);
            _transitionToNavMeshObstacleTimer.StartTimer(_navMeshSwitchTime);
        }
        else
        {
            SwitchToNavMeshAgent();
        }

        // Calculates far distance.
        if (DifficultyCalculator.Instance)
        {
            float randomFarDistanceMargin = 
                UnityEngine.Random.Range(-_farDistanceRandomMargin, _farDistanceRandomMargin);
                
            _farDistance = Mathf.Lerp(
                                _startingFarDistance,
                                _endingFarDistance,
                                DifficultyCalculator.Instance.NormalizedDifficultyLevel) +
                            randomFarDistanceMargin;
        }
        else
        {
            _farDistance = _startingFarDistance;
        }
    }

    private void Start()
    {
        CharacterStateMachine characterStateMachine = 
            GetComponent<ControlCharacter>().CharacterStateMachine;
        characterStateMachine.OnEnteredState += CharacterStateMachine_OnEnteredState;

        CharacterStats characterStats = GetComponent<CharacterStats>();
        characterStats.OnDeath += CharacterStats_OnDeath;
    }

    private void Update()
    {
        if (_hasDied) return;

        // Calculates path.
        if (_navMeshAgent.enabled && _characterCombatStats.CurrentTargetTransform)
        {
            _navMeshAgent.destination = _characterCombatStats.CurrentTargetTransform.position;
        }

        if (!_enableNavMeshSwitching) return;

        if (HasReachedTarget && _navMeshAgent.enabled)
        {
            _transitionToNavMeshObstacleTimer.UpdateTimer();

            _transitionToNavMeshAgentTimer.StartTimer(_navMeshSwitchTime, true);
        }
        else if (!HasReachedTarget && _navMeshObstacle.enabled)
        {
            _transitionToNavMeshAgentTimer.UpdateTimer();

            _transitionToNavMeshObstacleTimer.StartTimer(_navMeshSwitchTime, true);
        }
    }


    // Non-MonoBehaviour.

    /*
    SwitchToNavMesh methods: A workaround to improve the behavior of NavMeshAgents. To prevent NavMeshAgents 
    from blocking each other's paths, agents that are stopped are changed into NavMeshObstacles, allowing 
    other agents to navigate around them.
    */
    private void SwitchToNavMeshAgent()
    {
        _navMeshObstacle.enabled = false;
        _navMeshAgent.enabled = true;

        if (_enableNavMeshSwitching) _transitionToNavMeshAgentTimer.StartTimer(_navMeshSwitchTime);
    }

    private void SwitchToNavMeshObstacle()
    {
        _navMeshAgent.enabled = false;
        _navMeshObstacle.enabled = true;

        if (_enableNavMeshSwitching) _transitionToNavMeshObstacleTimer.StartTimer(_navMeshSwitchTime);
    }

    private void DisableNavMeshes()
    {
        _navMeshAgent.enabled = false;
        _navMeshObstacle.enabled = false;
    }


    // Event Handlers.
    private void CharacterStateMachine_OnEnteredState(object sender, EventArgs e)
    {
        switch (sender.GetType().Name)
        {
            case nameof(IdleState):
                _navMeshAgent.speed = 0;
                break;
            case nameof(WalkState):
                _navMeshAgent.speed = GetComponent<CharacterMovementStats>().WalkSpeed;
                break;
            case nameof(SprintState):
                _navMeshAgent.speed = GetComponent<CharacterMovementStats>().SprintSpeed;
                break;
        }
    }

    private void CharacterStats_OnDeath()
    {
        // Stops blocking others path.
        DisableNavMeshes();
        
        _hasDied = true;
    }
}
