using System;
using UnityEngine;

public class WeaponHitBoxController : MonoBehaviour
{
    // Components
    private CapsuleCollider _weaponHitBox;
    public CapsuleCollider WeaponHitBox => _weaponHitBox;

    private ControlCharacter _controlCharacter;


    // Variables.
    [SerializeField] private Transform _weaponBearer;
    public Transform WeaponBearer => _weaponBearer;


    // Events.
    public event Action OnGotParried;
    public event Action OnHitInflicted;


    // MonoBehaviour.
    private void Awake()
    {
        _weaponHitBox = GetComponent<CapsuleCollider>();
        _weaponHitBox.enabled = false;

        _controlCharacter = GetComponentInParent<ControlCharacter>();
    }

    private void Start()
    {
        CharacterAnimatorController characterAnimatorController = 
            _controlCharacter.GetComponent<CharacterAnimatorController>();
        characterAnimatorController.AnimationEventOnAttackHitFramesStart += 
            CharacterAnimatorController_AnimationEventOnAttackHitFramesStart;
        characterAnimatorController.AnimationEventOnAttackHitFramesEnd += 
            CharacterAnimatorController_AnimationEventOnAttackHitFramesEnd;

        CharacterStateMachine characterStateMachine = _controlCharacter.CharacterStateMachine;
        characterStateMachine.OnExitedState += CharacterStateMachine_OnExitedState;
    }

    private void OnTriggerEnter(Collider collider)
    {
        bool hasHitBoxBodyControllerComponent = 
            collider.TryGetComponent(out CharacterHitBoxController characterHitBoxBodyController);
        bool isFriendlyHit = WeaponBearer.gameObject.layer == collider.gameObject.layer;

        if (!hasHitBoxBodyControllerComponent || isFriendlyHit) return;
        
        if (characterHitBoxBodyController.IsParrying)
        {
            OnGotParried?.Invoke();
            return;
        }

        if (characterHitBoxBodyController.CanTakeDamage) OnHitInflicted?.Invoke();
    }


    // Event Handlers.
    private void CharacterAnimatorController_AnimationEventOnAttackHitFramesStart() => 
        _weaponHitBox.enabled = true;

    private void CharacterAnimatorController_AnimationEventOnAttackHitFramesEnd() => 
        _weaponHitBox.enabled = false;
        
    private void CharacterStateMachine_OnExitedState(object sender, EventArgs e)
    {
        // Useful when attack is interrupted during attack hit frames when hit.
        _weaponHitBox.enabled = false;
    }
}
