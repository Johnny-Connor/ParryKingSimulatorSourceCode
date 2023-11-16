using UnityEngine;

public class LoadingScreenSceneController : MonoBehaviour
{
    [SerializeField] private SceneTransitioner _sceneTransitioner;

    private void Start()
    {
        _sceneTransitioner.AnimationEventOnFadedOut += 
            () => _sceneTransitioner.StartTransitionFromLoadingScreenSceneToScene();
    }
}
