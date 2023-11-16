using Cinemachine;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCombatStats : MonoBehaviour
{
    #region Variables
    #region General Variables
    [Header("General Variables")]

    [Tooltip("The height at which Raycasts are instantiated.")]
    [SerializeField] private Transform _characterEyes;

    [Tooltip("The radius within which the character can detect and target (lock on) other objects.")]
    [SerializeField] private float _targetDetectionRadius = 10;

    [Tooltip("The layers that the character can target (lock on).")]
    [SerializeField] private LayerMask _targetDotLocationLayerMask = 1 << 6;

    private ControlCharacter _controlCharacter;

    // The height at which OverlapSpheres are instantiated.
    private float _characterControllerCenterY;

    // Can be targeted by others?
    private bool _canBeTargeted = true;
    #endregion General Variables

    [Header("Player Variables")]
    [SerializeField] private CinemachineVirtualCamera _cinemachineVirtualCombatCamera;

    #region Parry Variables
    [Header("Parry Variables")]

    [Tooltip("The radius within which the character can parry-stab targets.")]
    [SerializeField] private float _parryStabRadius = 2f;

    [Tooltip("The dot product offset used to determine character-target alignment for parry-stab.")]
    [SerializeField] [Range(0, 2)] private float _parryStabDotOffset = 1f;

    [Tooltip("The distance maintained between the stabber and the stabbed character during the ParryStab process.")]
    [SerializeField] [Range(0, 2)] private float _parryStabStoppingDistance = 1.1f;
    public float ParryStabStoppingDistance => _parryStabStoppingDistance;

    private Transform _parryStabberTransform;
    public Transform ParryStabberTransform => _parryStabberTransform;

    private Transform _parryStabbedTransform;
    public Transform ParryStabbedTransform => _parryStabbedTransform;

    private bool _isParried;
    public bool IsParried => _isParried;
    #endregion Parry Variables
    #endregion Variables


    #region Properties
    // Properties.
    public Transform CurrentTargetTransform 
    {
        get
        {
            if (_controlCharacter.IsPlayer)
            {
                return _cinemachineVirtualCombatCamera.LookAt; 
            }
            else
            {
                return TargetDotLocationsWithinRadiusList.Count > 0 ? 
                    TargetDotLocationsWithinRadiusList[0] : 
                    null;
            }
        }
    }

    public bool IsCollidingWithTargetedTargetInXZAxes
    {
        get
        {
            if (!CurrentTargetTransform) return false;

            // Variables Setup.
            CharacterController characterController = GetComponent<CharacterController>();
            CharacterController targetController = 
                CurrentTargetTransform.GetComponentInParent<CharacterController>();
            float characterRadius = characterController.radius;
            float targetRadius = targetController.radius;

            Vector2 characterPositionXZ = new(transform.position.x, transform.position.z);
            Vector2 targetPositionXZ = 
                new(CurrentTargetTransform.position.x, CurrentTargetTransform.position.z);


            // Calculation.
            /*
            By summing the radii of both objects, the total width required for collision detection is 
            calculated.
            */
            return Mathf.Abs(characterPositionXZ.x - targetPositionXZ.x) <= characterRadius + targetRadius &&
                Mathf.Abs(characterPositionXZ.y - targetPositionXZ.y) <= characterRadius + targetRadius;
        }
    }

    private List<Transform> TargetDotLocationsWithinRadiusList
    {
        get
        {
            Collider[] hitColliders = Physics.OverlapSphere(
                new Vector3(
                    transform.position.x, 
                    _characterControllerCenterY, 
                    transform.position.z
                ),
                _targetDetectionRadius,
                _targetDotLocationLayerMask);

            List<Transform> targetsDotsWithinRadiusList = new();

            foreach (var hitCollider in hitColliders)
            {
                ControlCharacter controlCharacterFromTarget =
                    hitCollider.gameObject.GetComponentInParent<ControlCharacter>();
                bool isAlly = _controlCharacter.IsPlayer == controlCharacterFromTarget.IsPlayer;

                CharacterCombatStats combatStatsFromTarget = 
                    hitCollider.gameObject.GetComponentInParent<CharacterCombatStats>();
                
                if (combatStatsFromTarget != null)
                {
                    if (!combatStatsFromTarget._canBeTargeted) continue;
                }

                if (isAlly) continue;

                targetsDotsWithinRadiusList.Add(hitCollider.transform);
            }

            return targetsDotsWithinRadiusList;
        }
    }

    public List<Transform> VisibleTargetDotLocationsWithinRadiusList
    {
        get
        {
            List<Transform> visibleTargetsWithinRadiusList = new();

            foreach (Transform target in TargetDotLocationsWithinRadiusList)
            {
                // Cast a ray from character position to the target position.
                Vector3 directionToTarget = target.position - _characterEyes.position;
                Ray rayToTarget = new(_characterEyes.position, directionToTarget);

                // Specify the maximum distance of the raycast based on the distance to the target.
                float maxDistance = directionToTarget.magnitude;

                LayerMask defaultLayer = 1 << 0; // Represents most obstacles.

                // Perform the raycast.
                if (!Physics.Raycast(rayToTarget, maxDistance, defaultLayer))
                {
                    // No obstacles hit, so the target is visible.
                    visibleTargetsWithinRadiusList.Add(target);
                }
            }

            return visibleTargetsWithinRadiusList;
        }
    }
    #endregion Properties


    // Events.
    public Action<Transform> OnParryStabbed;


    // MonoBehaviour.
    private void Awake()
    {
        // Setting variables up.
        CharacterController characterController = GetComponent<CharacterController>();
        _characterControllerCenterY = characterController.center.y;

        _controlCharacter = GetComponent<ControlCharacter>();

        // Subscribing to events.
        OnParryStabbed += CharacterCombatStats_OnParryStabbed;

        CharacterStats characterStats = GetComponent<CharacterStats>();
        characterStats.OnDeath += () => _canBeTargeted = false;
    }

    private void Start()
    {
        CharacterStateMachine characterStateMachine = _controlCharacter.CharacterStateMachine;
        characterStateMachine.OnEnteredState += CharacterStateMachine_OnEnteredState;
    }


    // Non-MonoBehaviour.
    public bool IsAReachableParriedTargetAvailable()
    {
        List<Transform> GetParriedTargetsWithinRadiusList()
        {
            Collider[] hitColliders = Physics.OverlapSphere(
                new Vector3(
                    transform.position.x, 
                    _characterControllerCenterY, 
                    transform.position.z
                ),
                _parryStabRadius,
                _targetDotLocationLayerMask
            );

            List<Transform> parriedTargetsWithinParryStabRadiusList = new();

            foreach (var hitCollider in hitColliders)
            {
                CharacterCombatStats targetCombatStats = 
                    hitCollider.gameObject.GetComponentInParent<CharacterCombatStats>();

                ControlCharacter controlCharacterFromTarget =
                    hitCollider.gameObject.GetComponentInParent<ControlCharacter>();
                bool isAlly = _controlCharacter.IsPlayer == controlCharacterFromTarget.IsPlayer;

                if (targetCombatStats.IsParried && !isAlly)
                {
                    Transform targetTransform = hitCollider.transform;

                    Vector3 dirFromTargetToCharacter = 
                        (transform.position - targetTransform.position).normalized;
                    float dotProduct = Vector3.Dot(targetTransform.forward, dirFromTargetToCharacter);

                    // Checking if target is aligned enough with character.
                    if (dotProduct >= 1 - _parryStabDotOffset) 
                        parriedTargetsWithinParryStabRadiusList.Add(targetTransform);
                }
            }

            return parriedTargetsWithinParryStabRadiusList;
        }

        Transform GetClosestTargetInList(List<Transform> targetsList)
        {
            Transform closestTarget = null;
            float closestDistance = float.MaxValue;
            
            foreach (var target in targetsList)
            {
                float distance = Vector3.Distance(transform.position, target.position);
                if (distance < closestDistance)
                {
                    closestTarget = target;
                    closestDistance = distance;
                }
            }

            return closestTarget;        
        }

        Transform closestParriedTarget = GetClosestTargetInList(GetParriedTargetsWithinRadiusList());
        _parryStabbedTransform = closestParriedTarget;

        return closestParriedTarget;
    }


    // Event Handlers.
    private void CharacterCombatStats_OnParryStabbed(Transform stabberTransform)
    {
        _parryStabberTransform = stabberTransform;
    }

    private void CharacterStateMachine_OnEnteredState(object sender, EventArgs e)
    {
        _isParried = sender is ParriedState;
    }
}
