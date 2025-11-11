using System.Collections;
using UnityEngine;

public class LaserEnemy : Enemy
{
    [Header("Damage")]
    [SerializeField] float laserDamage = 1f;
    [SerializeField] bool shotIgnoresIFrames = false;
    [Tooltip("How close the beam path must pass to the player to count as a hit.")]
    [SerializeField] float playerHitRadius = 0.45f;

    [Header("Timing")]
    [SerializeField] float preAimTime = 1.0f; // follow red duration (fades in)
    [SerializeField] float postAimDelay = 0.50f; // pause after locking aim
    [SerializeField] float whiteFlashDuration = 0.03f; // super short impact frame
    [SerializeField] float beamDuration = 0.15f; // how long orange stays on
    [SerializeField] float fireCooldown = 5.0f; // orange to orange

    [Header("Visuals")]
    [SerializeField] float aimWidth = 0.04f; // follow width
    [SerializeField] float beamWidth = 0.12f; // shot width
    [SerializeField] float range = 30f;

    // Colors
    static readonly Color followRedStart = new Color(1f, 0f, 0f, 0.10f);
    static readonly Color followRedEnd = new Color(1f, 0f, 0f, 1.00f);
    static readonly Color shotOrangeA = new Color(1f, 0.50f, 0.00f, 1f);
    static readonly Color shotOrangeB = new Color(1f, 0.60f, 0.20f, 1f);
    static readonly Color whiteFull = new Color(1f, 1f, 1f, 1f);

    LineRenderer lr;
    Vector3 startPos;
    float hoverT;

    bool isFiring = false;
    float lastShotTime = -999f;

    void Start()
    {
        startPos = transform.position;

        lr = gameObject.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.enabled = false;
        lr.sortingOrder = 10;
        lr.numCapVertices = 4;
        lr.material = new Material(Shader.Find("Sprites/Default"));
    }

    public override void Movement()
    {
        // subtle hover
        hoverT += Time.deltaTime * 1.25f;

        float hoverAmp = 0.75f;
        Vector3 expected = startPos + new Vector3(0f, Mathf.Sin(hoverT) * hoverAmp, 0f);
        if ((transform.position - expected).sqrMagnitude > 0.01f)
        {
            // Preserve current hover offset and anchor to wherever we were moved.
            startPos = transform.position - new Vector3(0f, Mathf.Sin(hoverT) * hoverAmp, 0f);
        }

        var p = startPos;
        p.y += Mathf.Sin(hoverT) * hoverAmp;
        transform.position = p;
    }

    public override void FireOnce()
    {
        if (!playerMovement || !firePoint) return;
        if (isFiring) return;
        if (Time.time - lastShotTime < fireCooldown) return;

        StartCoroutine(FireLaserRoutine());
    }

    IEnumerator FireLaserRoutine()
    {
        isFiring = true;

        // follow red 0-100% opacity
        lr.enabled = true;
        lr.startWidth = aimWidth;
        lr.endWidth = aimWidth;

        float t = 0f;
        Vector3 dirFallback = Vector3.right;

        while (t < preAimTime)
        {
            t += Time.deltaTime;

            Vector3 d = (playerMovement.transform.position - firePoint.position).normalized;
            if (d.sqrMagnitude < 0.0001f) d = dirFallback;

            float k = Mathf.Clamp01(t / Mathf.Max(0.0001f, preAimTime));
            float a = Mathf.Lerp(followRedStart.a, followRedEnd.a, k);

            lr.startColor = new Color(1f, 0f, 0f, a);
            lr.endColor = new Color(1f, 0.2f, 0.2f, a);
            lr.SetPosition(0, firePoint.position);
            lr.SetPosition(1, firePoint.position + d * range);
            yield return null;
        }

        // lock and pause
        Vector3 lockedDir = (playerMovement.transform.position - firePoint.position).normalized;
        if (lockedDir.sqrMagnitude < 0.0001f) lockedDir = dirFallback;

        lr.startWidth = aimWidth;
        lr.endWidth = aimWidth;
        lr.startColor = followRedEnd;
        lr.endColor = new Color(1f, 0.2f, 0.2f, 1f);
        lr.SetPosition(0, firePoint.position);
        lr.SetPosition(1, firePoint.position + lockedDir * range);

        yield return new WaitForSeconds(postAimDelay);

        // Precompute the shot segment
        Vector3 shotStart = firePoint.position + lockedDir * 0.05f; // tiny nudge out of own collider
        Vector3 shotEnd = shotStart + lockedDir * range;

        // Impact frame
        lr.startWidth = beamWidth;
        lr.endWidth = beamWidth;
        lr.startColor = whiteFull;
        lr.endColor = whiteFull;
        lr.SetPosition(0, firePoint.position);
        lr.SetPosition(1, shotEnd);

        yield return new WaitForSeconds(whiteFlashDuration);

        // Orange shot
        // Visuals first
        lr.startWidth = beamWidth;
        lr.endWidth = beamWidth;
        lr.startColor = shotOrangeA;
        lr.endColor = shotOrangeB;

        // If the shortest distance from player's position to the beam segment radiushit.
        Vector2 playerPos = playerMovement.transform.position;
        float dist = DistancePointToSegment2D(playerPos, shotStart, shotEnd);

        Vector3 finalEnd = shotEnd;
        if (dist <= playerHitRadius && laserDamage > 0f)
        {
            // Damage respects iframes unless shotIgnoresIFrames is true
            playerMovement.ChangeHealth(-laserDamage, shotIgnoresIFrames);

            // Move the visual end to the closest point on the segment to the player
            finalEnd = ClosestPointOnSegment2D(playerPos, shotStart, shotEnd);
        }

        lr.SetPosition(0, firePoint.position);
        lr.SetPosition(1, finalEnd);

        lastShotTime = Time.time;

        yield return new WaitForSeconds(beamDuration);

        lr.enabled = false;
        isFiring = false;
    }

    static float DistancePointToSegment2D(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float ab2 = Vector2.SqrMagnitude(ab);
        if (ab2 <= 1e-8f) return Vector2.Distance(p, a); // degenerate

        float t = Vector2.Dot(p - a, ab) / ab2;
        t = Mathf.Clamp01(t);
        Vector2 proj = a + t * ab;
        return Vector2.Distance(p, proj);
    }

    // Closest point on segment AB to P
    static Vector2 ClosestPointOnSegment2D(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float ab2 = Vector2.SqrMagnitude(ab);
        if (ab2 <= 1e-8f) return a;
        float t = Vector2.Dot(p - a, ab) / ab2;
        t = Mathf.Clamp01(t);
        return a + t * ab;
    }
}
