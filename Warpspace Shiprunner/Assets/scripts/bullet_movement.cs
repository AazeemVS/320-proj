using UnityEngine;

public class Bullet : MonoBehaviour {

    public float speed = 10f;
    private float borderX;

    private void Start() {
        Camera cam = Camera.main;
        borderX = cam.orthographicSize * cam.aspect;
        borderX += GetComponent<SpriteRenderer>().bounds.size.x;
    }
    void Update()
    {
        transform.Translate(Vector2.right * speed * Time.deltaTime);
        //destroy bullet if it goes fully off screen
        if (transform.position.x > borderX) Destroy(gameObject);
    }

    //requires either additional checking or a proper collision matrix
    private void OnCollisionEnter2D(Collision2D collision) {
        //do damage
    }
}