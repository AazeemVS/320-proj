using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] float xSpawn = 7.5f;
    [SerializeField] float interval = 1.5f;
    float borderY;
    [SerializeField] List<GameObject> enemyPrefabs;

    float _t;
    private void Start() {
        Camera cam = Camera.main;
        borderY = cam.orthographicSize;
        borderY -= enemyPrefabs[0].GetComponent<SpriteRenderer>().bounds.size.y / 2;
    }

    void Update()
    {
        _t += Time.deltaTime;
        if (_t >= interval)
        {
            _t = 0f;
            float y = Random.Range(-borderY, borderY);
            GameObject enemy = enemyPrefabs[Random.Range(0,enemyPrefabs.Count)];
            Instantiate(enemy, new Vector3(xSpawn, y, 0f), Quaternion.identity);
        }
    }
}
