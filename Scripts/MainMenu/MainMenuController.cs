using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    #region Variables
    // Variables.
    [Header("Modal PopUp Screen Background")]
    [SerializeField] private Image _modalPopUpScreenBackground;
    public Image ModalPopUpScreenBackground => _modalPopUpScreenBackground;

    [Header("Main Screens")]
    [SerializeField] private GameObject _startScreen;
    public GameObject StartScreen => _startScreen;
    [SerializeField] private GameObject _menuScreen;
    public GameObject MenuScreen => _menuScreen;
    [SerializeField] private GameObject _optionsScreen;
    public GameObject OptionsScreen => _optionsScreen;
    [SerializeField] private GameObject _creditsScreen;
    public GameObject CreditsScreen => _creditsScreen;
    
    [Header("Main Selectables")]
    [SerializeField] private Selectable[] _startSelectablesList;
    public Selectable[] StartSelectablesList => _startSelectablesList;
    [SerializeField] private Selectable[] _menuSelectablesList;
    public Selectable[] MenuSelectablesList => _menuSelectablesList;
    [SerializeField] private Selectable[] _optionsSelectablesList;
    public Selectable[] OptionsSelectablesList => _optionsSelectablesList;
    [SerializeField] private Selectable[] _creditsSelectablesList;
    public Selectable[] CreditsSelectablesList => _creditsSelectablesList;

    [Header("Pop Up Screens")]
    [SerializeField] private GameObject _quitConfirmationScreen;
    public GameObject QuitConfirmationScreen => _quitConfirmationScreen;

    [Header("Pop Up Selectables")]
    [SerializeField] private Selectable[] _exitSelectablesList;
    public Selectable[] ExitSelectablesList => _exitSelectablesList;

    private MainMenuStateMachine _mainMenuStateMachine = new();
    public MainMenuStateMachine MainMenuStateMachine => _mainMenuStateMachine;
    
    private PlayerInputActions _playerInputActions;
    
    #region State Variables
    // Main States.
    private StartState _startState;
    public StartState StartState => _startState;

    private MenuState _menuState;
    public MenuState MenuState => _menuState;

    private OptionsState _optionsState;
    public OptionsState OptionsState => _optionsState;

    private CreditsState _creditsState;
    public CreditsState CreditsState => _creditsState;

    // Pop Up States.
    private QuitConfirmationState _quitConfirmationState;
    public QuitConfirmationState QuitConfirmationState => _quitConfirmationState;
    #endregion State Variables
    #endregion Variables


    #region MonoBehaviour
    // MonoBehaviour.
    private void Awake()
    {
        void SetupStateMachine()
        {
            // Menu States.
            _startState = new StartState(this, _mainMenuStateMachine);
            _menuState = new MenuState(this, _mainMenuStateMachine);
            _optionsState = new OptionsState(this, _mainMenuStateMachine);
            _creditsState = new CreditsState(this, _mainMenuStateMachine);

            // Pop Up States.
            _quitConfirmationState = new QuitConfirmationState(this, _mainMenuStateMachine);
        }

        void SubscribeToEvents()
        {
            _playerInputActions.Menu.AnyKey.performed += Menu_OnAnyKeyPerformed;
            _playerInputActions.Menu.Back.performed += Menu_OnBackPerformed;
            _playerInputActions.Menu.Confirm.performed += Menu_OnConfirmPerformed;
            _playerInputActions.Menu.Move.performed += Menu_OnMovePerformed;
        
            _mainMenuStateMachine.OnEnteredState += MenuStateMachine_OnEnteredState;

            /*
            Ensures that the last input from a previous state is not carried over to the next state. For 
            example, it prevents an arrow key pressed in the StartState from causing unintended movement
            in the MenuState.
            */
            _mainMenuStateMachine.OnExitedState += MenuStateMachine_OnExitedState;
        }

        SetupStateMachine();

        _playerInputActions = new();
        _playerInputActions.Menu.Enable();

        SubscribeToEvents();
    }

    private void Start() => _mainMenuStateMachine.Initialize(_startState);

    private void OnDisable()
    {
        _playerInputActions.Menu.Disable();

        /*
        Unsubscribes from input asset events, preventing it from attempting to control the state machine
        across scenes when enabled again.
        */
        _playerInputActions.Menu.AnyKey.performed -= Menu_OnAnyKeyPerformed;
        _playerInputActions.Menu.Back.performed -= Menu_OnBackPerformed;
        _playerInputActions.Menu.Confirm.performed -= Menu_OnConfirmPerformed;
        _playerInputActions.Menu.Move.performed -= Menu_OnMovePerformed;

        _mainMenuStateMachine.OnEnteredState -= MenuStateMachine_OnEnteredState;
        _mainMenuStateMachine.OnExitedState -= MenuStateMachine_OnExitedState;
    }
    #endregion MonoBehaviour


    #region Non-MonoBehaviour
    // Non-MonoBehaviour.
    private void UpdateMenuActionMap(bool enable)
    {
        if (enable) _playerInputActions.Menu.Enable();
        else _playerInputActions.Menu.Disable();
    }
    #endregion Non-MonoBehaviour


    #region Event Handlers
    // Event Handlers.
    private void Menu_OnAnyKeyPerformed(InputAction.CallbackContext context) 
        => _mainMenuStateMachine.DoAnyKey();

    private void Menu_OnBackPerformed(InputAction.CallbackContext context) 
        => _mainMenuStateMachine.DoBack();

    private void Menu_OnConfirmPerformed(InputAction.CallbackContext context) 
        => _mainMenuStateMachine.DoConfirm();

    private void Menu_OnMovePerformed(InputAction.CallbackContext context)
        => _mainMenuStateMachine.DoMove(_playerInputActions.Menu.Move.ReadValue<Vector2>());

    private void MenuStateMachine_OnEnteredState(object sender, EventArgs e) => UpdateMenuActionMap(true);

    private void MenuStateMachine_OnExitedState(object sender, EventArgs e) => UpdateMenuActionMap(false);
    #endregion Event Handlers
}
