using System;
using UnityEngine;

public class CharacterVFXController : MonoBehaviour
{
    // Variables.
    [Header("Fade Out Variables.")]
    [SerializeField] private float _fadeOutDuration = 1;
    private Lerper _fadeOutLerper;
    private MaterialPropertyBlock _materialPropertyBlock;
    private SkinnedMeshRenderer[] _skinnedMeshRenderers;
    private float _currentOpacity;

    [Header("Death Particles Variables.")]
    [SerializeField] private ParticleSystem[] _deathParticles;
    private bool _areAllDeathParticlesDead;

    // Constants.
    private const float INITIAL_OPACITY = 1;
    private const float END_OPACITY = 0;

    // Events.
    public event Action OnCharacterFadeOutAnimationAndParticlesCompleted;


    // MonoBehaviour.
    private void Awake()
    {
        _fadeOutLerper = new(INITIAL_OPACITY, END_OPACITY, _fadeOutDuration);
        _materialPropertyBlock = new();
        _skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        _currentOpacity = INITIAL_OPACITY;

        CharacterAnimatorController characterAnimatorController = 
            GetComponent<CharacterAnimatorController>();
        characterAnimatorController.AnimationEventOnDeathStateAnimationStart += 
            CharacterAnimatorController_AnimationEventOnDeathStateAnimationStart;
        characterAnimatorController.AnimationEventOnDeathStateAnimationEnd += 
            CharacterAnimatorController_AnimationEventOnDeathStateAnimationEnd;
    }

    private void Update()
    {
        _fadeOutLerper.UpdateLerper(ref _currentOpacity);

        foreach (SkinnedMeshRenderer skinnedMeshRenderer in _skinnedMeshRenderers)
        {
            /*
            MaterialPropertyBlocks enable drawing multiple objects using the same material but with
            distinct properties.
            */
            skinnedMeshRenderer.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetFloat("_Opacity", _currentOpacity);
            skinnedMeshRenderer.SetPropertyBlock(_materialPropertyBlock);
        }

        if (_currentOpacity == 0)
        {
            _areAllDeathParticlesDead = true;

            foreach (ParticleSystem particleSystem in _deathParticles)
            {
                if (particleSystem.IsAlive())
                {
                    _areAllDeathParticlesDead = false;
                    break;
                }
            }

            if (_areAllDeathParticlesDead) OnCharacterFadeOutAnimationAndParticlesCompleted?.Invoke();
        }
    }


    // Event Handlers.
    private void CharacterAnimatorController_AnimationEventOnDeathStateAnimationStart()
    {
        foreach (ParticleSystem particleSystem in _deathParticles) particleSystem.Play();
    }

    private void CharacterAnimatorController_AnimationEventOnDeathStateAnimationEnd()
    {
        _fadeOutLerper.StartLerper();

        foreach (ParticleSystem particleSystem in _deathParticles)
        {
            // Stop emission without clearing current particles.
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}
