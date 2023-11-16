using UnityEngine;

public class WeaponVFXController : MonoBehaviour
{
    [SerializeField] private ParticleSystem _bloodParticle;

    private void Awake()
    {
        CharacterAnimatorController characterAnimatorController = 
            GetComponentInParent<CharacterAnimatorController>();
        characterAnimatorController.AnimationEventOnHitInflicted += 
            CharacterAnimatorController_AnimationEventOnHitInflicted;

        WeaponHitBoxController weaponHitBoxController = GetComponent<WeaponHitBoxController>();
        weaponHitBoxController.OnHitInflicted += WeaponHitBoxController_OnHitInflicted;
    }

    private void CharacterAnimatorController_AnimationEventOnHitInflicted() => _bloodParticle.Play();

    private void WeaponHitBoxController_OnHitInflicted() => _bloodParticle.Play();
}
