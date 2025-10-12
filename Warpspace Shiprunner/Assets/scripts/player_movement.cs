using System.Collections;
using UnityEngine;

public class player_movement : MonoBehaviour
{
    
    public GameObject bulletPrefab;
    public Transform firePoint;       // Where the bullet shoots from

    //Modifiable Stats
    public float moveSpeed = 5f;
        //Gun stats
    public float bulletSpeed = 10f;
    public float playerDamage = 1;
    public float shootTimerMax = .25f;
    public int projectileAmt = 1;
        //Dash stats
    public bool dashEnabled = true;
    public float dashCooldown = 1.5f;
    public float maxHealth = 5;


    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float borderX;
    private float borderY;
    private GameStateManager stateManager;
    [SerializeField] private float health;
    private float shootTimer = 0;
    private float iFrameMax = 1f;
    [SerializeField] private float iFrameTimer = 0f;
    //Dash Logic
    private float dashTimer = 0;
    private float dashStrength = 2.5f;
    private float dashing = 0f;
    private float dashLength = .15f;
    private Vector2 dashDirection;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Camera cam = Camera.main;
        borderY = cam.orthographicSize;
        borderX = borderY * cam.aspect;
        borderX -= GetComponent<SpriteRenderer>().bounds.size.x/2;
        borderY -= GetComponent<SpriteRenderer>().bounds.size.y/2;
        stateManager = GameStateManager.Instance;
        health = maxHealth;
    }

    void Update()
    {
        HandleMovement();
        HandleShooting();
        HandleHealth();
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

        if (dashEnabled) {
            if(Input.GetKey(KeyCode.LeftShift) && dashTimer < 0) {
                dashing = dashLength;
                dashDirection = moveInput;
                dashTimer = dashCooldown;
            }
            if (dashing > 0) {
                moveInput = dashDirection * dashStrength;
                dashing -= Time.deltaTime;
            }
            dashTimer -= Time.deltaTime;
        }
        
        
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
        if (Input.GetKey(KeyCode.Space) && shootTimer <= 0) {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
            Bullet bulletScript = bulletRb.GetComponent<Bullet>();
            bulletRb.linearVelocity = Vector2.right * bulletSpeed; // shoots to the right
            bulletScript.bulletDamage = playerDamage;
            shootTimer = shootTimerMax;

        } else {
            shootTimer -= Time.deltaTime;
        }
    }

    public void ChangeHealth(float healthChange, bool beatsIFrames = false) {
        
        if(healthChange < 0 && (iFrameTimer<=0 || beatsIFrames)) {
            health += healthChange;
            StartCoroutine(TakeDamage(beatsIFrames));
            if (!beatsIFrames) {
                iFrameTimer = iFrameMax;
            }
            
        } else if(healthChange > 0) {
            health += healthChange;
            if(health > maxHealth) health = maxHealth;
        }
    }
    private void HandleHealth() {
        if(health <= 0) stateManager.RequestSceneChange(GameState.Playing, GameState.GameOver);
        iFrameTimer -= Time.deltaTime;
    }

    IEnumerator TakeDamage(bool piercing) {
        GetComponent<SpriteRenderer>().color = Color.red;
        if (piercing) {
            yield return new WaitForSeconds(.1f);
        } else {
            yield return new WaitForSeconds(iFrameMax);
        }
        GetComponent<SpriteRenderer>().color = Color.white;
    }
}
