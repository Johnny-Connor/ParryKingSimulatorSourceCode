using UnityEngine;

public class TargetDotSpawner : MonoBehaviour
{
    // Variables.
    [SerializeField] private Transform _targetDotPrefab;
    private Transform _targetDot;


    // MonoBehaviour.
    private void Awake() =>
        CombatCameraController.OnAnyLookAtTargetUpdated += CombatCameraController_OnAnyLookAtTargetUpdated;

    private void OnDisable() =>
        CombatCameraController.OnAnyLookAtTargetUpdated += CombatCameraController_OnAnyLookAtTargetUpdated;


    // Event Handlers.
    private void CombatCameraController_OnAnyLookAtTargetUpdated(Transform targetedTargetDotLocation)
    {
        if (_targetDot) Destroy(_targetDot.gameObject);

        if (targetedTargetDotLocation)
        {
            _targetDot = Instantiate(
                            _targetDotPrefab, 
                            targetedTargetDotLocation.position, 
                            Quaternion.identity, 
                            targetedTargetDotLocation.transform);
        }
    }
}
