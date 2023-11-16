public class HealthBar : StatBar
{
    // Properties.
    protected override float GetCharacterStatMaxValue() => _characterStats.MaxHealth;
    protected override float GetCharacterStatCurrentValue() => _characterStats.CurrentHealth;
}
