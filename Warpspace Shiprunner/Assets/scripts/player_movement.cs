using System.Collections;
using UnityEngine;

public class player_movement : MonoBehaviour
{
    
    public GameObject bulletPrefab;
    public Transform firePoint;       // Where the bullet shoots from

    //Modifiable Stats
    public float moveSpeed = 3f;
        //Gun stats
    public float bulletSpeed = 20f;
    public float playerDamage = 1, playerDamageMult = 1;
    public float bulletSize = 1;
    public float shootTimerMax = .25f;
    public int projectileAmt = 1;
    public int piercing = 1;
    public bool enragesOnHit = false;
    public float enrageLength = 5f, enrageTimer = 0;
    public float enrageDamage = 0;
    // Damaging Upgrades
    [SerializeField] GameObject killExplosion, hitExplosion;
    public bool explodeOnKill, explodeOnHit = false;
    public float killExplosionScale, hitExplosionScale = 1;

    //Dash stats
    public bool dashEnabled = true;
    public bool dashHasDodge = false;
    public float dashCooldown = 1.5f;
    public float maxHealth = 5;
    // Credits
    public static int credits = 0;
    public static int roundCredits = 0;

    // gameRound
    public static int gameRound = 1;


    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float borderX;
    private float borderY;
    private GameStateManager stateManager;
    [SerializeField] PlayerAnimation animManager;
    [SerializeField] private float health;
    private float shootTimer = 0;
    public float iFrameMax = 1f;
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
        InventoryManager inv = InventoryManager.Instance;
        if (inv != null) {
            for (int i = 0; i < inv.Active.Count; i++) {
                inv.Active[i].upgrade.OnEquip(this);
            }
        }

        roundCredits = 0;
        Time.timeScale = 1f;

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
                if (dashHasDodge && dashLength > iFrameTimer) {
                    iFrameTimer = dashLength;
                }
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
        if (animManager != null) { animManager.HandleMovementAnimation(moveInput); }
        

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
            for (int i = 0; i < projectileAmt; i++) {
                Vector3 yOffset;
                //if statement becuase the formula I used for offset divides by 0 when projAmt = 1, will look if theres a cleaner one later
                if (projectileAmt == 1) { yOffset = Vector3.zero; } 
                else {
                    float fireRange = 1f;
                    yOffset = new Vector3(0, (-fireRange / 2) + (i * fireRange / (projectileAmt - 1)));
                }
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position + yOffset, Quaternion.identity);
                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
                Bullet bulletScript = bulletRb.GetComponent<Bullet>();
                //Set bullet attributes
                bullet.transform.localScale *= bulletSize;
                bulletRb.linearVelocity = Vector2.right * bulletSpeed; // shoots to the right
                bulletScript.bulletDamage = playerDamage * playerDamageMult;
                if (explodeOnHit) bulletScript.bulletDamage = 0;
                bulletScript.piercing = piercing;
                shootTimer = shootTimerMax;
            }
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
            if (enragesOnHit) {
                if (enrageTimer <= 0) {
                    playerDamage += enrageDamage;
                }
                enrageTimer = enrageLength;
            }
        } else if(healthChange > 0) {
            health += healthChange;
            if(health > maxHealth) health = maxHealth;
        }
    }
    private void HandleHealth() {
        if(health <= 0) stateManager.RequestSceneChange(GameState.Playing, GameState.GameOver);
        iFrameTimer -= Time.deltaTime;
        if (enrageTimer > 0) {
            enrageTimer -= Time.deltaTime;
            if (enrageTimer <= 0) {
                playerDamage -= enrageDamage;
            }
        }

    }

    public void TriggerHitEffects(Bullet bullet, Enemy hitEnemy) {
        if(explodeOnHit == true && bullet.GetType() != typeof(PlayerExplosion)) {
            GameObject hitObj = Instantiate(hitExplosion, bullet.transform.position, Quaternion.identity);
            PlayerExplosion hitExp = hitObj.GetComponent<PlayerExplosion>();
            hitExp.bulletDamage = playerDamage * playerDamageMult;
            hitObj.transform.localScale *= hitExplosionScale;
        }
    }
    public void TriggerKill(Enemy killedEnemy) {
        AddCredits((int)killedEnemy.spawnWeight);
        if (explodeOnHit == true ) {
            GameObject killObj = Instantiate(killExplosion, killedEnemy.transform.position, Quaternion.identity);
            PlayerExplosion killExp = killObj.GetComponent<PlayerExplosion>();
            killExp.bulletDamage *= hitExplosionScale;
            killObj.transform.localScale *= hitExplosionScale;
        }
    }
    public void AddCredits(int amount)
    {
        credits += amount;
        roundCredits += amount;
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
