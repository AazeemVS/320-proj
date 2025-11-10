using UnityEngine;

public class TankEnemy : Enemy
{
  [Header("Tank Movement")]
  [SerializeField] private float moveSpeed = 1.25f;
  [SerializeField] private float turnSpeedDeg = 120f;
  [SerializeField] private float stopRange = 4.0f;

  [Header("Tank Fire Settings")]
  [SerializeField] private float projectileScale = 1.8f;
  [SerializeField] private float projectileDamage = 4f;
  [SerializeField] private float inaccuracyDeg = 3f;

  private Transform player;

  void Start()
  {
    // Tank defaults: tougher, slow rate, beefy shots
    shotsPerSecond = Mathf.Min(shotsPerSecond, 0.5f);
    health = Mathf.Max(health, 12f);

    var pm = FindAnyObjectByType<player_movement>();
    if (pm) player = pm.transform;
  }

  // Slow advance + rotate to face player
  public override void Movement()
  {
    if (!player) return;

    Vector2 to = (Vector2)(player.position - transform.position);
    float dist = to.magnitude;
    Vector2 dir = (dist > 0.001f) ? to / dist : Vector2.right;

    // face the player
    float targetDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
    transform.rotation = Quaternion.RotateTowards(
        transform.rotation,
        Quaternion.Euler(0, 0, targetDeg),
        turnSpeedDeg * Time.deltaTime
    );

    // slow advance until within stopRange
    if (dist > stopRange)
    {
      transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
    }

    // keep inside camera borders computed by base Enemy
    var p = transform.position;
    p.x = Mathf.Clamp(p.x, -borderX, borderX);
    p.y = Mathf.Clamp(p.y, -borderY, borderY);
    transform.position = p;
  }

  public override void FireOnce()
  {
    if (bulletPool == null || firePoint == null) return;

    // aim at player
    Vector2 dir = player
        ? (Vector2)(player.position - firePoint.position).normalized
        : (Vector2)(-transform.up);

    // tiny spread for feel
    float baseDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    float spread = Random.Range(-inaccuracyDeg, inaccuracyDeg);
    float finalDeg = baseDeg + spread;

    // spawn / orient
    var rot = Quaternion.Euler(0, 0, finalDeg + 90f);
    var go = bulletPool.Spawn(firePoint.position, rot);

    // make it LOOK big & mean
    go.transform.localScale = Vector3.one * projectileScale;
    var sr = go.GetComponent<SpriteRenderer>();
    if (sr) sr.color = Color.red;

    // move it
    var proj = go.GetComponent<Projectile>();
    if (proj != null) proj.Fire(dir, bulletSpeed * 0.8f);

    // ensure bullet deals heavy damage
    var dmg = go.GetComponent<ProjectileDamage>();
    if (dmg == null) dmg = go.AddComponent<ProjectileDamage>();
    dmg.damage = projectileDamage;
    dmg.hitEnemy = false; // only hit player by default
  }
}
