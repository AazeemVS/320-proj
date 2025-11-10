using UnityEngine;

public class TurretEnemy : Enemy
{
    [Header("Turret Settings")]

    // How fast the turret turns
    [SerializeField] private float rotateSpeedDegPerSec = 360f;
    // Size of the fan
    [SerializeField] private float arcDegrees = 180f;
    // Bullets per 180° shot
    [SerializeField] private int bulletsPerVolley = 15;
    [SerializeField] private float spriteForwardOffset = 90f;

    private Transform _player;

    void Start()
    {
        // cache player transform
        var pm = FindAnyObjectByType<player_movement>();
        if (pm) _player = pm.transform;
    }

    // Rotate to face the player
    public override void Movement()
    {
        if (_player == null) return;

        Vector2 toPlayer = _player.position - transform.position;
        float targetDeg = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg + spriteForwardOffset;

        // Smooth rotate toward the player
        float step = rotateSpeedDegPerSec * Time.deltaTime;
        Quaternion targetRot = Quaternion.Euler(0, 0, targetDeg);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, step);
    }

    public override void FireOnce()
    {
        if (bulletPool == null || firePoint == null) return;

        // Base angle = where the turret is facing now
        float baseDeg = transform.eulerAngles.z - spriteForwardOffset;
        float halfArc = arcDegrees * 0.5f;

        // Edge case: 1 bullet => just shoot straight
        if (bulletsPerVolley <= 1)
        {
            FireAtAngle(baseDeg);
            return;
        }

        float step = arcDegrees / (bulletsPerVolley - 1);
        float start = baseDeg - halfArc;

        // Fire across the whole 180 degree plane centered on the turret facing
        for (int i = 0; i < bulletsPerVolley; i++)
        {
            float ang = start + i * step;
            FireAtAngle(ang);
        }
    }

    private void FireAtAngle(float angleDeg)
    {
        // Direction from angle
        float rad = angleDeg * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        // Spawn and orient the bullet
        var rot = Quaternion.Euler(0, 0, angleDeg + 90f);
        var go = bulletPool.Spawn(firePoint.position, rot);

        // Color so you can spot turret shots
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr) sr.color = Color.yellow;

        // Move it
        var proj = go.GetComponent<Projectile>();
        if (proj != null) proj.Fire(dir, bulletSpeed);
    }
}
