using TMPro;
using UnityEngine;

public class MatchInfoTextsController : MonoBehaviour
{
    // Variables.
    [SerializeField] private TMP_Text _enemySkillText;
    [SerializeField] private TMP_Text _killCountText;
    private int _killCount;


    // MonoBehaviour.
    private void Awake() => UpdateKillCountText();

    private void Start()
    {
        if (DifficultyCalculator.Instance)
        {
            UpdateEnemySkillText();

            DifficultyCalculator.Instance.OnDifficultyUpdated += DifficultyCalculator_OnDifficultyUpdated;
        }
        else
        {
            _enemySkillText.text = "Enemy Skill: N/A";
        }

        CharacterStats.OnAnyEnemyDeath += CharacterStats_OnAnyEnemyDeath;
    }


    // Non-MonoBehaviour.
    private void UpdateEnemySkillText()
    {
        _enemySkillText.text = 
            "Enemy Skill: " + 
            DifficultyCalculator.Instance.DifficultyLevelToString + 
            "/" + 
            DifficultyCalculator.Instance.DifficultyLevelsToString;
    }

    private void UpdateKillCountText()
    {
        _killCountText.text = "Kill Count:     " + _killCount.ToString();
    }


    // Event Handlers.
    private void CharacterStats_OnAnyEnemyDeath()
    {
        _killCount++;
        UpdateKillCountText();
    }

    private void DifficultyCalculator_OnDifficultyUpdated() => UpdateEnemySkillText();
}
