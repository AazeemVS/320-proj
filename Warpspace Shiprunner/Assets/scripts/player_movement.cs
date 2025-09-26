using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    // Players movement speed
    public float moveSpeed = 5f; 

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("No Rigidbody2D found! Add one to your Player object.");
        }
    }

    void Update()
    {
        // Get input from arrow keys / WASD
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveInput = new Vector2(moveX, moveY).normalized;
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }
    }
}
