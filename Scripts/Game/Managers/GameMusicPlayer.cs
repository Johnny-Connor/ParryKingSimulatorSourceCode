using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GameMusicPlayer : MonoBehaviour
{
    // Variables.
    [SerializeField] private List<AudioClip> _musicClips = new();
    [SerializeField] private float _delayBetweenTracks = 1;
    private AudioSource _audioSource;
    private readonly List<int> _playedIndices = new();
    private Timer _audioLengthTimer;


    // MonoBehaviour.
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioLengthTimer = new(PlayRandomMusic);

        PlayRandomMusic();
    }

    private void Update() => _audioLengthTimer.UpdateTimer();


    // Non-MonoBehaviour.
    private void PlayRandomMusic()
    {
        int randomIndex = GetRandomUnplayedIndex();
        if (randomIndex != -1)
        {
            _audioSource.clip = _musicClips[randomIndex];
            _audioLengthTimer.StartTimer(_musicClips[randomIndex].length + _delayBetweenTracks);
            _audioSource.Play();
            _playedIndices.Add(randomIndex);
        }
        else
        {
            ResetPlayedIndices();
            PlayRandomMusic();
        }
    }

    private int GetRandomUnplayedIndex()
    {
        List<int> unplayedIndices = new();
        for (int i = 0; i < _musicClips.Count; i++)
        {
            if (!_playedIndices.Contains(i)) unplayedIndices.Add(i);
        }

        if (unplayedIndices.Count > 0)
        {
            int randomIndex = Random.Range(0, unplayedIndices.Count);
            return unplayedIndices[randomIndex];
        }
        else
        {
            return -1; // All clips have been played.
        }
    }

    private void ResetPlayedIndices() => _playedIndices.Clear();
}
