using System.Collections;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [Header("Firing")]
    [SerializeField] protected SimplePool bulletPool;  // ok if left empty on prefab
    [SerializeField] protected Transform firePoint;    // ok if missing; we'll create one
    [SerializeField] protected float shotsPerSecond = 2f;
    [SerializeField] protected float bulletSpeed = 10f;
    [SerializeField] protected float warmupDelay = 0.25f;
    [SerializeField] protected bool useLocalDown = false;
    [SerializeField] protected float health = 3;
    [SerializeField] public float spawnCost;
    [SerializeField] public float spawnWeight;
    [SerializeField] public int unlockRound;
    private float poisonTimer, poisonDamage;

    Coroutine _loop;
    public player_movement playerMovement;
    protected float borderX;
    protected float borderY;

    // Allow spawner to inject dependencies if you want
    public void Init(SimplePool pool)
    {
        bulletPool = pool;
        EnsureFirePoint();
        RestartFireLoop();
    }

    void Awake()
    {
        // Fallbacks so spawned copies work without manual wiring
        if (bulletPool == null) bulletPool = FindAnyObjectByType<SimplePool>();

        // Finds playerMovement script
        if (playerMovement == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerMovement = player.GetComponent<player_movement>();
            }
            else
            {
                Debug.LogWarning("Player GameObject not found! Make sure it's tagged 'Player'");
            }
        }

        EnsureFirePoint();
        Camera cam = Camera.main;
        borderY = cam.orthographicSize;
        borderX = borderY * cam.aspect;
        borderX -= GetComponent<SpriteRenderer>().bounds.size.x / 2;
        borderY -= GetComponent<SpriteRenderer>().bounds.size.y / 2;
    }

    protected virtual void Update()
    {
        Movement();
        ManageStatuses();
        if (health <= 0)
        {
            // Gives money
            playerMovement.TriggerKill(this);
            // Kills enemy
            Destroy(gameObject);
        }
    }
    public void ChangeHealth(float healthChange) {
        health += healthChange;
    }

    void OnEnable() => RestartFireLoop();
    void OnDisable() { if (_loop != null) StopCoroutine(_loop); }

    void RestartFireLoop()
    {
        if (_loop != null) StopCoroutine(_loop);
        if (isActiveAndEnabled) _loop = StartCoroutine(FireLoop());
    }

    void EnsureFirePoint()
    {
        if (firePoint == null)
        {
            // Try find existing child named "firePoint"
            var t = transform.Find("firePoint");
            if (t != null) { firePoint = t; return; }

            // Otherwise create one
            firePoint = new GameObject("firePoint").transform;
            firePoint.SetParent(transform);
            firePoint.localPosition = Vector3.down * 0.5f;
        }
    }

    void ManageStatuses() {
        if (poisonTimer >= 0) {
            poisonTimer -= Time.deltaTime;
            ChangeHealth(-poisonDamage * Time.deltaTime);
        }
    }

    public void EnablePoison(float length, float DPS) {
        poisonTimer = length;
        poisonDamage = DPS;
    }

    IEnumerator FireLoop()
    {
        yield return new WaitForSeconds(warmupDelay);
        var wait = new WaitForSeconds(1f / Mathf.Max(0.01f, shotsPerSecond));
        while (true)
        {
            FireOnce();
            yield return wait;
        }
    }

    public abstract void Movement();
    public abstract void FireOnce();
}
