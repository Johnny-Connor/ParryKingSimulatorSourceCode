public class StaminaBar : StatBar
{
    // Properties.
    protected override float GetCharacterStatMaxValue() => _characterStats.MaxStamina;
    protected override float GetCharacterStatCurrentValue() => _characterStats.CurrentStamina;
}
