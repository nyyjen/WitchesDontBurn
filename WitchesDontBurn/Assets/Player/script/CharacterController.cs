using UnityEngine;
using UnityEngine.InputSystem;

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
    
    [Header("Shooting")]
    [Tooltip("Projectile prefab used when shooting water")]
    public GameObject waterProjectilePrefab;
    [Tooltip("Optional spawn point for projectiles. If null, player's position is used.")]
    public Transform shootOrigin;
    [Tooltip("Projectile speed in units/sec")]
    public float shootSpeed = 10f;
    [Tooltip("Seconds between shots")]
    public float shootCooldown = 0.3f;
    private float lastShootTime = -Mathf.Infinity;
    [Tooltip("Maximum distance to shoot water at window")]
    public float maxShootDistance = 10f;
    [Tooltip("Distance threshold for near vs far shooting animation")]
    public float nearFarThreshold = 5f;
    
    [Header("NPC Carrying")]
    public bool isCarryingNPC = false;
    public GameObject npcOnWindow = null;
    public GameObject npcPrefabToDrop;


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
        if (move == Vector2.zero)
        {
            moveSpeed = 0f;
        }
        else
        {
            // 有在移動才加速
            if (moveSpeed < maxMoveSpeed)
            {
                moveSpeed += acceleration * Time.fixedDeltaTime;
            }
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
            scale.x = -Mathf.Sign(move.x) * Mathf.Abs(scale.x); 
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

    public void SetWateringTrigger()
    {
        if (animator != null)
        {
            animator.SetTrigger("isWatering");
        }
    }

    public void SetFlyingBool(bool value)
    {
        if (animator != null)
        {
            animator.SetBool("isFlying", value);
        }
    }

    public void ShootWater()
    {
        // cooldown
        if (Time.time < lastShootTime + shootCooldown) return;
        if (currentWater <= 0) return;

        if (Mouse.current == null) return;
        Vector2 mouseScreen = Mouse.current.position.ReadValue();

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 0f));
        mouseWorld.z = 0f;

        // origin
        Vector2 origin = shootOrigin != null ? (Vector2)shootOrigin.position : (Vector2)transform.position;

        // Check if mouse is over a window using raycast
        Vector2 direction = (mouseWorld - (Vector3)origin).normalized;
        float distance = Vector2.Distance(origin, mouseWorld);
        
        // Check if mouse position hits a window
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, distance);
        GameObject hitWindow = null;
        
        if (hit.collider != null)
        {
            GameObject hitObject = hit.collider.gameObject;
            // Check if it's a window or find window in parent
            if (hitObject.CompareTag("Window"))
            {
                hitWindow = hitObject;
            }
            else
            {
                // Check parent
                Transform parent = hitObject.transform.parent;
                if (parent != null && parent.CompareTag("Window"))
                {
                    hitWindow = parent.gameObject;
                }
            }
        }
        
        // If no window found via raycast, try finding closest window to mouse position
        if (hitWindow == null)
        {
            GameObject[] windows = GameObject.FindGameObjectsWithTag("Window");
            float closestDist = float.MaxValue;
            foreach (GameObject window in windows)
            {
                float dist = Vector2.Distance(mouseWorld, window.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    hitWindow = window;
                }
            }
            // Check if the closest window is close enough to mouse position (within 2 units)
            if (closestDist > 2f)
            {
                hitWindow = null;
            }
        }
        
        // Check if window is within shooting range
        if (hitWindow == null)
        {
            return; // No valid window target
        }
        
        float windowDistance = Vector2.Distance(origin, hitWindow.transform.position);
        if (windowDistance > maxShootDistance)
        {
            return; // Window too far
        }
        
        // Set animation triggers based on distance
        if (windowDistance <= nearFarThreshold)
        {
            animator.SetTrigger("isWateringNear");
        }
        else
        {
            animator.SetTrigger("isWateringFar");
        }
        
        // Don't create projectile - just trigger animation
        currentWater--;
        lastShootTime = Time.time;
    }
}