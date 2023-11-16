public class FocusBar : StatBar
{
    // Properties.
    protected override float GetCharacterStatMaxValue() => _characterStats.MaxFocus;
    protected override float GetCharacterStatCurrentValue() => _characterStats.CurrentFocus;
}
