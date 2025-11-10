using UnityEngine;

public class EnemyFlyIn : MonoBehaviour
{
    public Vector3 targetPosition;
    public float moveDuration = 1f;

    private Vector3 startPosition;
    private float timer = 0f;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (timer < moveDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / moveDuration);
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
        }
        else
        {
            Destroy(this); // remove once finished
        }
    }
}
