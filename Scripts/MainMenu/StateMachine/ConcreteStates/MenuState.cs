using UnityEngine.UI;

public class MenuState : BaseMenuState
{
    // Properties.
    public override Selectable[] SelectablesList => _menuController.MenuSelectablesList;
    public override bool IsNavigationHorizontal => false;
    public override bool IsPopUpScreen => false;


    // Constructor.
    public MenuState(
        MainMenuController menuController, 
        IMainMenuStateMachineContext menuStateMachineContext)
    : base(menuController, menuStateMachineContext)
    {
    }


    // State Methods.
    public override void OnBackPerformed() => 
        _menuStateMachineContext.SetState(_menuController.QuitConfirmationState);
}
