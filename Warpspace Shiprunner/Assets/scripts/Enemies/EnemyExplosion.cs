using System.Collections.Generic;
using UnityEngine;

public class EnemyExplosion : Projectile
{
    float lifetimeTimer;
    Color c;
    public List<Enemy> hitEnemies = new List<Enemy>();
    [SerializeField] bool friendlyFire = true;
    SpriteRenderer spriteRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lifetimeTimer = lifetime;
        c = Color.white;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        lifetimeTimer -= Time.deltaTime;
        //modify the explosions alpha to fade out over time
        c.a = lifetimeTimer / lifetime;
        spriteRenderer.color = c;
        if (lifetimeTimer <= 0) { Destroy(gameObject); }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        //this collision also checks our lifetime as the transparency of the explosion fading makes getting hit at the very end feel bad
        if (other.CompareTag("Player") && lifetimeTimer > lifetime / 3) {
            player_movement player = other.GetComponent<player_movement>();
            player.ChangeHealth(-damage);
        } else if (other.CompareTag("Enemy") && friendlyFire) { // friendly fire for enemy explosions
            Enemy enemy = other.gameObject.GetComponent<Enemy>();
            if (!hitEnemies.Contains(enemy)) {
                enemy.ChangeHealth(-damage);
                hitEnemies.Add(enemy);
            }
        }
    }
}
