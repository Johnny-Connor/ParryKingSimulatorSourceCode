using UnityEngine;

public class GameSceneController : MonoBehaviour
{
    // Variables.
    [SerializeField] private SceneTransitioner _sceneTransitioner;
    [SerializeField] private ControlCharacter _playerControlCharacter;


    // MonoBehaviour.
    private void Start() =>
        _playerControlCharacter.OnDestroyed += PlayerControlCharacter_OnDestroyed;


    // Event Handlers.
    private void PlayerControlCharacter_OnDestroyed() =>
        _sceneTransitioner.StartTransitionFromSceneToLoadingScreenScene(
            "MainMenuScene",
            "LoadingScreenScene");
}
