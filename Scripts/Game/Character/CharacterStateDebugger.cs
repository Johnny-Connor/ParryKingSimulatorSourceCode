using System;
using UnityEngine;

public class CharacterStateDebugger : MonoBehaviour
{
    [SerializeField] private ControlCharacter _controlCharacter;

    private void Start()
    {
        CharacterStateMachine _characterStateMachine = _controlCharacter.CharacterStateMachine;

        _characterStateMachine.OnEnteredState += CharacterStateMachine_OnEnteredState;
    }

    private void CharacterStateMachine_OnEnteredState(object sender, EventArgs e)
    {
        Debug.Log(sender.GetType());
    }
}
