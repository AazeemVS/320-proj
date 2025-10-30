using UnityEngine;

public class EnemySpawnWarning : MonoBehaviour
{
    public GameObject enemyToSpawn;
    private float timeTillSpawn;
    [SerializeField] float lifetime;
    Vector2 scale;
    private float spawnDelay = .4f;

    private void Start() {
        timeTillSpawn = lifetime;
        lifetime -= spawnDelay;
        scale = transform.localScale;
        transform.localScale = scale*.5f;
    }

    // Update is called once per frame
    void Update()
    {
        timeTillSpawn -= Time.deltaTime;
        //added target scaling and pause in scaling before timer is done to increase information for player
        if(timeTillSpawn > spawnDelay) transform.localScale = scale * (1 - .5f*(timeTillSpawn-spawnDelay)/lifetime);
        if(timeTillSpawn < 0) {
            Instantiate(enemyToSpawn, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
