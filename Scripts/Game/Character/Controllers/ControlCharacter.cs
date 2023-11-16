using System;
using UnityEngine;

public class ControlCharacter : MonoBehaviour
{
    #region Variables
    // By Value Variables.
    [SerializeField] private bool _isPlayer;
    public bool IsPlayer => _isPlayer;

    [SerializeField] private bool _enableAttack;
    public bool EnableAttack => _enableAttack;

    // State Machine.
    private CharacterStateMachine _characterStateMachine;
    public CharacterStateMachine CharacterStateMachine => _characterStateMachine;
    
    #region Available States
    // Available States.
    private AttackState _attackState;
    public AttackState AttackState => _attackState;

    private BackStepState _backStepState;
    public BackStepState BackStepState => _backStepState;

    private DeathState _deathState;
    public DeathState DeathState => _deathState;

    private FallState _fallState;
    public FallState FallState => _fallState;

    private HitState _hitState;
    public HitState HitState => _hitState;

    private IdleState _idleState;
    public IdleState IdleState => _idleState;

    private JumpState _jumpState;
    public JumpState JumpState => _jumpState;

    private ParriedStabbedState _parriedStabbedState;
    public ParriedStabbedState ParriedStabbedState => _parriedStabbedState;

    private ParriedState _parriedState;
    public ParriedState ParriedState => _parriedState;

    private ParryStabState _parryStabState;
    public ParryStabState ParryStabState => _parryStabState;

    private ParryState _parryState;
    public ParryState ParryState => _parryState;

    private RollState _rollState;
    public RollState RollState => _rollState;

    private SprintState _sprintState;
    public SprintState SprintState => _sprintState;

    private WalkState _walkState;
    public WalkState WalkState => _walkState;
    #endregion Available States

    // Player Exclusive.
    [Tooltip("The camera following the character.")]
    [SerializeField] private Transform _camera;
    public Transform Camera => _camera;

    [SerializeField] private DeathScreenAnimatorController _deathScreenAnimatorController;

    private PlayerInputActions _playerInputActions;
    public PlayerInputActions PlayerInputActions => _playerInputActions;

    private PlayerCharacterInputActionsProcessor _playerInputActionsProcessor;
    public PlayerCharacterInputActionsProcessor PlayerInputActionsProcessor => 
        _playerInputActionsProcessor;

    // AI Exclusive.
    private NavMeshController _navMeshController;
    public NavMeshController NavMeshController => _navMeshController;
    #endregion Variables

    // Events.
    public event Action OnDestroyed;


    // MonoBehaviour.
    private void Awake()
    {
        SetupVariables();
        SetupStateMachine();

        CharacterVFXController characterVFXController = GetComponent<CharacterVFXController>();
        characterVFXController.OnCharacterFadeOutAnimationAndParticlesCompleted += 
            CharacterVFXController_OnCharacterFadeOutAnimationAndParticlesCompleted;
    }

    private void Start()
    {
        /*
        Initializing the state machine with an initial state.
        This is placed in Start to allow observers to subscribe before Initialize's OnEnteredState event
        fires.
        */
        _characterStateMachine.Initialize(_idleState);
    }

    private void Update() => _characterStateMachine.DoTicks();


    // Non-MonoBehaviour.
    private void SetupVariables()
    {
        if (_isPlayer)
        {
            _playerInputActionsProcessor = GetComponent<PlayerCharacterInputActionsProcessor>();

            // Creating and assigning an instance of PlayerInputActions.
            _playerInputActions = new PlayerInputActions();
            // Enabling Input Actions.
            _playerInputActions.Game.Enable();
        }
        else
        {
            _navMeshController = GetComponent<NavMeshController>();
        }
    }

    private void SetupStateMachine()
    {
        // Creating and assigning an instance of CharacterStateMachine.
        _characterStateMachine = new CharacterStateMachine();
        
        // Instantiating the states that this controller can use.
        _attackState = new AttackState(this, _characterStateMachine);
        _backStepState = new BackStepState(this, _characterStateMachine);
        _deathState = new DeathState(this, _characterStateMachine);
        _fallState = new FallState(this, _characterStateMachine);
        _hitState = new HitState(this, _characterStateMachine);
        _idleState = new IdleState(this, _characterStateMachine);
        _jumpState = new JumpState(this, _characterStateMachine);
        _parriedStabbedState = new ParriedStabbedState(this, _characterStateMachine);
        _parriedState = new ParriedState(this, _characterStateMachine);
        _parryState = new ParryState(this, _characterStateMachine);
        _parryStabState = new ParryStabState(this, _characterStateMachine);
        _rollState = new RollState(this, _characterStateMachine);
        _sprintState = new SprintState(this, _characterStateMachine);
        _walkState = new WalkState(this, _characterStateMachine);
    }


    // Event Handlers.
    private void CharacterVFXController_OnCharacterFadeOutAnimationAndParticlesCompleted()
    {
        Destroy(gameObject);
        OnDestroyed?.Invoke();
    }
}
