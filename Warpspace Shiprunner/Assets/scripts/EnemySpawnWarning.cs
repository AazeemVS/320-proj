using UnityEngine;

public class EnemySpawnWarning : MonoBehaviour
{
    public GameObject enemyToSpawn;
    [SerializeField] private float timeTillSpawn;

    // Update is called once per frame
    void Update()
    {
        timeTillSpawn -= Time.deltaTime;
        if(timeTillSpawn < 0) {
            Instantiate(enemyToSpawn, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
