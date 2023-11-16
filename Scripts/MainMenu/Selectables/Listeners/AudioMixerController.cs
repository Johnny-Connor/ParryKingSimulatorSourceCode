using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioMixerController : MonoBehaviour
{
    // Variables.
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer _audioMixer;

    [Tooltip("The maximum decibel value allowed.")]
    [SerializeField] [Range(-80, 20)] private int _maxDecibelLevel = 0;

    [Tooltip("The minimum decibel value allowed. When the audio volume reaches this level, it will be set to the lowest possible decibel level (-80 dB).")]
    [SerializeField] [Range(-80, 20)] private int _minDecibelLevel = -40;

    [Header("Master Volume")]
    [SerializeField] private Slider _masterVolumeSlider;
    [SerializeField] private string _masterAudioMixerGroupString = "Master";

    [Header("Music Volume")]
    [SerializeField] private Slider _musicVolumeSlider;
    [SerializeField] private string _musicAudioMixerGroupString = "Music";

    [Header("Sound Effects Volume")]
    [SerializeField] private Slider _soundEffectsVolumeSlider;
    [SerializeField] private string _soundEffectsAudioMixerGroupString = "SoundEffects";

    [Header("Voice Volume")]
    [SerializeField] private Slider _voiceVolumeSlider;
    [SerializeField] private string _voiceAudioMixerGroupString = "Voice";

    private Dictionary<string, Slider> _audioMixerGroupStringToSliderDictionary;


    // MonoBehaviour.
    private void Awake()
    {
        _audioMixerGroupStringToSliderDictionary = new()
        {
            [_masterAudioMixerGroupString] = _masterVolumeSlider,
            [_musicAudioMixerGroupString] = _musicVolumeSlider,
            [_soundEffectsAudioMixerGroupString] = _soundEffectsVolumeSlider,
            [_voiceAudioMixerGroupString] = _voiceVolumeSlider
        };

        // Subscribes to sliders' value change event for each audio mixer group.
        foreach (KeyValuePair<string, Slider> audioMixerGroupStringToVolumeSlider in 
            _audioMixerGroupStringToSliderDictionary)
        {
            Slider audioMixerGroupSlider = audioMixerGroupStringToVolumeSlider.Value;

            audioMixerGroupSlider.onValueChanged.AddListener((value) => 
                VolumeSlider_OnValueChanged(audioMixerGroupStringToVolumeSlider));
        }
    }

    private void Start()
    {
        foreach (KeyValuePair<string, Slider> audioMixerGroupStringToSlider in 
        _audioMixerGroupStringToSliderDictionary)
        {
            Slider audioMixerGroupSlider = audioMixerGroupStringToSlider.Value;

            float startingSliderValueToDecibel = SliderValueToDecibel(audioMixerGroupSlider);

            // Sets sliders to previously saved or default values.
            audioMixerGroupSlider.value = DecibelToSliderValue(
                PlayerPrefs.GetFloat(audioMixerGroupStringToSlider.Key, 
                                    startingSliderValueToDecibel), 
                audioMixerGroupSlider);

            // Sets the audio mixer groups' decibel based on their slider value.
            UpdateAudioMixerGroupDecibel(audioMixerGroupStringToSlider);
        }
    }


    // Non-MonoBehaviour.
    private float DecibelToSliderValue(float decibelValue, Slider sliderToMap)
    {
        float normalizedSliderValue = 
            (decibelValue - _minDecibelLevel) / (_maxDecibelLevel - _minDecibelLevel);
        return Mathf.Lerp(sliderToMap.minValue, sliderToMap.maxValue, normalizedSliderValue);
    }

    private float SliderValueToDecibel(Slider slider)
    {
        float normalizedSliderValue = 
            Mathf.InverseLerp(slider.minValue, slider.maxValue, slider.value);
        float decibelValue = Mathf.Lerp(_minDecibelLevel, _maxDecibelLevel, normalizedSliderValue);
        return decibelValue <= _minDecibelLevel ? -80 : decibelValue;
    }

    private void UpdateAudioMixerGroupDecibel(KeyValuePair<string, Slider> audioMixerGroupStringToSlider)
    {
        float sliderValueToDecibel = SliderValueToDecibel(audioMixerGroupStringToSlider.Value);
        _audioMixer.SetFloat(audioMixerGroupStringToSlider.Key, sliderValueToDecibel);
        PlayerPrefs.SetFloat(audioMixerGroupStringToSlider.Key, sliderValueToDecibel);
    }


    // Event Handlers.
    private void VolumeSlider_OnValueChanged(
        KeyValuePair<string, Slider> audioMixerGroupStringAndSliderPair) =>
            UpdateAudioMixerGroupDecibel(audioMixerGroupStringAndSliderPair);
}
