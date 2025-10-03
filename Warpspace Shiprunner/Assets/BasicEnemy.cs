using UnityEngine;

public class BasicEnemy : Enemy
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override void FireOnce() {
        if (bulletPool == null || firePoint == null) return;
        var go = bulletPool.Spawn(firePoint.position, Quaternion.Euler(0, 0, 270));
        var proj = go.GetComponent<Projectile>();
        if (proj == null) return;

        Vector2 dir = useLocalDown ? -(Vector2)transform.right : Vector2.left;
        proj.Fire(dir, bulletSpeed);
    }
}
