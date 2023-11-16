using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class MainMenuSlider : MainMenuSelectable
{
    // Variables.
    private Slider _slider;
    private TMP_Text _sliderValueText;
    private float _defaultValue = -1;


    // MonoBehaviour.
    private void Awake()
    {
        _slider = GetComponent<Slider>();

        // Sets the slider's default value.
        if (_defaultValue == -1) _defaultValue = _slider.value;

        // Sets the _sliderValueText variables.
        _sliderValueText = GetComponentInChildren<TMP_Text>();
        UpdateSliderValueText(_slider.value);

        // Adds listeners to events.
        _slider.onValueChanged.AddListener(UpdateSliderValueText);
        _slider.onValueChanged.AddListener((float value) => RaiseOnInteractedEvent());
    }


    // Event Handlers.
    public void ResetValue()
    {
        _slider.value = _defaultValue;
        UpdateSliderValueText(_slider.value);
    }

    private void UpdateSliderValueText(float value) => _sliderValueText.text = value.ToString();
}
