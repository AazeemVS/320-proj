using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] float speed = 10f;
    [SerializeField] float lifetime = 5f;
    [SerializeField] int damage = 1;

    Vector2 _direction = Vector2.left;
    float _age;

    public void Fire(Vector2 worldDirection, float overrideSpeed = -1f)
    {
        _direction = worldDirection.normalized;
        if (overrideSpeed > 0f) speed = overrideSpeed;
        _age = 0f;
        gameObject.SetActive(true);
    }

    void Update()
    {
        transform.Translate(_direction * speed * Time.deltaTime, Space.World);
        _age += Time.deltaTime;
        if (_age >= lifetime) gameObject.SetActive(false);
    }

    // Hook this up to your health system later
    void OnTriggerEnter2D(Collider2D other)
    {
        // Example: only hit Player layer
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // TODO: call player.TakeDamage(damage);
            gameObject.SetActive(false);
        }

        // Optionally despawn on walls/kill zones
        if (other.CompareTag("KillZone")) gameObject.SetActive(false);
    }
}
