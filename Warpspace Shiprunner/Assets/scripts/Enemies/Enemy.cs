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

    [SerializeField] private float fleeSpeed = 6f;     // how fast they slide off
    [SerializeField] private float fleeDistance = 4f;  // how far to move right before despawn

    private bool isFleeing = false;
    private float fleeTargetX = 0f;


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
        if (isFleeing) { FleeUpdate(); return; }  // <- short-circuit to flee
        Movement();

        if (health <= 0)
        {
            playerMovement.TriggerKill(this);
            Destroy(gameObject);
        }
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

    public void BeginFlee()
    {
        if (isFleeing) return;           // only once
        isFleeing = true;

        // stop firing
        if (_loop != null) StopCoroutine(_loop);

        // make bullets not hurt: easiest is to disable collider
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        // set flee target
        fleeTargetX = transform.position.x + fleeDistance;

        // optional: visual cue (tint, flash, trail, etc.)
        // GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.75f);
    }

    public void ChangeHealth(float healthChange)
    {
        if (isFleeing && healthChange < 0) return; // invulnerable while fleeing
        health += healthChange;
    }

    private void FleeUpdate()
    {
        Vector3 p = transform.position;
        float step = fleeSpeed * Time.deltaTime;
        p.x = Mathf.MoveTowards(p.x, fleeTargetX, step);
        transform.position = p;

        if (Mathf.Approximately(p.x, fleeTargetX))
        {
            Destroy(gameObject); // finally disappear
        }
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
