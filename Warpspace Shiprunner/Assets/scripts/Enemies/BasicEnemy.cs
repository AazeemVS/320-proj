using UnityEngine;

public class BasicEnemy : Enemy
{
    [SerializeField] bool stationary;
    public float upperLimit;
    public float lowerLimit;
    private float totalRange = 3f;
    private float speed = 2f;
    private int direction = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (transform.position.y + (totalRange / 2) > borderY) {
            upperLimit = borderY;
            lowerLimit = borderY - totalRange;
        } else if (transform.position.y - (totalRange / 2) < -borderY) {
            upperLimit = -borderY + totalRange;
            lowerLimit = -borderY;
        } else {
            upperLimit = transform.position.y + totalRange / 2;
            lowerLimit = transform.position.y - totalRange / 2;
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override void FireOnce() {
        if (bulletPool == null || firePoint == null) return;
        var go = bulletPool.Spawn(firePoint.position, Quaternion.identity);
        var proj = go.GetComponent<Projectile>();
        if (proj == null) return;

        Vector2 dir = useLocalDown ? -(Vector2)transform.right : Vector2.left;
        proj.Fire(dir, bulletSpeed);
    }

    public override void Movement() {
        if(!stationary) {
            if (transform.position.y > upperLimit && direction == 1) {
                direction = -1;
            } else if (transform.position.y < lowerLimit && direction == -1){
                direction = 1;
            }
            transform.position = new Vector2(transform.position.x, transform.position.y + (Time.deltaTime*speed*direction));
        }
    }
}
