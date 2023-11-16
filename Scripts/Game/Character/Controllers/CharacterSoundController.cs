using UnityEngine;

public class CharacterSoundController : MonoBehaviour
{
    // Audio Clips.
    [Header("Audio Clips")]
    [SerializeField] private AudioClip _attackAudioClip;
    [SerializeField] private AudioClip _backStepAudioClip;
    [SerializeField] private AudioClip[] _deathScreamAudioClips;
    [SerializeField] private AudioClip[] _footstepAudioClips;
    [SerializeField] private AudioClip[] _hitAudioClips;
    [SerializeField] private AudioClip _landAudioClip;
    [SerializeField] private AudioClip _parryAudioClip;
    [SerializeField] private AudioClip _parryStabInsertAudioClip;
    [SerializeField] private AudioClip _parryStabRemoveAudioClip;
    [SerializeField] private AudioClip _parrySucceededAudioClip;
    [SerializeField] private AudioClip _rollAudioClip;


    // Audio Variables.
    [Header("Audio Variables")]

    [Tooltip("The volume level of the sounds emitted by the character.")]
    [SerializeField] [Range(0, 1f)] private float _audioVolume = 1f;

    [Tooltip("Minimum clip weight for blended animation audio events. Lower values trigger sounds during partial clip activations, while higher values inhibit triggering even during higher clip activations.")]
    [SerializeField] [Range(0, 1f)] private float _audioTriggerAnimatorClipWeightThreshold = 0.25f;

    [Tooltip("The cooldown period between consecutive audio events of the same type. Prevents simultaneous triggering of an audio clip.")]
    [SerializeField] [Range(0, 0.2f)] private float _audioClipCooldown = 0.1f;


    // Audio cooldown time left.
    private float _attackAudioClipCooldownDelta;
    private float _backStepAudioClipCooldownDelta;
    private float _deathScreamAudioClipsCooldownDelta;
    private float _footstepAudioClipsCooldownDelta;
    private float _hitAudioClipsCooldownDelta;
    private float _landAudioClipCooldownDelta;
    private float _parryAudioClipCooldownDelta;
    private float _parryStabInsertAudioClipCooldownDelta;
    private float _parryStabRemoveAudioClipCooldownDelta;
    private float _parrySucceededAudioClipCooldownDelta;
    private float _rollAudioClipCooldownDelta;


    // Components.
    [SerializeField] private AudioSource _soundEffectsAudioSource;
    [SerializeField] private AudioSource _voiceAudioSource;


    // MonoBehaviour.
    private void Awake()
    {
        _attackAudioClipCooldownDelta = _audioClipCooldown;
        _backStepAudioClipCooldownDelta = _audioClipCooldown;
        _deathScreamAudioClipsCooldownDelta = _audioClipCooldown;
        _footstepAudioClipsCooldownDelta = _audioClipCooldown;
        _hitAudioClipsCooldownDelta = _audioClipCooldown;
        _landAudioClipCooldownDelta = _audioClipCooldown;
        _parryAudioClipCooldownDelta = _audioClipCooldown;
        _parryStabInsertAudioClipCooldownDelta = _audioClipCooldown;
        _parryStabRemoveAudioClipCooldownDelta = _audioClipCooldown;
        _parrySucceededAudioClipCooldownDelta = _audioClipCooldown;
        _rollAudioClipCooldownDelta = _audioClipCooldown;

        CharacterHitBoxController characterHitBoxBodyController = 
            GetComponent<CharacterHitBoxController>();
        characterHitBoxBodyController.OnHitTaken += HitBoxBodyController_OnHitTaken;
        characterHitBoxBodyController.OnParrySucceeded += HitBoxBodyController_OnParrySucceeded;
    }

