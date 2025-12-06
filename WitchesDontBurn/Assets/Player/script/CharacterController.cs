using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private int maxWaterCapacity = 6;
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

    public void ShootWater()
    {
        // cooldown
        if (Time.time < lastShootTime + shootCooldown) return;

        if (currentWater <= 0) return;

        Vector3 mouseScreen = Input.mousePosition;
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("ShootWater: Camera.main not found.");
            return;
        }

        Vector3 mouseWorld = cam.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = 0f;

        Vector2 origin = shootOrigin != null ? (Vector2)shootOrigin.position : (Vector2)transform.position;
        Vector2 direction = (mouseWorld - (Vector3)origin).normalized;

        if (waterProjectilePrefab == null)
        {
            Debug.LogWarning("ShootWater: waterProjectilePrefab is not assigned.");
            return;
        }

        GameObject proj = Instantiate(waterProjectilePrefab, origin, Quaternion.identity);
        Rigidbody2D prb = proj.GetComponent<Rigidbody2D>();
        if (prb == null)
            prb = proj.AddComponent<Rigidbody2D>();
        prb.gravityScale = 0f;
        prb.linearVelocity = direction * shootSpeed;

        // optional: tag projectile so other systems can detect it
        try { proj.tag = "WaterProjectile"; } catch { }

        // destroy after time to avoid clutter
        Destroy(proj, 5f);

        currentWater--;
        lastShootTime = Time.time;
    }


}