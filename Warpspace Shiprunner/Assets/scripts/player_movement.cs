using System.Collections;
using TMPro;
using Unity.VisualScripting;
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
    public float spread = 0;
    // Damaging Upgrades
    public int killTriggers = 1; //Amount of times upgrades that trigger on kill activate. Modified by "ExtraKillTrigger" upgrade
    public bool enragesOnHit = false; public float enrageLength = 5f, enrageTimer = 0, enrageDamage = 0; // "EnrageUpgrade" fields
    public bool killBoost = false; public float killBoostLength = 3f, killBoostTimer = 0, killBoostDamage = 0; // "DamageOnKill" fields
    public int virusBoost = 0; public int virusBonus = 0; // "VirusDamageBoost" fields
    public bool hasPoison = false; public float poisonLength = 0, poisonDPS = .5f; // DoT on hit upgrade
    [SerializeField] GameObject killExplosion, hitExplosion; // prefabs for "ExplosiveKillUpgrade" and "ExplosiveHitUpgrade" respectively
    public bool explodeOnKill, explodeOnHit = false; public float killExplosionScale, hitExplosionScale = 1;
    //Health stats
    public bool hasHealthSteal = false; public float healthStealScalar = .1f; // "HealthOnKill"
    public float iFrameMax = 1f;
    //Dash stats
    public bool dashEnabled = true;
    public bool dashHasDodge = false;
    public float dashCooldown = 1.5f;
    public float maxHealth = 5;
    // Credits
    public static int credits = 0;
    public static int roundCredits = 0;
    public int extraKillCredits = 0; // modified by "CreditsOnKill" upgrade
    public float insuranceCreditsScalar = 0; // modified by "CreditsWhenHit" upgrade

    // gameRound
    public static int gameRound = 1;


    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float borderX;
    private float borderY;
    public float topUIHeight;
    public float bottomUIHeight;
    private GameStateManager stateManager;
    private GraphicsManager graphicsManager;
    private AudioManager audioManager;
    [SerializeField] PlayerAnimation animManager;
    [SerializeField] TextMeshProUGUI healthUI;
    [SerializeField] private float health;
    private float shootTimer = 0;
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
        audioManager = GetComponentInChildren<AudioManager>();
        graphicsManager = FindAnyObjectByType<GraphicsManager>();
        if(graphicsManager != null) { graphicsManager.playerHealthMax = maxHealth; }
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
        HandleTimers();
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
        if (transform.position.y > borderY - topUIHeight) adjustedPos.y = borderY - topUIHeight;
        //else if (transform.position.y < -borderY + bottomUIHeight) adjustedPos.y = -borderY + bottomUIHeight;
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
                float spreadAngle = Random.Range(-spread, spread);
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position + yOffset, Quaternion.Euler(0, 0, spreadAngle));
                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
                Bullet bulletScript = bulletRb.GetComponent<Bullet>();
                //Set bullet attributes
                bullet.transform.localScale *= bulletSize;
                bulletRb.linearVelocity = Vector2.right * bulletSpeed;
                bulletRb.linearVelocity = new Vector2(Mathf.Cos(Mathf.Deg2Rad * spreadAngle),Mathf.Sin(Mathf.Deg2Rad*spreadAngle)) * bulletSpeed; // shoots to the right
                bulletScript.bulletDamage = (playerDamage + (virusBoost*virusBonus*.5f)) * playerDamageMult;
                if (explodeOnHit) bulletScript.bulletDamage = 0;
                bulletScript.piercing = piercing;
                shootTimer = shootTimerMax;
                audioManager.PlaySound(SoundID.PlayerShoot);
                
            }
        } else {
            shootTimer -= Time.deltaTime;
        }
    }

    public void ChangeHealth(float healthChange, bool beatsIFrames = false) {
        
        if(healthChange < 0 && (iFrameTimer<=0 || beatsIFrames)) {
            health += healthChange;
            GetComponent<SpriteRenderer>().color = Color.red;
            if (!beatsIFrames) {
                iFrameTimer = iFrameMax;
            }
            OnTakeDamageUpgrades(healthChange);
            audioManager.PlaySound(SoundID.PlayerHit);
        } else if(healthChange >= 0) {
            health += healthChange;
            if(health > maxHealth) health = maxHealth;
        }
        //if (healthUI != null) { healthUI.text = ("Health:" + health); }
        if (graphicsManager != null) { graphicsManager.UpdateHealthbar(health); }
    }

    private void OnTakeDamageUpgrades(float damage) {
        if (enragesOnHit) {
            if (enrageTimer <= 0) {
                playerDamage += enrageDamage;
            }
            enrageTimer = enrageLength;
        }
        if (insuranceCreditsScalar > 0) {
            AddCredits(1 + (int)Mathf.Ceil(damage * insuranceCreditsScalar));
        }
       
    }
    private void HandleHealth() {
        if (health <= 0) {

            // Remove all equipped upgrades when the player dies
            var inv = InventoryManager.Instance;
            if (inv != null)
            {
                inv.ClearAllUpgrades();
            }

            gameRound = 1;
            roundCredits = 0;
            stateManager.RequestSceneChange(GameState.Playing, GameState.GameOver);
        }
        if (iFrameTimer > 0) {
            iFrameTimer -= Time.deltaTime;
            if (iFrameTimer <= 0) { 
                GetComponent<SpriteRenderer>().color = Color.white;
                audioManager.PlaySound(SoundID.PlayerIFrame);
            }
        }
    if (enrageTimer > 0) {
            enrageTimer -= Time.deltaTime;
            if (enrageTimer <= 0) {
                playerDamage -= enrageDamage;
            }
        }

    }

    private void HandleTimers() {
        if (killBoost && killBoostTimer > 0) {
            killBoostTimer -= Time.deltaTime;
            if(killBoostTimer <= 0) {
                playerDamage -= killBoostDamage * killTriggers;
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
        if (hasPoison) { hitEnemy.EnablePoison(poisonLength, poisonDPS); }
    }
    public void TriggerKill(Enemy killedEnemy) {
        //AddCredits((int)Mathf.Ceil(killedEnemy.spawnCost)); Credits are given in enemy
        for (int i = 0; i < killTriggers; i++) {
            OnKillUpgrades(killedEnemy);
        }
    }

    private void OnKillUpgrades(Enemy killedEnemy) {
        //explosion on kill upgrade
        if (explodeOnKill == true) {
            GameObject killObj = Instantiate(killExplosion, killedEnemy.transform.position, Quaternion.identity);
            PlayerExplosion killExp = killObj.GetComponent<PlayerExplosion>();
            killExp.bulletDamage *= killExplosionScale;
            killObj.transform.localScale *= killExplosionScale;
        }
        //damage up on kill logic
        if (killBoost) {
            if (killBoostTimer <= 0) {
                playerDamage += killBoostDamage * killTriggers;
            }
            killBoostTimer = killBoostLength;
        }
        if (hasHealthSteal) { ChangeHealth(killedEnemy.spawnCost*healthStealScalar); }
        AddCredits(extraKillCredits);
    }
    public void AddCredits(int amount) {
        credits += amount;
        roundCredits += amount;
    }
}
