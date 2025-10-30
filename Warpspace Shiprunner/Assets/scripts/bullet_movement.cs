using UnityEngine;
using System.Collections.Generic;

public class Bullet : MonoBehaviour {

    public float speed = 10f;
    private float borderX;
    public float bulletDamage;
    public int piercing = 1;
    public List<Enemy> hitEnemies = new List<Enemy>();
    public bool explosive = false;
    [SerializeField] private GameObject Explosion;

    private void Start() {
        Camera cam = Camera.main;
        borderX = cam.orthographicSize * cam.aspect;
        borderX += GetComponent<SpriteRenderer>().bounds.size.x;
    }
    void Update()
    {
        //destroy bullet if it goes fully off screen
        if (transform.position.x > borderX) Destroy(gameObject);
    }

    //requires either additional checking or a proper collision matrix
    protected void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Enemy")) {
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            if (!hitEnemies.Contains(enemy)) {
                FindAnyObjectByType<player_movement>().TriggerHitEffects(this, enemy);
                enemy.ChangeHealth(-bulletDamage);
                hitEnemies.Add(enemy);
                //check if projectile can pierce
                if (piercing == 1) {
                    Destroy(gameObject);
                } else {
                    piercing--;
                }
            }
        }
    }
}