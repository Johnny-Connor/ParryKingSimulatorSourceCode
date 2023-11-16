using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Variables.
    private TMP_Text _nameText;
    private static List<string> _inUseNames = new();


    // MonoBehaviour.
    private void Awake()
    {
        _nameText = GetComponent<TMP_Text>();
        AssignUniqueName();
    }

    private void OnDestroy()
    {
        string nameToRelease = _nameText.text;
        _inUseNames.Remove(nameToRelease);
    }


    // Non-MonoBehaviour.
    private void AssignUniqueName()
    {
        string randomName = GetRandomName();
        _nameText.text = randomName;
        _inUseNames.Add(randomName);
    }

    private string GetRandomName()
    {
        List<string> availableNames = GetAvailableNames();
        
        if (availableNames.Count == 0)
        {
            Debug.LogWarning("No available names left.");
            return "Wow, there are a lot of foes here!";
        }

        int randomIndex = Random.Range(0, availableNames.Count);
        string randomName = availableNames[randomIndex];
        return randomName;
    }

    private List<string> GetAvailableNames()
    {
        List<string> availableNames = new()
        {
            "AchievementHunter",
            "AnotherAnimeCosplayer",
            "Coopmancer",
            "Hatemailer",
            "MinMaxer",
            "NotACasul",
            "PraiseTheRoll",
            "R1Masher",
            "SupremeGanker",
            "TrollingYoutuber"
        };

        foreach (string beingUsedName in _inUseNames)
        {
            availableNames.Remove(beingUsedName);
        }

        return availableNames;
    }
}
