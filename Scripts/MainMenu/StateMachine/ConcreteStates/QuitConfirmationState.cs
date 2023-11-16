using UnityEngine.UI;

public class QuitConfirmationState : BaseMenuState
{
    // Properties.
    public override Selectable[] SelectablesList => _menuController.ExitSelectablesList;
    public override bool IsNavigationHorizontal => true;
    public override bool IsPopUpScreen => true;


    // Constructor.
    public QuitConfirmationState(
        MainMenuController menuController,
        IMainMenuStateMachineContext menuStateMachineContext)
    : base(menuController, menuStateMachineContext)
    {
    }


    // State Methods.
    public override void OnBackPerformed() => 
        _menuStateMachineContext.SetState(_menuController.MenuState);
}
