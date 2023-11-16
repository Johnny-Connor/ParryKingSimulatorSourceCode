using UnityEngine;

public class DeathScreenAudioController : MonoBehaviour
{
    // Audio Clips.
    [Header("Audio Clips")]
    [SerializeField] private AudioClip _deathScreenAudioClip;

    // Audio Variables.
    [Header("Audio Variables")]

    [Tooltip("The volume level of the sound emitted by the death screen.")]
    [SerializeField] [Range(0, 1f)] private float _audioVolume = 1f;

    // Components.
    [Header("Components")]
    [SerializeField] private CharacterAnimatorController _characterAnimatorController;


    // MonoBehaviour.
    private void Awake()
    {
        _characterAnimatorController.AnimationEventOnDeathStateAnimationEnd += 
            CharacterAnimatorController_AnimationEventOnDeathStateAnimationEnd;
    }


    // Event Handlers.
    private void CharacterAnimatorController_AnimationEventOnDeathStateAnimationEnd()
    {
        AudioSource cameraAudioSource = Camera.main.GetComponent<AudioSource>();
        cameraAudioSource.PlayOneShot(_deathScreenAudioClip, _audioVolume);
    }
}
