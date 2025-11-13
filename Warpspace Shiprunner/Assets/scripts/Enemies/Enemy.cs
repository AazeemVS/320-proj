using System.Collections;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [Header("Firing")]
    [SerializeField] protected SimplePool bulletPool;
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected float shotsPerSecond = 2f;
    [SerializeField] protected float bulletSpeed = 10f;
    [SerializeField] protected float warmupDelay = 0.25f;
    [SerializeField] protected bool useLocalDown = false;
    [SerializeField] protected float health = 3;
    [SerializeField] public float spawnCost;
    [SerializeField] public float spawnWeight;
    [SerializeField] public int unlockRound;
    private float poisonTimer, poisonDamage;

    [SerializeField] private float fleeSpeed = 6f; // how fast they slide off
    [SerializeField] private float fleeDistance = 4f; // how far to move right before despawn

    [SerializeField] private int creditsOnKill = 1;
    public int CreditsOnKill => creditsOnKill;

    private bool isFleeing = false;
    private float fleeTargetX = 0f;
    private bool hasDied = false;

    Coroutine _loop;
    public player_movement playerMovement;
    protected float borderX;
    protected float borderY;

    public void Init(SimplePool pool)
    {
        bulletPool = pool;
        EnsureFirePoint();
        RestartFireLoop();
    }

    void Awake()
    {
        if (bulletPool == null) bulletPool = FindAnyObjectByType<SimplePool>();

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
        if (isFleeing) { FleeUpdate(); return; }

        Movement();


        ManageStatuses();
        if (health <= 0)
        {
            Kill(true);
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
            var t = transform.Find("firePoint");
            if (t != null) { firePoint = t; return; }

            firePoint = new GameObject("firePoint").transform;
            firePoint.SetParent(transform);
            firePoint.localPosition = Vector3.down * 0.5f;
        }
    }

    public void BeginFlee()
    {
        if (isFleeing) return;
        isFleeing = true;

        if (_loop != null) StopCoroutine(_loop);

        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        fleeTargetX = transform.position.x + fleeDistance;
    }

    public void ChangeHealth(float healthChange)
    {
        if (isFleeing && healthChange < 0) return;
        health += healthChange;
        if (health <= 0 && !hasDied)
        {
            Kill(true);
        }
    }

    public void Kill(bool awardCredits)
    {
        if (hasDied) return;
        hasDied = true;

        if (awardCredits && playerMovement != null)
        {
            // playerMovement.AddCredits(creditsOnKill); gave double ecredits for some reason
            playerMovement.TriggerKill(this);
        }

        Destroy(gameObject);
    }

    private void FleeUpdate()
    {
        Vector3 p = transform.position;
        float step = fleeSpeed * Time.deltaTime;
        p.x = Mathf.MoveTowards(p.x, fleeTargetX, step);
        transform.position = p;

        if (Mathf.Approximately(p.x, fleeTargetX))
        {
            Kill(false);
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
