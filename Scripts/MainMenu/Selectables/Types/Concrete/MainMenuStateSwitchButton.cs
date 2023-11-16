using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MainMenuStateSwitchButton : ConfirmableSelectable
{
    // Variables.
    private IMainMenuState _currentState;

    private enum EnumState
    {
        StartState,
        MenuState,
        OptionsState,
        CreditsState,
        QuitConfirmationState,
    }

    [SerializeField] private EnumState _goesToState;
    private IMainMenuState GoesToState => _enumStateToMainMenuStateDictionary[_goesToState];

    private Dictionary<EnumState, IMainMenuState> _enumStateToMainMenuStateDictionary;


    // MonoBehaviour.
    private void Awake()
    {
        MainMenuController mainMenuController = GetComponentInParent<MainMenuController>();

        MainMenuStateMachine mainMenuStateMachine = mainMenuController.MainMenuStateMachine;
        mainMenuStateMachine.OnEnteredState += 
            (object sender, EventArgs e) => _currentState = sender as IMainMenuState;
    }

    private void Start()
    {
        MainMenuController mainMenuController = GetComponentInParent<MainMenuController>();

        _enumStateToMainMenuStateDictionary = new()
        {
            [EnumState.StartState] = mainMenuController.StartState,
            [EnumState.MenuState] = mainMenuController.MenuState,
            [EnumState.OptionsState] = mainMenuController.OptionsState,
            [EnumState.CreditsState] = mainMenuController.CreditsState,
            [EnumState.QuitConfirmationState] = mainMenuController.QuitConfirmationState,
        };
    }


    // Non-MonoBehaviour.
    public override void Confirm() => _currentState.GoToState(GoesToState);
}
