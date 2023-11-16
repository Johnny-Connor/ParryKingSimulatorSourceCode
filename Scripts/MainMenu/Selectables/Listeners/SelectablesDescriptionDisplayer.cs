using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectablesDescriptionDisplayer : MonoBehaviour
{
    // Variables.
    private TMP_Text _descriptionText;


    // MonoBehaviour.
    private void Awake()
    {
        _descriptionText = GetComponent<TMP_Text>();
        
        MainMenuController mainMenuController = GetComponentInParent<MainMenuController>();
        MainMenuStateMachine mainMenuStateMachine = mainMenuController.MainMenuStateMachine;

        mainMenuStateMachine.OnEnteredState += MainMenuStateMachine_OnEnteredState;
        mainMenuStateMachine.OnExitedState += MainMenuStateMachine_OnExitedState;
    }


    // Event Handlers.
    private void MainMenuStateMachine_OnEnteredState(object sender, EventArgs e)
    {
        BaseMenuState baseMenuState = sender as BaseMenuState;

        baseMenuState.OnSelectableChanged += BaseMenuState_OnSelectableChanged;
    }

    private void MainMenuStateMachine_OnExitedState(object sender, EventArgs e)
    {
        BaseMenuState baseMenuState = sender as BaseMenuState;

        baseMenuState.OnSelectableChanged -= BaseMenuState_OnSelectableChanged;
    }

    private void BaseMenuState_OnSelectableChanged(Selectable selectable)
    {
        if (selectable.TryGetComponent(
            out MainMenuSelectableDescription mainMenuSelectableDescriptionComponent))
        {
            _descriptionText.text = mainMenuSelectableDescriptionComponent.Description;
        }
        else
        {
            _descriptionText.text = "Description not available.";
        }        
    }
}
