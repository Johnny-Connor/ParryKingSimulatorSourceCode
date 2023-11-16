using UnityEngine;

public class DeathParticlesAudioController : MonoBehaviour
{
    // Audio Clips.
    [Header("Audio Clips")]
    [SerializeField] private AudioClip _deathParticlesAudioClip;

    // Audio Variables.
    [Header("Audio Variables")]

    [Tooltip("The volume level of the sound emitted by the death particles.")]
    [SerializeField] [Range(0, 1f)] private float _audioVolume = 0.05f;
    [SerializeField] private float _audioFadeOutDuration = 1.5f;

    // Components.
    private AudioSource _audioSource;

    // Variables.
    private Lerper _fadeOutLerper;

    // Constants.
    private float _initialAudioValue;
    private const float END_AUDIO_VOLUME = 0;


    // MonoBehaviour.
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.volume = _audioVolume;
        _initialAudioValue = _audioVolume;
        
        _fadeOutLerper = new(_initialAudioValue, END_AUDIO_VOLUME, _audioFadeOutDuration);

        CharacterAnimatorController characterAnimatorController = 
            GetComponentInParent<CharacterAnimatorController>();
        characterAnimatorController.AnimationEventOnDeathStateAnimationStart += 
            CharacterAnimatorController_AnimationEventOnDeathStateAnimationStart;
        characterAnimatorController.AnimationEventOnDeathStateAnimationEnd += 
            CharacterAnimatorController_AnimationEventOnDeathStateAnimationEnd;
    }

    private void Update()
    {
        _fadeOutLerper.UpdateLerper(ref _audioVolume);

        _audioSource.volume = _audioVolume;
    }


    // Event Handlers.
    private void CharacterAnimatorController_AnimationEventOnDeathStateAnimationStart()
    {
        _audioSource.clip = _deathParticlesAudioClip;
        _audioSource.volume = _audioVolume;
        _audioSource.Play();
    }

    private void CharacterAnimatorController_AnimationEventOnDeathStateAnimationEnd()
    {
        _fadeOutLerper.StartLerper();
    }
}
