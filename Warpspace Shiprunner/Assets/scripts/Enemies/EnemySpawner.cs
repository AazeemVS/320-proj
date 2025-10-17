using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] float xSpawn = 7.5f;
    [SerializeField] float interval = 1.5f;
    [SerializeField] bool enableBasicSpawning = false;
    float basicSpawningTimer;
    float borderY;
    [SerializeField] List<GameObject> enemyPrefabs;
    [SerializeField] GameObject enemyWarningPrefab;

    int wavesThisLevel = 0;
    float baseWaveStrength = 5;
    [SerializeField] float waveInterval = 5f;
    float waveTimer = 0;
    
    private void Start() {
        Camera cam = Camera.main;
        borderY = cam.orthographicSize;
        borderY -= enemyPrefabs[0].GetComponent<SpriteRenderer>().bounds.size.y / 2;
    }

    void Update()
    {
        if (enableBasicSpawning) { BasicSpawning(); }
        else { LevelSpawning(); };
    }

    void LevelSpawning(int level = 1, float difficulty = 1) {
        if(waveTimer < 0) {
            SpawnWave(level, difficulty);
            waveTimer = waveInterval;
            wavesThisLevel++;
        } else {
            waveTimer -= Time.deltaTime;
        }
    }

    void SpawnWave(int level, float difficulty) {
        //completely arbitrary formula, will need a balance pass later
        float pointsThisWave = baseWaveStrength + wavesThisLevel + (level * difficulty);
        float pointsUsed = 0;
        //currently allows an enemy with high points to overflow the point limit, which adds some unpredictability to spawn patterns
        while (pointsUsed < pointsThisWave) {
            float y = Random.Range(-borderY, borderY);
            GameObject enemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            float enemyValue = enemy.GetComponent<Enemy>().spawnWeight;
            //prevent an enemy larger than the point allowance to spawn, allows for cases where a particularly dangerous enemy can only spawn in the later waves of a level
            if (enemyValue <= pointsThisWave) {
                Instantiate(enemyWarningPrefab, new Vector3(xSpawn, y, 0f), Quaternion.identity);
                enemyWarningPrefab.GetComponent<EnemySpawnWarning>().enemyToSpawn = enemy;
                pointsUsed += enemyValue;
            }
            //avoid crashes if an enemy accidently didnt get assigned a value
            if (enemyValue == 0) { throw new System.Exception("Enemy with unassigned spawn weight"); }
        }
    }

    void BasicSpawning() {
        basicSpawningTimer += Time.deltaTime;
        if (basicSpawningTimer >= interval) {
            basicSpawningTimer = 0f;
            float y = Random.Range(-borderY, borderY);
            GameObject enemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            Instantiate(enemy, new Vector3(xSpawn, y, 0f), Quaternion.identity);
        }
    }
}