    private void Update()
    {
        _attackAudioClipCooldownDelta -= Time.deltaTime;
        _backStepAudioClipCooldownDelta -= Time.deltaTime;
        _deathScreamAudioClipsCooldownDelta -= Time.deltaTime;
        _footstepAudioClipsCooldownDelta -= Time.deltaTime;
        _hitAudioClipsCooldownDelta -= Time.deltaTime;
        _landAudioClipCooldownDelta -= Time.deltaTime;
        _parryAudioClipCooldownDelta -= Time.deltaTime;
        _parryStabInsertAudioClipCooldownDelta -= Time.deltaTime;
        _parryStabRemoveAudioClipCooldownDelta -= Time.deltaTime;
        _parrySucceededAudioClipCooldownDelta -= Time.deltaTime;
        _rollAudioClipCooldownDelta -= Time.deltaTime;
    }


    // Non-MonoBehaviour.
    private void PlayBlendedAnimationAudio(
        AnimationEvent animationEvent, 
        AudioClip audioClip, 
        ref float actionAudioClipCooldownDelta, 
        bool useVoiceAudioSource = false)
    {
        if (animationEvent.animatorClipInfo.weight > _audioTriggerAnimatorClipWeightThreshold && 
        actionAudioClipCooldownDelta <= 0)
        {
            if (useVoiceAudioSource)
            {
                _voiceAudioSource.PlayOneShot(audioClip, _audioVolume);
            }
            else
            {
                _soundEffectsAudioSource.PlayOneShot(audioClip, _audioVolume);
            }

            actionAudioClipCooldownDelta = _audioClipCooldown;
        }
    }

    private void PlayNonBlendedAnimationAudio(
        AudioClip audioClip, 
        ref float actionAudioClipCooldownDelta, 
        bool useVoiceAudioSource = false)
    {
        if (actionAudioClipCooldownDelta <= 0)
        {
            if (useVoiceAudioSource)
            {
                _voiceAudioSource.PlayOneShot(audioClip, _audioVolume);
            }
            else
            {
                _soundEffectsAudioSource.PlayOneShot(audioClip, _audioVolume);
            }

            actionAudioClipCooldownDelta = _audioClipCooldown;
        }
    }


    // Blended Animations Events.
    private void OnFootstep(AnimationEvent animationEvent)
    {
        var index = Random.Range(0, _footstepAudioClips.Length);
        PlayBlendedAnimationAudio(
            animationEvent, 
            _footstepAudioClips[index], 
            ref _footstepAudioClipsCooldownDelta);
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        PlayBlendedAnimationAudio(animationEvent, _landAudioClip, ref _landAudioClipCooldownDelta);
    }


    // Non-Blended Animations Events.
    private void OnAnimationSound(string animationName)
    {
        switch (animationName)
        {
            case "Attack":
                PlayNonBlendedAnimationAudio(_attackAudioClip, ref _attackAudioClipCooldownDelta);
                return;
            case "BackStep":
                PlayNonBlendedAnimationAudio(_backStepAudioClip, ref _backStepAudioClipCooldownDelta);
                return;
            case "Death":
                var index = Random.Range(0, _deathScreamAudioClips.Length);
                PlayNonBlendedAnimationAudio(
                    _deathScreamAudioClips[index], 
                    ref _deathScreamAudioClipsCooldownDelta, 
                    true);
                return;
            case "Parry":
                PlayNonBlendedAnimationAudio(_parryAudioClip, ref _parryAudioClipCooldownDelta);
                return;
            case "ParryStabInsert":
                PlayNonBlendedAnimationAudio(
                    _parryStabInsertAudioClip, 
                    ref _parryStabInsertAudioClipCooldownDelta);
                return;
            case "ParryStabRemove":
                PlayNonBlendedAnimationAudio(
                    _parryStabRemoveAudioClip, 
                    ref _parryStabRemoveAudioClipCooldownDelta);
                return;
            case "Roll":
                PlayNonBlendedAnimationAudio(_rollAudioClip, ref _rollAudioClipCooldownDelta);
                return;
        }
    }

    private void HitBoxBodyController_OnHitTaken(HitInformation hitInformation)
    {
        var index = Random.Range(0, _hitAudioClips.Length);
        PlayNonBlendedAnimationAudio(_hitAudioClips[index], ref _hitAudioClipsCooldownDelta);
    }

    private void HitBoxBodyController_OnParrySucceeded()
    {
        PlayNonBlendedAnimationAudio(_parrySucceededAudioClip, ref _parrySucceededAudioClipCooldownDelta);
    }
}
