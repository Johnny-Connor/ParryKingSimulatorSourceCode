using UnityEngine;
using UnityEngine.UI;

public abstract class StatBar : MonoBehaviour
{
    // Components.
    [Header("Components")]
    [SerializeField] protected CharacterStats _characterStats;


    // Sliders.
    [Header("Sliders")]
    [SerializeField] private Slider _statSlider;
    [SerializeField] private Slider _statTakenHighlightSlider;


    // Parameters.
    [Header("Parameters")]

    [Tooltip("Stat taken values below this margin will not trigger the 'Stat Taken Highlight' effect.")]
    [SerializeField] private float _statTakenThresholdForTakenHighlightEffect = 1f;

    [Tooltip("The delay in seconds before the 'Stat Taken Highlight Sync' effect starts after losing a portion of the stat.")]
    [SerializeField] private float _statTakenHighlightSyncEffectDelayTime = 0.5f;
    
    [Tooltip("The speed at which the '_statTakenHighlightSlider' moves towards the '_statSlider' value during the 'Stat Taken Highlight Sync' effect.")]
    [SerializeField] private float _statTakenHighlightFillEffectSpeed = 40;


    // Variables.
    private Timer _statTakenHighlightStatSliderDelayEffectTimer;
    private bool _isStatTakenHightlightSyncEffectOn;


    // Properties.
    protected abstract float GetCharacterStatMaxValue();
    protected abstract float GetCharacterStatCurrentValue();


    // MonoBehaviour.
    protected virtual void Awake()
    {
        _statTakenHighlightStatSliderDelayEffectTimer = 
            new(() => _isStatTakenHightlightSyncEffectOn = true);
    }

    protected virtual void Start()
    {
        void SetupSlidersValues(float value, float maxValue)
        {
            _statSlider.maxValue = maxValue;
            _statTakenHighlightSlider.maxValue = maxValue;

            _statSlider.value = value;
            _statTakenHighlightSlider.value = value;
        }

        SetupSlidersValues(GetCharacterStatCurrentValue(), GetCharacterStatMaxValue());

        _characterStats.OnStatUpdated += CharacterStats_OnStatUpdated;
    }

    protected virtual void Update()
    {
        void HandleStatTakenHightlightSyncEffect()
        {
            if (_isStatTakenHightlightSyncEffectOn)
            {
                // Calculate the direction of movement
                float direction = Mathf.Sign(_statSlider.value - _statTakenHighlightSlider.value);

                // Calculate the amount to move based on the speed and time
                float moveAmount = _statTakenHighlightFillEffectSpeed * Time.deltaTime * direction;

                // Update the highlight slider's value
                _statTakenHighlightSlider.value += moveAmount;

                // Check if the movement direction needs to be changed (reached the destination)
                if (Mathf.Sign(_statSlider.value - _statTakenHighlightSlider.value) != direction)
                {
                    _statTakenHighlightSlider.value = _statSlider.value;
                    _isStatTakenHightlightSyncEffectOn = false;
                }
            }
        }

        _statTakenHighlightStatSliderDelayEffectTimer.UpdateTimer();

        HandleStatTakenHightlightSyncEffect();
    }


    // Event Handlers.
    private void CharacterStats_OnStatUpdated()
    {
        _statSlider.value = GetCharacterStatCurrentValue();

        if (_isStatTakenHightlightSyncEffectOn) return;

        float statTakenValue = _statTakenHighlightSlider.value - _statSlider.value;

        if (statTakenValue > _statTakenThresholdForTakenHighlightEffect)
        {
            _statTakenHighlightStatSliderDelayEffectTimer.StartTimer(
                                                            _statTakenHighlightSyncEffectDelayTime, 
                                                            true);
        }
        else
        {
            _statTakenHighlightSlider.value = _statSlider.value;
        }
    }
}
