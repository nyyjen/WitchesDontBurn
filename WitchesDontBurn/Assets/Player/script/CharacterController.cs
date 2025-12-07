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

    public void ShootWater()
    {
        // cooldown
        if (Time.time < lastShootTime + shootCooldown) return;
        if (currentWater <= 0) return;

        if (Mouse.current == null) return;
        Vector2 mouseScreen = Mouse.current.position.ReadValue();

        Camera cam = Camera.main;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 0f));
        mouseWorld.z = 0f;

        // origin
        Vector2 origin = shootOrigin != null ? (Vector2)shootOrigin.position : (Vector2)transform.position;

        // direction
        Vector2 direction = (mouseWorld - (Vector3)origin).normalized;

        GameObject proj = Instantiate(waterProjectilePrefab, origin, Quaternion.identity);
        Rigidbody2D prb = proj.GetComponent<Rigidbody2D>();
        if (prb == null)
            prb = proj.AddComponent<Rigidbody2D>();

        prb.gravityScale = 0f;
        prb.linearVelocity = direction * shootSpeed;

        // tag
        try { proj.tag = "ShootingWater"; } catch { }

        Destroy(proj, 5f);

        currentWater--;
        lastShootTime = Time.time;
    }
}