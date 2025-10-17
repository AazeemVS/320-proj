using UnityEngine;

public class ChasingEnemy : Enemy
{
    //0 = turning, 1 = moving, 2 = cooldown after hit
    public int state = 0;
    [SerializeField] protected float chaseLength;
    [SerializeField] protected float speed;
    [SerializeField] protected float damage;
    float chaseTimer = 0;
    float damageCooldown;
    float turnTime = 0;
    Quaternion startingRotation;
    Vector2 towardsPlayer;
    [SerializeField] float turnSpeed = 10f;
    Transform playerPosition; 
    Rigidbody2D rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerPosition = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override void FireOnce() {
    }

    public override void Movement() {
        switch (state) {
            case 0:
                TurnTowardsPlayer();
                break;
            case 1:
                ChasePlayer();
                break;
            case 2:
                Cooldown();
                break;
        }
    }

    private void TurnTowardsPlayer() {
        float targetAngle = Mathf.Atan2(towardsPlayer.y, towardsPlayer.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Slerp(Quaternion.Euler(0, 0, targetAngle), startingRotation, turnTime/turnSpeed);
        turnTime -= Time.deltaTime;
        if (turnTime < 0) {
            state = 1;
            chaseTimer = chaseLength;
        }
    }
    private void ChasePlayer() {
        rb.linearVelocity = -towardsPlayer * speed * Time.deltaTime;
        chaseTimer -= Time.deltaTime;
        if(chaseTimer <= 0) {
            state = 0;
            rb.linearVelocity = Vector2.zero;
            towardsPlayer = (transform.position - playerPosition.position).normalized;
            startingRotation = transform.rotation;
            turnTime = Quaternion.Angle(startingRotation, Quaternion.Euler(0, 0, Mathf.Atan2(towardsPlayer.y, towardsPlayer.x) * Mathf.Rad2Deg)) *turnSpeed/ 180;
        }
    }

    private void Cooldown() {
        GetComponent<SpriteRenderer>().color = new Color((255 - 85*damageCooldown/3f)/255, (0 + 60*damageCooldown/3f)/255, (153 - 30 *damageCooldown/3f)/255);
        damageCooldown -= Time.deltaTime;
        if (damageCooldown <= 0) {
            state = 0;
            GetComponent<SpriteRenderer>().color = new Color(1, 0, .6f);
        }
        
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player") && state != 2) {
            player_movement player = other.GetComponent<player_movement>();
            player.ChangeHealth(-damage);
            state = 2;
            damageCooldown = 3;
        }
    }

}
