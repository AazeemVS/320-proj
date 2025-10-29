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
    [SerializeField] float waveInterval = 6f; // Starting wave interval
    float waveTimer = 0;

    // --- Wave pacing ---
    [SerializeField] float minWaveInterval = 2.5f; // floor
    [SerializeField] float intervalDecayPerLevel = 0.15f; // how much faster per level

    // --- Budget growth (enemy amount) ---
    [SerializeField] float budgetPerLevel = 2.0f; // add to budget each level
    [SerializeField] float budgetPerWave = 1.0f; // add to budget each wave this level

    // --- Weight scaling (no struct) ---
    [Header("Weight Scaling (by base weight)")]
    [SerializeField] float easyDeltaPerLevel = -0.04f; // Easy gets rarer
    [SerializeField] float medDeltaPerLevel = 0.03f; // Medium a bit more likely
    [SerializeField] float hardDeltaPerLevel = 0.05f; // Hard more likely

    private void Start() {
        Camera cam = Camera.main;
        borderY = cam.orthographicSize;
        borderY -= enemyPrefabs[0].GetComponent<SpriteRenderer>().bounds.size.y / 2;
    }

    void Update()
    {
        if (enableBasicSpawning) { BasicSpawning(); }
        else { 
            int level = player_movement.gameRound; 
            LevelSpawning(level); 
        };
    }

    void LevelSpawning(int level) {
        if(waveTimer < 0) {
            SpawnWave(level);

            // shrink interval as level rises, but never below minWaveInterval
            float lvlInterval = Mathf.Max(minWaveInterval, waveInterval - ((level - 1) * intervalDecayPerLevel));
            waveTimer = lvlInterval;

            wavesThisLevel++;
        } else {
            waveTimer -= Time.deltaTime;
        }
    }

    void SpawnWave(int level) {
        //completely arbitrary formula, will need a balance pass later
        float pointsThisWave = baseWaveStrength + (level * budgetPerLevel) + (wavesThisLevel * budgetPerWave); // more points each round, and each wave within the round
        float pointsUsed = 0;

        int safety = 0;

        //currently allows an enemy with high points to overflow the point limit, which adds some unpredictability to spawn patterns
        while (pointsUsed < pointsThisWave && safety++ < 1000) {
            float y = Random.Range(-borderY, borderY);
            // Gets random enemy using weighting (Difficult enemies spawn less)
            GameObject enemy = GetWeightedEnemy();
            float enemyValue = enemy.GetComponent<Enemy>().spawnWeight;
            float remaining = pointsThisWave - pointsUsed;

            // Adds cheapest float equal to cheapest enemy weight
            float cheapest = Mathf.Infinity;
            foreach (GameObject e in enemyPrefabs)
            {
                Enemy enemy1 = e.GetComponent<Enemy>();
                if (enemy1 != null && enemy1.spawnWeight < cheapest)
                    cheapest = enemy1.spawnWeight;
            }
            // Breaks if no more enemies can fit
            if (remaining < cheapest)
                break;

            //prevent an enemy larger than the point allowance to spawn, allows for cases where a particularly dangerous enemy can only spawn in the later waves of a level
            if (enemyValue <= remaining) {
                Instantiate(enemyWarningPrefab, new Vector3(xSpawn, y, 0f), Quaternion.identity);
                enemyWarningPrefab.GetComponent<EnemySpawnWarning>().enemyToSpawn = enemy;
                pointsUsed += enemyValue;
            }
            //avoid crashes if an enemy accidently didnt get assigned a value
            if (enemyValue == 0) { throw new System.Exception("Enemy with unassigned spawn weight"); }
        }
    }

    GameObject GetWeightedEnemy()
    {
        float totalWeight = 0f;
        foreach (GameObject e in enemyPrefabs)
        {
            Enemy comp = e.GetComponent<Enemy>();
            if (comp != null) totalWeight += Mathf.Clamp(comp.spawnWeight, 0.01f, 10f);
        }

        float rng = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        // Loops through every enemy prefab
        foreach (GameObject e in enemyPrefabs)
        {
            // Get the Enemy component to access its weight
            Enemy enemy = e.GetComponent<Enemy>();

            // Skip this prefab if it doesn’t have an Enemy script
            if (enemy == null) continue;

            // Add this enemy's weight to the running total
            cumulative += Mathf.Clamp(enemy.spawnWeight, 0.01f, 10f);

            // If the random number falls within this enemys weight range, pick it
            if (rng <= cumulative)
                return e;
        }

        // fallback
        return enemyPrefabs[enemyPrefabs.Count - 1];
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
