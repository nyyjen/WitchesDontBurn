using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 move;
    private bool CarryRequested = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
    }

    private void FixedUpdate()
    {
        // Use the PlayerInputHandler to get movement input
        rb.linearVelocity = move * moveSpeed;
        if (CarryRequested)
        {
            // Execute skill A logic here
            CarryRequested = false;
        }
        if (move.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(move.x) * Mathf.Abs(scale.x); 
            transform.localScale = scale;
        }
    }
    public void Move(Vector2 moveVector)
    {
        // Use the PlayerInputHandler to get movement input
        move = moveVector;
    }
    public void UseBroom()
    {
        // Handle skill usage logic here
        CarryRequested = true;
    }


}