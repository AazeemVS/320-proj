using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] Vector2 xRange = new(-8f, 8f);
    [SerializeField] float ySpawn = 5.5f;
    [SerializeField] float interval = 1.5f;

    float _t;

    void Update()
    {
        _t += Time.deltaTime;
        if (_t >= interval)
        {
            _t = 0f;
            float x = Random.Range(xRange.x, xRange.y);
            Instantiate(enemyPrefab, new Vector3(x, ySpawn, 0f), Quaternion.identity);
        }
    }
}
