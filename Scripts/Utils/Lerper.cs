/*
A class that uses Mathf.Lerp the way it was intended to be used, guaranteeing a value transition over a
given time without slowing down exponentialy as it approaches the end, as often seen in many
implementations.
*/

using System;
using UnityEngine;

public class Lerper
{
    // Variables.
    private float _initialValue;
    private float _endValue;
    private float _lerpDuration;
    private Action _lerperCallback;
    private float _timeElapsed = Mathf.Infinity;


    // Constructor.
    public Lerper(float initialValue, float endValue, float lerpDuration, Action lerperCallback = null)
    {
        _initialValue = initialValue;
        _endValue = endValue;
        _lerpDuration = lerpDuration;
        _lerperCallback = lerperCallback ?? (() => { /* Default action, do nothing. */ });
    }


    // Non-MonoBehaviour.
    public void StartLerper(bool canOverrideLerpCycle = false)
    {
        if (_timeElapsed < _lerpDuration)
        {
            Debug.LogWarning("Cannot override Lerp cycle because 'canOverrideLerpCycle' is set to false.");
            return;
        }

        _timeElapsed = 0;
    }

    public void UpdateLerper(ref float lerpValue)
    {
        if (_timeElapsed < _lerpDuration)
        {
            lerpValue = Mathf.Lerp(_initialValue, _endValue, _timeElapsed / _lerpDuration);
            _timeElapsed += Time.deltaTime;

            if (_timeElapsed >= _lerpDuration)
            {
                lerpValue = _endValue;
                _lerperCallback();
            }
        }
    }
}
