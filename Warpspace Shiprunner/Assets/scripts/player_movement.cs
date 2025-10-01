using UnityEngine;

public class player_movement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public GameObject bulletPrefab;
    public Transform firePoint;       // Where the bullet shoots from
    public float bulletSpeed = 10f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float borderX;
    private float borderY;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Camera cam = Camera.main;
        borderY = cam.orthographicSize;
        borderX = borderY * cam.aspect;
        borderX -= GetComponent<SpriteRenderer>().bounds.size.x/2;
        borderY -= GetComponent<SpriteRenderer>().bounds.size.y/2;
    }

    void Update()
    {
        HandleMovement();
        HandleShooting();
    }

    void HandleMovement()
    {
        float moveX = 0f;
        float moveY = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            moveX = -1f;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            moveX = 1f;

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            moveY = -1f;
        else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            moveY = 1f;

        moveInput = new Vector2(moveX, moveY).normalized;
        rb.linearVelocity = moveInput * moveSpeed;

        //Adjust player position to stay in camera bounds
        Vector3 adjustedPos = transform.position;
        if(transform.position.x > borderX) adjustedPos.x = borderX;
        else if (transform.position.x < -borderX) adjustedPos.x = -borderX;
        if (transform.position.y > borderY) adjustedPos.y = borderY;
        else if (transform.position.y < -borderY) adjustedPos.y = -borderY;
        transform.position = adjustedPos;
    }

    void HandleShooting()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            bulletRb.linearVelocity = Vector2.right * bulletSpeed; // shoots to the right
        }
    }
}
