using System;
using UnityEngine;

public class CharacterHitBoxController : MonoBehaviour
{
    // Components.
    private WeaponHitBoxController _hitBoxWeaponCollider;

    // Paramenters.
    [Header("Paramenters")]

    [Tooltip("The duration of invincibility in seconds granted to the character after taking damage.")]
    [SerializeField] private float _invincibilityTime = 0.2f;

    [SerializeField] private int _ignoreCollisionsLayerIndex = 3;

    // Variables.
    private Timer _invincibilityTimer;

    private bool _isParrying;
    public bool IsParrying => _isParrying;

    private bool _canTakeDamage;
    public bool CanTakeDamage => _canTakeDamage;

    private bool _hasDied;
    private bool _isInAnimationIFrames;

    // Events.
    public event Action<HitInformation> OnHitTaken;
    public event Action OnParrySucceeded;


    // MonoBehaviour.
    private void Awake()
    {
        _hitBoxWeaponCollider = GetComponentInChildren<WeaponHitBoxController>();

        SetCanTakeDamage(true);
        _invincibilityTimer = new(() =>
        {
            if (!_isInAnimationIFrames)
            {
                SetCanTakeDamage(true);
            }
        });
    }

    private void Start()
    {
        CharacterStateMachine characterStateMachine = GetComponent<ControlCharacter>().CharacterStateMachine;
        characterStateMachine.OnExitedState += CharacterStateMachine_OnExitedState;

        CharacterStats characterStats = GetComponent<CharacterStats>();
        characterStats.OnDeath += CharacterStats_OnDeath;
    }

    private void Update() => _invincibilityTimer.UpdateTimer();

    private void OnTriggerEnter(Collider collider)
    {
        bool isCollidingWithOwnWeapon = collider.gameObject == _hitBoxWeaponCollider.WeaponHitBox.gameObject;
        bool isColliderAWeapon = collider.gameObject.layer == 
            _hitBoxWeaponCollider.WeaponHitBox.gameObject.layer;
        bool isFriendlyHit = false;

        if (isColliderAWeapon)
        {
            Transform hitWeaponBearer = collider.GetComponent<WeaponHitBoxController>().WeaponBearer;
            isFriendlyHit = hitWeaponBearer.gameObject.layer == gameObject.layer;
        }

        if (isCollidingWithOwnWeapon || !isColliderAWeapon || isFriendlyHit) return;

        if (_isParrying)
        {
            OnParrySucceeded?.Invoke();
            return;
        }

        if (_canTakeDamage)
        {
            Transform hitWeaponBearer = collider.GetComponent<WeaponHitBoxController>().WeaponBearer;
            int hitWeaponBearerDamage = hitWeaponBearer.GetComponent<CharacterStats>().CurrentDamage;
            Vector3 dirToHitWeaponBearer = (hitWeaponBearer.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, dirToHitWeaponBearer);

            if (dotProduct >= 0)
            {
                HitInformation hitInformation = 
                    new (hitWeaponBearerDamage, HitInformation.HitDirection.Front);
                OnHitTaken?.Invoke(hitInformation);
            }
            else
            {
                HitInformation hitInformation = 
                    new (hitWeaponBearerDamage, HitInformation.HitDirection.Back);
                OnHitTaken?.Invoke(hitInformation);
            }

            SetCanTakeDamage(false);
            _invincibilityTimer.StartTimer(_invincibilityTime);
            return;
        }
    }


    // Non-MonoBehaviour.
    private void SetCanTakeDamage(bool value)
    {
        if (value && _hasDied) return;
        
        _canTakeDamage = value;
    }


    // Event Handlers.
    private void CharacterStateMachine_OnExitedState(object sender, EventArgs e)
    {
        // Useful when an animation is cancelled before its OnIFramesEnd event.
        _isInAnimationIFrames = false;

        // Useful when parry in cancelled before OnParryFramesEnd.
        _isParrying = false;
    }

    private void CharacterStats_OnDeath()
    {
        _hasDied = true;
        SetCanTakeDamage(false);
        gameObject.layer = _ignoreCollisionsLayerIndex;
    }

    private void OnIFramesStart(AnimationEvent animationEvent)
    {
        SetCanTakeDamage(false);
        _isInAnimationIFrames = true;
    }

    private void OnIFramesEnd(AnimationEvent animationEvent)
    {
        SetCanTakeDamage(true);
        _isInAnimationIFrames = false;
    }

    private void OnParryFramesStart(AnimationEvent animationEvent) => _isParrying = true;

    private void OnParryFramesEnd(AnimationEvent animationEvent) => _isParrying = false;
}
