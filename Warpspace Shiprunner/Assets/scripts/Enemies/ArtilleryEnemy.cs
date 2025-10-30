using UnityEngine;

public class ArtilleryEnemy : Enemy
{
    [SerializeField] GameObject bombWarning;
    [SerializeField] GameObject bombExplosion;
    [SerializeField] float playerVisionTolerance = 2;
    [SerializeField] float speed;
    Transform playerPosition;
    Rigidbody2D rb;
    [SerializeField] float accuracy = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerPosition = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
    }

    public override void FireOnce() {
        //create three warnings for bombs at random spread near the player
        for (int i = 0; i < 3; i++) {
            Vector3 offset = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f))*accuracy;
            GameObject warning = Instantiate(bombWarning, playerPosition.position + offset, Quaternion.identity);
            warning.GetComponent<EnemySpawnWarning>().enemyToSpawn = bombExplosion;
        }
    }
    //the artillery enemy runs away from the player if they align on the y axis (within tolerance)
    public override void Movement() {
        Vector2 pos = transform.position;
        Vector2 playerPos = playerPosition.position;
        Vector2 moveDirection = Vector2.zero;
        if (pos.y + playerVisionTolerance > playerPos.y && pos.y - playerVisionTolerance < playerPos.y) {
            //first, if we are too close too the screen edge we move away from that edge, even if it brings us closer to the player
            if (playerPos.y + playerVisionTolerance >= borderY) moveDirection = Vector2.down;
            //otherwise we simply move away from the player
            else if (playerPos.y - playerVisionTolerance <= borderY) moveDirection = Vector2.up;
            else if (playerPos.y >= pos.y) moveDirection = Vector2.down;
            else moveDirection = Vector2.up;
        }
        rb.linearVelocity = moveDirection * speed;
    }
}
