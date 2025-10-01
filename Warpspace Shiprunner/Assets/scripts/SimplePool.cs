using System.Collections.Generic;
using UnityEngine;

public class SimplePool : MonoBehaviour
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] int prewarmCount = 64;

    readonly Queue<GameObject> _pool = new();

    void Awake()
    {
        for (int i = 0; i < prewarmCount; i++)
        {
            var go = Instantiate(bulletPrefab, transform);
            go.SetActive(false);
            _pool.Enqueue(go);
        }
    }

    public GameObject Spawn(Vector3 position, Quaternion rotation)
    {
        GameObject go = _pool.Count > 0 ? _pool.Dequeue() : Instantiate(bulletPrefab, transform);
        go.transform.SetPositionAndRotation(position, rotation);
        go.SetActive(true);
        return go;
    }

    public void Despawn(GameObject go)
    {
        go.SetActive(false);
        _pool.Enqueue(go);
    }
}
