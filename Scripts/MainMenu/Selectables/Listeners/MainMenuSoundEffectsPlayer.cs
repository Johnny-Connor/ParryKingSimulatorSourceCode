using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class MainMenuSoundEffectsPlayer : MonoBehaviour
{
    // Audio Variables.
    [Header("Audio Variables")]

    [Tooltip("PlayOneShot does not cancel audio clips that are already being played.")]
    [SerializeField] private bool _playOneShot = true;

    [Tooltip("The cooldown period between consecutive PlayOneShot audio events.")]
    [SerializeField] [Range(0, 0.2f)] private float _playOneShotCooldown = 0.05f;

    [Tooltip("The volume level of the sounds emitted by the AudioSource.")]
    [SerializeField] [Range(0, 1f)] private float _audioVolume = 1f;
    
    [SerializeField] private AudioClip _menuReturnAudioClip;
    [SerializeField] private AudioClip _selectableChangeAudioClip;
    private AudioSource _audioSource;

    // Components.
    [Header("Components")]
    [Tooltip("Reference to the MainMenuController script. Used for listening to the events of its selectors to play their sounds.")]
    [SerializeField] private MainMenuController _mainMenuController;

    // Other Variables.
    /*
    List used to store references to selectables from a previous state. Particularly useful for state
    switch buttons, as MainMenuSelectable.OnInteracted is fired after MainMenuStateMachine.OnExitedState,
    making it difficult to listen to with local references alone.
    */
    private List<MainMenuSelectable> _previousStateMainMenuSelectablesList = new();
    private Timer _playOneShotCooldownTimer = new();


    // MonoBehaviour.
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        MainMenuStateMachine mainMenuStateMachine = _mainMenuController.MainMenuStateMachine;
        mainMenuStateMachine.OnEnteredState += MainMenuStateMachine_OnEnteredState;
        mainMenuStateMachine.OnExitedState += MainMenuStateMachine_OnExitedState;
        mainMenuStateMachine.OnReturnedToMenuState += MainMenuStateMachine_OnReturnedToMenuState;
    }

    private void Update() => _playOneShotCooldownTimer.UpdateTimer();


    // Non-MonoBehaviour.
    private void PlayAudio(AudioClip audioClip)
    {
        if (_playOneShot)
        {
            if (_playOneShotCooldownTimer.TimeLeft == 0)
            {
                _audioSource.PlayOneShot(audioClip, _audioVolume);

                _playOneShotCooldownTimer.StartTimer(_playOneShotCooldown);
            }
        }
        else
        {
            _audioSource.clip = audioClip;
            _audioSource.volume = _audioVolume;
            _audioSource.Play();
        }
    }


    // Event Handlers.
    private void BaseMenuState_OnSelectableChanged(Selectable selectable)
    {
        PlayAudio(_selectableChangeAudioClip);
    }

    private void MainMenuStateMachine_OnEnteredState(object sender, EventArgs e)
    {
        BaseMenuState baseMenuState = sender as BaseMenuState;

        // Subscribes to main menu state events.
        baseMenuState.OnSelectableChanged += BaseMenuState_OnSelectableChanged;

        // Subscribes to selectable events.
        foreach (Selectable selectable in baseMenuState.SelectablesList)
        {
            MainMenuSelectable mainMenuSelectable = selectable.GetComponent<MainMenuSelectable>();

            if (_previousStateMainMenuSelectablesList.Contains(mainMenuSelectable))
            {
                continue;
            }
            else
            {
                mainMenuSelectable.OnInteracted += MainMenuSelectable_OnInteracted;
                _previousStateMainMenuSelectablesList.Add(mainMenuSelectable);
            }
        }
    }

    private void MainMenuStateMachine_OnExitedState(object sender, EventArgs e)
    {
        BaseMenuState baseMenuState = sender as BaseMenuState;

        // Unsubscribes from main menu state events.
        baseMenuState.OnSelectableChanged -= BaseMenuState_OnSelectableChanged;
    }

    private void MainMenuSelectable_OnInteracted(object sender, EventArgs e)
    {
        MainMenuSelectable mainMenuSelectable = sender as MainMenuSelectable;
        AudioClip interactionAudioClip = mainMenuSelectable.InteractionAudioClip;

        PlayAudio(interactionAudioClip);
    }

    private void MainMenuStateMachine_OnReturnedToMenuState() => PlayAudio(_menuReturnAudioClip);
}
