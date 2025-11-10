using System.Collections;
using UnityEngine;

public class LaserEnemy : Enemy
{
    [Header("Laser Settings")]
    [SerializeField] float laserDamage = 1f;

    [Tooltip("How long the blue 'aim' line shows before firing the red beam.")]
    [SerializeField] float preAimTime = 1.0f;     // blue warning duration

    [Tooltip("How long the red beam is visible after firing.")]
    [SerializeField] float beamDuration = 0.15f;  // red beam duration

    [Tooltip("Time between RED beam shots (from red to next red).")]
    [SerializeField] float fireCooldown = 5.0f;   // cadence target (~5s)

    [SerializeField] float aimWidth = 0.04f;      // blue line width
    [SerializeField] float beamWidth = 0.12f;     // red line width
    [SerializeField] float range = 30f;

    [Tooltip("Layers the laser can hit (make sure Player layer is included).")]
    [SerializeField] LayerMask hitMask = ~0;

    [Header("Movement")]
    [SerializeField] float hoverSpeed = 1.25f;
    [SerializeField] float hoverAmplitude = 0.75f;

    private LineRenderer lr;
    private float hoverT;
    private Vector3 startPos;

    // Cooldown gating so we don't rely on base shotsPerSecond timing
    private bool isFiring = false;
    private float lastRedTime = -999f;

    void Start()
    {
        startPos = transform.position;

        // Set up a LineRenderer for the laser
        lr = gameObject.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.enabled = false;
        lr.sortingOrder = 10;

        // Simple material for 2D line
        var mat = new Material(Shader.Find("Sprites/Default"));
        lr.material = mat;
    }

    public override void Movement()
    {
        // Simple floaty hover
        hoverT += Time.deltaTime * hoverSpeed;
        Vector3 p = startPos;
        p.y += Mathf.Sin(hoverT) * hoverAmplitude;
        transform.position = p;
    }

    public override void FireOnce()
    {
        if (!playerMovement || !firePoint) return;

        // Gate by cooldown, measured at the moment the RED beam fires
        if (isFiring) return;
        if (Time.time - lastRedTime < fireCooldown) return;

        StartCoroutine(FireLaserRoutine());
    }

    IEnumerator FireLaserRoutine()
    {
        isFiring = true;

        // === BLUE WARNING PHASE (no damage) ===
        Vector3 dirWarn = (playerMovement.transform.position - firePoint.position).normalized;
        if (dirWarn.sqrMagnitude < 0.0001f) dirWarn = Vector3.left;

        lr.enabled = true;
        lr.startWidth = aimWidth;
        lr.endWidth = aimWidth;
        lr.startColor = Color.cyan;                // blue aim color
        lr.endColor = new Color(0f, 0.7f, 1f);

        float timer = 0f;
        while (timer < preAimTime)
        {
            timer += Time.deltaTime;
            Vector3 d = (playerMovement.transform.position - firePoint.position).normalized;
            if (d.sqrMagnitude < 0.0001f) d = dirWarn;

            lr.SetPosition(0, firePoint.position);
            lr.SetPosition(1, firePoint.position + d * range);
            yield return null;
        }

        // === RED BEAM PHASE (does damage) ===
        Vector3 dir = (playerMovement.transform.position - firePoint.position).normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = dirWarn;

        // Nudge forward to avoid hitting our own collider
        Vector3 castStart = firePoint.position + dir * 0.05f;

        // Exclude this enemy's layer from the raycast so we don't self-hit
        int excludeSelfLayer = 1 << gameObject.layer;
        int mask = hitMask & ~excludeSelfLayer;

        RaycastHit2D hit = Physics2D.Raycast(castStart, dir, range, mask);
        Vector3 end = castStart + dir * range;

        if (hit.collider != null)
        {
            end = hit.point;

            // Damage player (even if collider is on a child)
            var pm = hit.collider.GetComponent<player_movement>() ??
                     hit.collider.GetComponentInParent<player_movement>();

            if (pm != null && laserDamage > 0f)
            {
                pm.ChangeHealth(-laserDamage);
            }
        }

        // Draw the RED beam
        lr.startWidth = beamWidth;
        lr.endWidth = beamWidth;
        lr.startColor = Color.red;
        lr.endColor = new Color(1f, 0.3f, 0.3f);
        lr.SetPosition(0, firePoint.position);
        lr.SetPosition(1, end);

        // Mark the time at which the RED beam fired (cadence reference)
        lastRedTime = Time.time;

        yield return new WaitForSeconds(beamDuration);

        lr.enabled = false;
        isFiring = false;
    }
}
