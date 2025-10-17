using UnityEngine;

public class BasicAimingEnemy : Enemy
{
    private GameObject player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = FindAnyObjectByType<player_movement>().gameObject;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override void FireOnce() {
        Vector2 dir = player.transform.position - transform.position;
        var go = bulletPool.Spawn(firePoint.position, Quaternion.Euler(0, 0, 90 + (Mathf.Atan2(dir.y, dir.x)*180/Mathf.PI)));
        var proj = go.GetComponent<Projectile>();
        go.GetComponent<SpriteRenderer>().color = Color.green;
        if (proj == null) return;

        
        proj.Fire(dir, bulletSpeed);
    }

    public override void Movement() {
    }
}
