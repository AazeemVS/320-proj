using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SlowingProjectile : MonoBehaviour
{
    private Vector2 _dir;
    private float _speed;
    private float _slowPercent;
    private float _slowDuration;

    public void Configure(Vector2 dir, float speed, float slowPercent, float slowDuration)
    {
        _dir = dir;
        _speed = speed;
        _slowPercent = Mathf.Clamp01(slowPercent);
        _slowDuration = Mathf.Max(0f, slowDuration);
    }

    void Update()
    {
        transform.position += (Vector3)(_dir * _speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryApplySlow(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryApplySlow(collision.gameObject);
    }

    private void TryApplySlow(GameObject hit)
    {
        if (!hit.CompareTag("Player")) return;

        var pm = hit.GetComponent<player_movement>();
        if (pm == null) return;

        var status = hit.GetComponent<SlowedStatus>();
        if (status == null) status = hit.AddComponent<SlowedStatus>();

        status.ApplySlow(_slowPercent, _slowDuration);

        gameObject.SetActive(false);
    }
}
