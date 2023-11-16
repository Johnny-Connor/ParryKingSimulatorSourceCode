using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // Variables.
    [Header("Concurrent Enemies")]
    [SerializeField] private int _startingConcurrentEnemiesLimit = 1;
    [SerializeField] private int _endingConcurrentEnemiesLimit = 5;
    
    [Header("Respawn Time")]
    [SerializeField] private float _startingRespawnTime = 0f;
    [SerializeField] private float _endingRespawnTime = 10f;

    [Header("References")]
    [SerializeField] private List<Transform> _spawnPoints;
    [SerializeField] private Transform _enemyContainer;
    [SerializeField] private Transform _enemyPrefab;

    private Timer _spawnTimer;
    private int _currentEnemyCount;

    // Properties.
    private int ConcurrentEnemiesLimit
    {
        get
        {
            if (DifficultyCalculator.Instance)
            {
                return Mathf.RoundToInt(
                        Mathf.Lerp(
                            _startingConcurrentEnemiesLimit,
                            _endingConcurrentEnemiesLimit,
                            DifficultyCalculator.Instance.NormalizedDifficultyLevel));
            }
            else
            {
                return _startingConcurrentEnemiesLimit;
            }
        }
    }

    private float SpawnTimeInterval
    {
        get
        {
            if (DifficultyCalculator.Instance)
            {
                return Mathf.Lerp(
                    _startingRespawnTime, 
                    _endingRespawnTime, 
                    DifficultyCalculator.Instance.NormalizedDifficultyLevel);
            }
            else
            {
                return _startingRespawnTime;
            }
        }
    }


    // MonoBehaviour.
    private void Awake()
    {
        _spawnTimer = new(SpawnTimerCallback);
        _spawnTimer.StartTimer(SpawnTimeInterval);

        CharacterStats.OnAnyEnemyDeath += () => _currentEnemyCount--;
    }

    private void Update()
    {
        if (_currentEnemyCount < ConcurrentEnemiesLimit) _spawnTimer.UpdateTimer();
    }


    // Non-MonoBehaviour.
    private void SpawnEnemy()
    {
        int randomIndex = Random.Range(0, _spawnPoints.Count);
        Vector3 randomSpawnPoint = _spawnPoints[randomIndex].position;

        Instantiate(_enemyPrefab, randomSpawnPoint, Quaternion.identity, _enemyContainer);

        _currentEnemyCount++;
    }

    private void SpawnTimerCallback()
    {
        SpawnEnemy();

        _spawnTimer.StartTimer(SpawnTimeInterval);
    }
}
