using UnityEngine;
using UnityEngine.Audio;

public class AudioMixerSetter : MonoBehaviour
{
    // Variables.
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private string _masterAudioMixerGroupString = "Master";
    [SerializeField] private string _musicAudioMixerGroupString = "Music";
    [SerializeField] private string _soundEffectsAudioMixerGroupString = "SoundEffects";
    [SerializeField] private string _voiceAudioMixerGroupString = "Voice";


    private void Awake()
    {
        _audioMixer.SetFloat(_masterAudioMixerGroupString, 
                            PlayerPrefs.GetFloat(_masterAudioMixerGroupString, -80));

        _audioMixer.SetFloat(_musicAudioMixerGroupString,
                            PlayerPrefs.GetFloat(_musicAudioMixerGroupString, -80));

        _audioMixer.SetFloat(_soundEffectsAudioMixerGroupString,
                            PlayerPrefs.GetFloat(_soundEffectsAudioMixerGroupString, -80));

        _audioMixer.SetFloat(_voiceAudioMixerGroupString,
                            PlayerPrefs.GetFloat(_voiceAudioMixerGroupString, -80));
    }
}
