using System;
using UnityEngine;

public class DeathScreenAnimatorController : MonoBehaviour
{
    // Variables.
    [SerializeField] private CharacterAnimatorController _characterAnimatorController;
    private Animator _animator;
    private int _animIDStartDeathScreenAnimation = Animator.StringToHash("StartDeathScreenAnimation");

    // Events.
    public event Action OnDeathScreenAnimationEnd;


    // MonoBehaviour.
    private void Awake()
    {
        _animator = GetComponent<Animator>();

        _characterAnimatorController.AnimationEventOnDeathStateAnimationEnd += 
            CharacterAnimatorController_AnimationEventOnDeathStateAnimationEnd;
    }


    // Event Handlers.
    private void CharacterAnimatorController_AnimationEventOnDeathStateAnimationEnd()
    {
        _animator.SetTrigger(_animIDStartDeathScreenAnimation);
    }

    private void OnAnimationEnd() => OnDeathScreenAnimationEnd?.Invoke();
}
