using UnityEngine;

public class SlowingEnemy : Enemy
{
    [Header("Slow Bullet Settings")]
    [SerializeField] private float slowPercent = 0.95f;
    [SerializeField] private float slowDuration = 2.0f;

    private GameObject player;

    void Start()
    {
        player = FindAnyObjectByType<player_movement>()?.gameObject;
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void FireOnce()
    {
        if (player == null) return;

        Vector2 dir = player.transform.position - transform.position;

        float angleDeg = 90f + Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        var go = bulletPool.Spawn(firePoint.position, Quaternion.Euler(0, 0, angleDeg));

        var sr = go.GetComponent<SpriteRenderer>();
        if (sr) sr.color = Color.cyan;

        var slowProj = go.GetComponent<SlowingProjectile>();
        if (slowProj == null) slowProj = go.AddComponent<SlowingProjectile>();
        slowProj.Configure(dir.normalized, bulletSpeed, slowPercent, slowDuration);

        var proj = go.GetComponent<Projectile>();
        if (proj != null) proj.Fire(dir, bulletSpeed);
    }

    public override void Movement() { }
}
