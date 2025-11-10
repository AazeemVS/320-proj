using UnityEngine;

public class EnemySpawnWarning : MonoBehaviour
{
    public GameObject enemyToSpawn;

    [SerializeField] private float lifetime = 1f;     // total visible time of the warning
    [SerializeField] private float offscreenDistance = 2f;
    [SerializeField] private float flyInDuration = 1f;

    private float timeTillSpawn;
    private Vector2 scale;

    private void Start()
    {
        timeTillSpawn = lifetime;
        scale = transform.localScale;
        transform.localScale = scale * 0.5f;
    }

    private void Update()
    {
        timeTillSpawn -= Time.deltaTime;

        // scaling animation (optional visual feedback)
        if (timeTillSpawn > 0.4f)
        {
            transform.localScale = scale * (1 - 0.5f * (timeTillSpawn - 0.4f) / lifetime);
        }

        // time to spawn enemy
        if (timeTillSpawn <= 0)
        {
            SpawnEnemy();
            Destroy(gameObject); // remove red circle
        }
    }

    private void SpawnEnemy()
    {
        if (enemyToSpawn == null) return;

        Vector3 finalPos = transform.position;
        Vector3 spawnPos = finalPos + Vector3.right * offscreenDistance;

        GameObject enemy = Instantiate(enemyToSpawn, spawnPos, Quaternion.identity);

        // make the enemy fly in to the warning position
        EnemyFlyIn mover = enemy.AddComponent<EnemyFlyIn>();
        mover.targetPosition = finalPos;
        mover.moveDuration = flyInDuration;
    }
}
