using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitioner : MonoBehaviour
{
    // Variables.
    private static string _sceneToLoadName;

    private Animator _animator;
    private int _animIDStartSceneTransition = Animator.StringToHash("StartSceneTransition");

    private string _loadingScreenSceneName;

    // Events.
    public event Action AnimationEventOnFadedOut;


    // MonoBehaviour.
    private void Awake() => _animator = GetComponent<Animator>();


    // Non-MonoBehaviour.
    public void StartTransitionFromSceneToLoadingScreenScene
    (string sceneToLoadName, string loadingScreenSceneName)
    {
        _sceneToLoadName = sceneToLoadName;
        _loadingScreenSceneName = loadingScreenSceneName;

        _animator.SetTrigger(_animIDStartSceneTransition);
    }

    public void StartTransitionFromLoadingScreenSceneToScene()
    {
        _animator.SetTrigger(_animIDStartSceneTransition);
    }

    private void LoadScene()
    {
        if (string.IsNullOrEmpty(_loadingScreenSceneName))
        {
            // Loads scene.
            SceneManager.LoadScene(_sceneToLoadName);
        }
        else
        {
            // Loads loading screen scene.
            SceneManager.LoadScene(_loadingScreenSceneName);
        }
    }


    // Animation Event Handlers.
    private void OnFadedIn() => LoadScene();
    private void OnFadedOut() => AnimationEventOnFadedOut?.Invoke();
}
