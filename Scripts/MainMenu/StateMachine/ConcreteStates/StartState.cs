using UnityEngine.UI;

public class StartState : BaseMenuState
{
    // Properties.
    public override Selectable[] SelectablesList => _menuController.StartSelectablesList;
    public override bool IsPopUpScreen => false;
    public override bool IsNavigationHorizontal => true;


    // Constructor.
    public StartState(
        MainMenuController menuController,
        IMainMenuStateMachineContext menuStateMachineContext)
    : base(menuController, menuStateMachineContext)
    {
    }


    // State Methods.
    public override void OnAnyKeyPerformed() => OnConfirmPerformed();

    public override void OnBackPerformed() { /* Do nothing. */ }
}
