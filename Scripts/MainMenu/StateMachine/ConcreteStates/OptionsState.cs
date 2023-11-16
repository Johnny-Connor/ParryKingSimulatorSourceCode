using UnityEngine.UI;

public class OptionsState : BaseMenuState
{
    // Properties.
    public override Selectable[] SelectablesList => _menuController.OptionsSelectablesList;
    public override bool IsPopUpScreen => false;
    public override bool IsNavigationHorizontal => false;


    // Constructor.
    public OptionsState(
        MainMenuController menuController, 
        IMainMenuStateMachineContext menuStateMachineContext)
    : base(menuController, menuStateMachineContext)
    {
    }

    
    // State Methods.
    public override void OnBackPerformed() => _menuStateMachineContext.SetState(_menuController.MenuState);
}
