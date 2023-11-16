// Useful for canvases that use the 'World Space' render mode.

using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    // Variables.
    [SerializeField] private bool _invert;
    private Transform _cameraTransform;


    // MonoBehaviour.
    private void Awake() => _cameraTransform = Camera.main.transform;

    // Ensures that the rotation will only update after the camera moves, that is, after Update.
    private void LateUpdate()
    {
        if (_invert)
        {
            Vector3 dirToCamera = (_cameraTransform.position - transform.position).normalized;
            transform.LookAt(transform.position + dirToCamera * -1);
        }
        else
        {
            /*
            The use of the "transform.LookAt(_cameraTransform)" command without inversion will result in a 
            mirrored appearance of the elements due to the fact that the UI element will be facing the 
            camera, which is facing the UI. This phenomenon can be observed in practice when, for example, 
            one looks at an individual's face: their left eye will appear as their right eye, and vice 
            versa. 
            */
            transform.LookAt(_cameraTransform);
        }
    }
}
