using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [SerializeField] private float maxMoveSpeed = 8f;
    [SerializeField] private float acceleration = 3f;
    [SerializeField] private float moveSpeed = 0f;
    
    [SerializeField] public int maxWaterCapacity = 6;
    [SerializeField] public int currentWater = 3;
    private Rigidbody2D rb;
    private Vector2 move;
    private bool CarryRequested = false;
    
    private Animator animator;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
    }

    private void FixedUpdate()
    {
        // Use the PlayerInputHandler to get movement input
        if (moveSpeed < maxMoveSpeed){
            moveSpeed += acceleration * Time.fixedDeltaTime;
        }
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
        AkSoundEngine.SetRTPCValue("PlayerSpeed", move.magnitude * moveSpeed, gameObject);
    }
    public void Move(Vector2 moveVector)
    {
        // Use the PlayerInputHandler to get movement input
        move = moveVector;
        animator.SetBool("isFlying", true);
    }
    public void UseBroom()
    {
        // Handle skill usage logic here
        CarryRequested = true;
    }

    public bool CanCarry()
    {

        return currentWater < maxWaterCapacity;
    }


}