using UnityEngine;

public class ProjectileDamage : MonoBehaviour
{
  [Tooltip("Damage dealt on hit")]
  public float damage = 1f;

  [Tooltip("If true, bullets damage enemies; otherwise they damage the player.")]
  public bool hitEnemy = false;

  private void OnTriggerEnter2D(Collider2D other)
  {
    // Route damage by tag (or use layers)
    if (hitEnemy && !other.CompareTag("Enemy")) return;
    if (!hitEnemy && !other.CompareTag("Player")) return;

    var dmg = other.GetComponent<IDamageable>();
    if (dmg != null)
    {
      dmg.Damage(damage);
      gameObject.SetActive(false); // return to pool / destroy
    }
  }

  private void OnCollisionEnter2D(Collision2D c)
  {
    OnTriggerEnter2D(c.collider);
  }
}
