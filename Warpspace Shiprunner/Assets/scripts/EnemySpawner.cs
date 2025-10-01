using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] float xSpawn = 7.5f;
    [SerializeField] float interval = 1.5f;
    float borderY;

    float _t;
    private void Start() {
        Camera cam = Camera.main;
        borderY = cam.orthographicSize;
        borderY -= enemyPrefab.GetComponent<SpriteRenderer>().bounds.size.y / 2;
    }

    void Update()
    {
        _t += Time.deltaTime;
        if (_t >= interval)
        {
            _t = 0f;
            float y = Random.Range(-borderY, borderY);
            Instantiate(enemyPrefab, new Vector3(xSpawn, y, 0f), Quaternion.identity);
        }
    }
}
