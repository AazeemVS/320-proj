using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Firing")]
    [SerializeField] SimplePool bulletPool;  // ok if left empty on prefab
    [SerializeField] Transform firePoint;    // ok if missing; we'll create one
    [SerializeField] float shotsPerSecond = 2f;
    [SerializeField] float bulletSpeed = 10f;
    [SerializeField] float warmupDelay = 0.25f;
    [SerializeField] bool useLocalDown = false;

    Coroutine _loop;

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
        if (bulletPool == null) bulletPool = FindObjectOfType<SimplePool>();
        EnsureFirePoint();
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

    void FireOnce()
    {
        if (bulletPool == null || firePoint == null) return;

        var go = bulletPool.Spawn(firePoint.position, Quaternion.Euler(0, 0, 270));
        var proj = go.GetComponent<Projectile>(); // or Projectile2D if that’s your script
        if (proj == null) return;

        Vector2 dir = useLocalDown ? -(Vector2)transform.right : Vector2.left;
        proj.Fire(dir, bulletSpeed);
    }
}
