using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    // float interval = 1.5f; // For basic enemy spawning
    [SerializeField] bool enableBasicSpawning = false;
    float basicSpawningTimer;
    public float borderY;
    public float borderYLower;

    [SerializeField] List<GameObject> enemyPrefabs;
    [SerializeField] GameObject enemyWarningPrefab;

    int wavesThisLevel = 0;
    float baseWaveStrength = 1f;
    float waveTimer = 0f;

    // Wave pacing
    [SerializeField] float minWaveInterval = 2.5f; // floor
    [SerializeField] float waveInterval = 15.0f; // starting interval
    [SerializeField] float intervalDecayPerLevel = 0.1f; // faster per level

    // Budget growth (enemy amount)
    [SerializeField] float budgetPerLevel = 1.0f; // add to budget each level
    [SerializeField] float budgetPerWave = 0.25f; // add to budget each wave this level

    // Round timing / Flee phase
    [SerializeField] float roundDuration = 40f; // enter flee phase at 40s
    [SerializeField] float fleeTickInterval = 1f; // roll frequency (each second)
    [SerializeField] float fleeChancePerTick = 0.25f; // 10% per tick
    [SerializeField] bool advanceRoundOnClear = true; // auto-advance when clear

    float roundTimer = 0f;
    float fleeTickTimer = 0f;
    bool inFleePhase = false;

    public bool InFleePhase => inFleePhase;


    int lastLevel = -1;

    // Exposed for UI
    public float WaveTimer => waveTimer;
    public float WaveInterval => waveInterval;

    void Start()
    {
        Camera cam = Camera.main;
        borderY = cam.orthographicSize;

        // keep a small border equal to half the height of the first enemy (if available)
        if (enemyPrefabs != null && enemyPrefabs.Count > 0)
        {
            var sr = enemyPrefabs[0].GetComponent<SpriteRenderer>();
            if (sr != null) borderY -= sr.bounds.size.y * 0.5f;
        }
    }

    void Update()
    {
        if (enableBasicSpawning)
        {
            //BasicSpawning();
            return;
        }

        int level = player_movement.gameRound;

        // Reset wave counter and phase per new round
        if (level != lastLevel)
        {
            wavesThisLevel = 0;
            lastLevel = level;

            roundTimer = 0f;
            fleeTickTimer = 0f;
            inFleePhase = false;
        }

        if (!inFleePhase)
        {
            // Normal spawning until flee phase
            roundTimer += Time.deltaTime;

            if (roundTimer >= roundDuration)
            {
                EnterFleePhase();
            }
            else
            {
                LevelSpawning(level);
            }
        }
        else
        {
            // Roll flee chance every tick
            fleeTickTimer -= Time.deltaTime;
            if (fleeTickTimer <= 0f)
            {
                fleeTickTimer = fleeTickInterval;
                FleeTick();
            }

            // End the round once everything is gone and no warnings remain
            if (NoEnemiesOrWarningsRemain())
            {
                EndRound();
            }
        }
    }


    void LevelSpawning(int level)
    {
        if (inFleePhase) return; // stop new spawns once fleeing starts

        if (waveTimer < 0f)
        {
            SpawnWave(level);

            // shrink interval as level rises, but never below minWaveInterval
            float lvlInterval = Mathf.Max(minWaveInterval, waveInterval - ((level - 1) * intervalDecayPerLevel));
            waveTimer = lvlInterval;

            wavesThisLevel++;
        }
        else
        {
            waveTimer -= Time.deltaTime;
        }
    }

    void SpawnWave(int level)
    {
        // points grow by level and by waves within the level
        float pointsThisWave = baseWaveStrength + ((level - 1) * budgetPerLevel) + (budgetPerWave);
        float pointsUsed = 0f;

        int safety = 0;
        while (pointsUsed < pointsThisWave && safety++ < 1000)
        {
            // pick an enemy by fixed weight (no scaling), only those unlocked for this level
            GameObject enemy = GetWeightedEnemyFixed(level);
            if (enemy == null) break;

            Enemy enemyComp = enemy.GetComponent<Enemy>();
            if (enemyComp == null) break;

            float enemyValue = enemyComp.spawnCost;
            if (enemyValue <= 0f) throw new System.Exception("Enemy with unassigned or zero spawn cost");

            float remaining = pointsThisWave - pointsUsed;

            // if nothing can fit, bail out cleanly
            if (!AnyEnemyFits(level, remaining)) break;

            // only spawn if it fits within remaining budget
            if (enemyValue <= remaining)
            {
                float y = Random.Range(borderYLower, borderY);
                float finalX = Random.Range(6.25f, 8.25f);

                Vector3 finalPos = new Vector3(finalX, y, 0f);
                GameObject warning = Instantiate(enemyWarningPrefab, finalPos, Quaternion.identity);
                warning.GetComponent<EnemySpawnWarning>().enemyToSpawn = enemy;

                pointsUsed += enemyValue;
            }
        }
    }

    GameObject GetWeightedEnemyFixed(int level)
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0) return null;

        // sum weights of unlocked enemies
        float total = 0f;
        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            var e = enemyPrefabs[i];
            var comp = e ? e.GetComponent<Enemy>() : null;
            if (comp == null) continue;
            if (comp.unlockRound > level) continue;
            if (comp.spawnWeight <= 0f) continue;

            total += comp.spawnWeight;
        }
        if (total <= 0f) return null;

        float pick = Random.Range(0f, total);
        float accum = 0f;

        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            var e = enemyPrefabs[i];
            var comp = e ? e.GetComponent<Enemy>() : null;
            if (comp == null) continue;
            if (comp.unlockRound > level) continue;
            if (comp.spawnWeight <= 0f) continue;

            accum += comp.spawnWeight;
            if (pick <= accum)
                return e;
        }
        return null;
    }

    bool AnyEnemyFits(int level, float budgetRemaining)
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0) return false;
        if (budgetRemaining <= 0f) return false;

        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            var prefab = enemyPrefabs[i];
            if (!prefab) continue;

            var comp = prefab.GetComponent<Enemy>();
            if (comp == null) continue;
            if (comp.unlockRound > level) continue;

            float cost = comp.spawnCost;
            if (cost > 0f && cost <= budgetRemaining)
                return true;
        }
        return false;
    }

    void EnterFleePhase()
    {
        inFleePhase = true;

        // Stop any queued spawns so nothing new appears during the flee phase
        var warnings = FindObjectsOfType<EnemySpawnWarning>();
        for (int i = 0; i < warnings.Length; i++)
        {
            if (warnings[i]) Destroy(warnings[i].gameObject);
        }

        // Start ticking immediately
        fleeTickTimer = 0f;
    }

    void FleeTick()
    {
        var enemies = FindObjectsOfType<Enemy>();
        for (int i = 0; i < enemies.Length; i++)
        {
            var e = enemies[i];
            if (!e || !e.isActiveAndEnabled) continue;

            if (Random.value < fleeChancePerTick)
            {
                e.BeginFlee(); // no credits
            }
        }
    }

    bool NoEnemiesOrWarningsRemain()
    {
        return FindObjectsOfType<Enemy>().Length == 0
            && FindObjectsOfType<EnemySpawnWarning>().Length == 0;
    }

    void EndRound()
    {
        // Reset phase state for next round
        inFleePhase = false;
        roundTimer = 0f;
        fleeTickTimer = 0f;
        wavesThisLevel = 0;
    }

    public void EnterFleePhaseExternal()
    {
        if (!inFleePhase)
            EnterFleePhase();
    }

    public bool IsFieldClear()
    {
        return NoEnemiesOrWarningsRemain();
    }

    /*
    void BasicSpawning()
    {
        basicSpawningTimer += Time.deltaTime;
        if (basicSpawningTimer >= interval)
        {
            basicSpawningTimer = 0f;
            float y = Random.Range(-borderY, borderY);
            float finalX = Random.Range(6.25f, 8.25f);
            GameObject enemy = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            Instantiate(enemy, new Vector3(finalX, y, 0f), Quaternion.identity);
        }
    }
    */
}
