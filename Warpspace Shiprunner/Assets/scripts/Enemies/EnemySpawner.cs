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
    float baseWaveStrength = 4;
    float waveTimer = 0;

    // --- Wave pacing ---
    [SerializeField] float minWaveInterval = 2.5f; // floor
    [SerializeField] float waveInterval = 10.0f; // Starting wave interval
    [SerializeField] float intervalDecayPerLevel = 0.15f; // how much faster per level

    // --- Budget growth (enemy amount) ---
    [SerializeField] float budgetPerLevel = 1.0f; // add to budget each level
    [SerializeField] float budgetPerWave = 0.5f; // add to budget each wave this level

    // --- Weight scaling ---
    [Header("Weight Scaling (by base weight)")]
    [SerializeField] float basicMin = 0.4f;   // Min weight of easy enemy
    [SerializeField] float movingMax = 1.5f; //  Max weight of medium enemy
    [SerializeField] float aimingMax = 1.6f; //  Max weight of medium enemy
    [SerializeField] float chasingMax = 2.0f; //  Max weight of hard enemy
    [SerializeField] float basicDeltaPerLevel = -0.04f; // Basic enemy gets rarer
    [SerializeField] float movingPerLevel = 0.03f; // Moving enemy gets more likely
    [SerializeField] float aimingDeltaPerLevel = 0.04f; // Aiming enemy gets more likely
    [SerializeField] float chasingDeltaPerLevel = 0.06f; // Chasing enemy gets even more likely

    int lastLevel = -1;

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

            // Resets wave counter each new round
            if (level != lastLevel)
            {
                wavesThisLevel = 0;
                lastLevel = level;
            }

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
            GameObject enemy = GetWeightedEnemy(level);
            if (enemy == null) break; // Prevents breaks
            float enemyValue = enemy.GetComponent<Enemy>().spawnWeight;
            float remaining = pointsThisWave - pointsUsed;

            // Adds cheapest float equal to cheapest enemy weight
            float cheapest = Mathf.Infinity;
            foreach (GameObject e in enemyPrefabs)
            {
                Enemy enemy1 = e.GetComponent<Enemy>();
                if (enemy1 == null) continue;
                if (enemy1.unlockRound > level) continue;
                if (enemy1.spawnWeight < cheapest) cheapest = enemy1.spawnWeight;
            }
            if (cheapest == Mathf.Infinity) break;

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

    GameObject GetWeightedEnemy(int level)
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0) return null;

        // sum adjusted weights
        float totalWeight = 0f;
        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            Enemy enemy = enemyPrefabs[i].GetComponent<Enemy>();
            if (enemy == null) continue;

            // Skips if enemy hasn't been unlocked yet
            if (enemy.unlockRound > level) continue;

            totalWeight += GetAdjustedWeight(enemy, level);
        }

        // Safeguard
        if (totalWeight <= 0f) return null;

        float rng = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (GameObject e in enemyPrefabs)
        {
            Enemy enemy1 = e.GetComponent<Enemy>();
            if (enemy1 == null) continue;

            // Skips if enemy hasn't been unlocked eyt
            if (enemy1.unlockRound > level) continue;

            cumulative += GetAdjustedWeight(enemy1, level);
            if (rng <= cumulative)
                return e;
        }

        return null;
    }

    float GetAdjustedWeight(Enemy comp, int level)
    {
        int L = Mathf.Max(0, level - 1);
        float w = comp.spawnWeight;

        // Figure out which type it is based on weight range
        float delta = 0f;

        if (w <= basicMin)
            delta = basicDeltaPerLevel;       // basic enemies get rarer
        else if (w <= movingMax)
            delta = movingPerLevel;           // moving enemies more likely
        else if (w <= aimingMax)
            delta = aimingDeltaPerLevel;      // aiming enemies more likely
        else if (w <= chasingMax)
            delta = chasingDeltaPerLevel;     // chasing enemies more likely

        return Mathf.Clamp(w + delta * L, basicMin, chasingMax);
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
