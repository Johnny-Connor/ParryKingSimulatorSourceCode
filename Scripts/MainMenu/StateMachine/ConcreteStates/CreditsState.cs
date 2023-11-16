using UnityEngine.UI;

public class CreditsState : BaseMenuState
{
    // Properties.
    public override Selectable[] SelectablesList => _menuController.CreditsSelectablesList;
    public override bool IsPopUpScreen => false;
    public override bool IsNavigationHorizontal => false;


    // Constructor.
    public CreditsState(
        MainMenuController menuController, 
        IMainMenuStateMachineContext menuStateMachineContext) 
    : base(menuController, menuStateMachineContext)
    {
    }


    // State Methods.
    public override void OnBackPerformed() => _menuStateMachineContext.SetState(_menuController.MenuState);
}
