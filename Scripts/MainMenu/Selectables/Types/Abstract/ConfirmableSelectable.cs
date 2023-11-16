public abstract class ConfirmableSelectable : MainMenuSelectable
{
    // Non-MonoBehaviour.
    public void DoConfirm()
    {
        Confirm();
        RaiseOnInteractedEvent();
    }

    public abstract void Confirm();
}
