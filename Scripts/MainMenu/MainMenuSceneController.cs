using UnityEngine;

public class MainMenuSceneController : MonoBehaviour
{
    // Variables.
    [SerializeField] private SceneTransitioner _sceneTransitioner;
    private bool _enableNewGameButton;


    // MonoBehaviour.
    public void Start() =>
        _sceneTransitioner.AnimationEventOnFadedOut += () => _enableNewGameButton = true;
    

    // Unity Event Handlers.
    public void NewGameButton()
    {
        if (_enableNewGameButton)
            _sceneTransitioner.StartTransitionFromSceneToLoadingScreenScene(
                "GameScene", 
                "LoadingScreenScene");
    }

    public void QuitGame() => Application.Quit();
}
