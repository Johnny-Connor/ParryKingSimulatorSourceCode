using Cinemachine;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CombatCameraController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterCombatStats _characterCombatStats;
    [SerializeField] private CharacterStats _characterStats;
    [SerializeField] private ControlCharacter _controlCharacter;
    [SerializeField] private Transform _characterTransform;
    private CinemachineVirtualCamera _cinemachineVirtualCombatCamera;
    private PlayerInputActions _playerInputActions;

    // Other Variables.
    private enum LockOnDirection
    {
        Left,
        Right,
    }

    private bool _hasDied;

    // Events.
    public static Action<Transform> OnAnyLookAtTargetUpdated;


    // MonoBehaviour.
    private void Awake()
    {
        _cinemachineVirtualCombatCamera = GetComponent<CinemachineVirtualCamera>();
        
        /*
        Disable the combat camera to ensure it does not interfere with the movement camera when there is
        no 'Look At' target.
        */
        _cinemachineVirtualCombatCamera.enabled = false;

        _characterStats.OnDeath += CharacterStatsPlayer_OnDeath;
    }

    private void Start() => _playerInputActions = _controlCharacter.PlayerInputActions;

    private void Update()
    {
        if (_hasDied) return;

        if (_characterCombatStats.VisibleTargetDotLocationsWithinRadiusList.Count == 0 && 
            _cinemachineVirtualCombatCamera.enabled)
        {
            TurnCombatCameraOff();
            return;
        }

        if (_playerInputActions.Game.ToggleLockOn.WasPerformedThisFrame()) ToggleLockOn();

        if (_playerInputActions.Game.LockOnCycleLeft.WasPerformedThisFrame()) 
            LockOnCycle(LockOnDirection.Left);
        if (_playerInputActions.Game.LockOnCycleRight.WasPerformedThisFrame()) 
            LockOnCycle(LockOnDirection.Right);
    }


    // Non-MonoBehaviour.
    #region Non-MonoBehaviour
    private void SetLookAtTarget(Transform newTarget)
    {
        Transform oldTarget = _cinemachineVirtualCombatCamera.LookAt;

        // Check if oldTarget is not null before accessing its components.
        if (oldTarget != null)
        {
            CharacterStats characterStatsTargetOld = 
                oldTarget.gameObject.GetComponentInParent<CharacterStats>();
            
            // Check if characterStatsTargetOld is not null before accessing its events.
            if (characterStatsTargetOld != null)
            {
                characterStatsTargetOld.OnDeath -= CharacterStatsTarget_OnDeath;
            }
        }

        _cinemachineVirtualCombatCamera.LookAt = newTarget;
        OnAnyLookAtTargetUpdated?.Invoke(newTarget);

        // Check if newTarget is not null before accessing its components.
        if (newTarget != null)
        {
            CharacterStats characterStatsTargetNew = 
                newTarget.gameObject.GetComponentInParent<CharacterStats>();
            
            // Check if characterStatsTargetNew is not null before accessing its events.
            if (characterStatsTargetNew != null)
            {
                characterStatsTargetNew.OnDeath += CharacterStatsTarget_OnDeath;
            }
        }
    }

    private void TurnCombatCameraOff()
    {
        _cinemachineVirtualCombatCamera.enabled = false;

        SetLookAtTarget(null);
    }

    // Targets a target based on how close they are to the character.
    private void ToggleLockOn()
    {
        if (_cinemachineVirtualCombatCamera.LookAt)
        {
            TurnCombatCameraOff();
            return;
        }
        
        float shortestDistance = Mathf.Infinity;
        Transform closestTarget = null;

        // Find the closest target to the player
        foreach (Transform targetTransform in _characterCombatStats.VisibleTargetDotLocationsWithinRadiusList)
        {
            float distance = Vector3.Distance(targetTransform.position, _characterTransform.position);

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                closestTarget = targetTransform;
            }
        }

        if (closestTarget != null)
        {
            // Enable the combat camera and set the closest target as the 'Look At' target
            _cinemachineVirtualCombatCamera.enabled = true;
            SetLookAtTarget(closestTarget);
        }
    }

    private void LockOnCycle(LockOnDirection direction)
    {
        if (!_cinemachineVirtualCombatCamera.enabled) return;

        List<Transform> visibleTargetsDotLocationsWithinRadiusList = 
            _characterCombatStats.VisibleTargetDotLocationsWithinRadiusList;

        List<Transform> visibleTargetsDotLocationsWithinRadiusSortedByAngleList = 
            GetVisibleTargetsDotLocationsWithinRadiusSortedByAngleList(
                visibleTargetsDotLocationsWithinRadiusList);

        int currentIndex = 
            visibleTargetsDotLocationsWithinRadiusSortedByAngleList.IndexOf(
                _cinemachineVirtualCombatCamera.LookAt);

        int nextIndex = currentIndex + (direction == LockOnDirection.Right ? 1 : -1);
        if (nextIndex >= visibleTargetsDotLocationsWithinRadiusSortedByAngleList.Count)
        {
            nextIndex = 0;
        }
        else if (nextIndex < 0)
        {
            nextIndex = visibleTargetsDotLocationsWithinRadiusSortedByAngleList.Count - 1;
        }

        SetLookAtTarget(visibleTargetsDotLocationsWithinRadiusSortedByAngleList[nextIndex]);
    }

    private List<Transform> GetVisibleTargetsDotLocationsWithinRadiusSortedByAngleList(
        List<Transform> targetsDotLocationsWithingRadius)
    {
        // Sort the target based on their relative position to the player.
        targetsDotLocationsWithingRadius.Sort((a, b) =>
        {
            Vector3 playerToA = _characterTransform.position - a.position;
            Vector3 playerToB = _characterTransform.position - b.position;

            // The angle between the player's forward vector and the vector from the player to target A.
            float angleA = Vector3.Angle(_characterTransform.forward, playerToA);
            // The angle between the player's forward vector and the vector from the player to target B.
            float angleB = Vector3.Angle(_characterTransform.forward, playerToB);

            /*
            Check if target A is to the left of the player by calculating the cross product of the forward
            vector of the target and the vector from the target to the player. If the y component of the
            resulting vector is negative, it means that the target is to the left of the player.
            */
            if (Vector3.Cross(_characterTransform.forward, playerToA).y < 0)
            {
                /*
                Adjust the angle of target A by subtracting its angle from 360 degrees to make it relative
                to the player's position for the sorting comparisons.
                */
                angleA = 360 - angleA;
            }

            /*
            Check if target B is to the left of the player by calculating the cross product of the forward
            vector of the target and the vector from the target to the player. If the y component of the
            resulting vector is negative, it means that the target is to the left of the player.
            */
            if (Vector3.Cross(_characterTransform.forward, playerToB).y < 0)
            {
                /*
                Adjust the angle of target B by subtracting its angle from 360 degrees to make it relative
                to the player's position for the sorting comparisons.
                */
                angleB = 360 - angleB;
            }

            /*
            Sort the given list using the 'Quicksort' algorithm (https://visualgo.net/en/sorting), which
            repeatedly divides the list into smaller sublists and compares their elements in pairs to sort
            the entire list. The comparison between two elements is performed as follows:
            - If 'a' is less than 'b', returns a negative integer (-1 by convention), resulting in 'a'
            being placed before 'b'.
            - If 'a' is greater than 'b', returns a positive integer (1 by convention), resulting in 'a'
            being placed after 'b'.
            - If 'a' and 'b' are equal, returns zero, resulting in 'a' being placed alongside 'b'.

            Note that the comparison function is not commutative, meaning that swapping 'a' and 'b' will
            result in a different sorting order.
            */
            return angleA.CompareTo(angleB);
        });

        return targetsDotLocationsWithingRadius;
    }
    #endregion Non-MonoBehaviour


    // Event Handlers.
    private void CharacterStatsPlayer_OnDeath()
    {
        _hasDied = true;

        TurnCombatCameraOff();
    }

    private void CharacterStatsTarget_OnDeath()
    {
        TurnCombatCameraOff();

        List<Transform> visibleTargetsDotLocationsWithinRadiusList = 
            _characterCombatStats.VisibleTargetDotLocationsWithinRadiusList;
        List<Transform> visibleTargetsDotLocationsWithinRadiusSortedByAngleList = 
            GetVisibleTargetsDotLocationsWithinRadiusSortedByAngleList(
                visibleTargetsDotLocationsWithinRadiusList);

        if (visibleTargetsDotLocationsWithinRadiusSortedByAngleList.Count > 0) ToggleLockOn();        
    }
}
