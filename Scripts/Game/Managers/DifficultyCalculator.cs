using System;
using UnityEngine;

public class DifficultyCalculator : MonoBehaviour
{
    // Singleton.
    public static DifficultyCalculator Instance { get; private set; }

    // Constants.
    private const int FIRST_DIFFICULTY_LEVEL_INDEX = 1;

    // Variables.
    [SerializeField] private AnimationCurve _difficultyLevelPerKillsRequiredToAdvanceAnimationCurve;
    [SerializeField] private int _difficultyLevels = 5;
    public string DifficultyLevelsToString => _difficultyLevels.ToString();
    private int _killCount;

    // Properties.
    private int DifficultyLevel => Mathf.Clamp(
        Mathf.RoundToInt(_difficultyLevelPerKillsRequiredToAdvanceAnimationCurve.Evaluate(_killCount)),
        FIRST_DIFFICULTY_LEVEL_INDEX,
        _difficultyLevels - 1 + FIRST_DIFFICULTY_LEVEL_INDEX);

    public string DifficultyLevelToString => DifficultyLevel.ToString();

    public float NormalizedDifficultyLevel => 
        Mathf.Clamp01( ( (float) DifficultyLevel - FIRST_DIFFICULTY_LEVEL_INDEX ) / 
                    (_difficultyLevels - FIRST_DIFFICULTY_LEVEL_INDEX) );


    // Events.
    public event Action OnDifficultyUpdated;


    // MonoBehaviour.
    private void Awake()
    {
        // Checks singleton instance.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        CharacterStats.OnAnyEnemyDeath += CharacterStats_OnAnyEnemyDeath;
    }


    // Event Handlers.
    private void CharacterStats_OnAnyEnemyDeath()
    {
        int oldDifficultyLevel = DifficultyLevel;

        _killCount++;

        if (oldDifficultyLevel != DifficultyLevel) OnDifficultyUpdated?.Invoke();
    }
}
