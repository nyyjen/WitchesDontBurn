using UnityEngine;
using System.Collections;

public class CatBehavior : MonoBehaviour
{
    [Header("Jump Settings")]
    public float horizontalJumpSpeed = 2f;
    public float verticalJumpSpeed = 4f;
    public float fallGravity = 2f;

    [Header("Walk Settings")]
    public float walkSpeed = 1.2f;     
    public LayerMask Wall;        
    public float wallCheckDistance = 0.2f;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.45f;

    private Rigidbody2D rb;
    private Animator anim;
    private CapsuleCollider2D col;

    private bool isJumping = false;
    private bool hasLanded = false;

    // -1 = 左走，1 = 右走
    private int walkDirection = -1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        anim = GetComponent<Animator>();

        rb.freezeRotation = true;
        rb.gravityScale = 0;  
    }

    private void Update()
    {
        if (anim.GetBool("catWalk"))
            WalkMovement();
    }

    // -----------------------------
    //   自動走路
    // -----------------------------
    private void WalkMovement()
    {
        rb.linearVelocity = new Vector2(walkDirection * walkSpeed, 0);

        transform.localScale = new Vector3(-walkDirection, 1, 1);

        // 偵測前方是否有牆
        Vector2 origin = new Vector2(col.bounds.center.x + (walkDirection * col.bounds.extents.x), col.bounds.center.y);
        Vector2 dir = new Vector2(walkDirection, 0);

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, wallCheckDistance, Wall);
        Debug.DrawRay(origin, dir * wallCheckDistance, Color.green);

        if (hit.collider != null)
        {
            // ★ 你說「碰到牆直接消失」
            Destroy(gameObject);
        }
    }

    // -----------------------------
    //   跳窗 → 掉落 → 落地 → 開始走路
    // -----------------------------
    private IEnumerator JumpOffWindow()
    {
        isJumping = true;
        hasLanded = false;

        anim.SetBool("catJump", true);
        anim.SetBool("catLand", false);
        anim.SetBool("catWalk", false);

        yield return new WaitForSeconds(0.05f);

        rb.gravityScale = fallGravity;
        rb.linearVelocity = new Vector2(-horizontalJumpSpeed, verticalJumpSpeed); // 往左跳

        // 落地偵測
        while (!hasLanded)
        {
            Vector2 footPos = new Vector2(col.bounds.center.x, col.bounds.min.y);
            RaycastHit2D hit = Physics2D.Raycast(footPos, Vector2.down, 0.2f, groundLayer);

            Debug.DrawRay(footPos, Vector2.down * 0.2f, Color.red);

            if (hit.collider != null)
                hasLanded = true;

            yield return null;
        }

        // 落地
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;

        anim.SetBool("catJump", false);
        anim.SetBool("catLand", true);
        yield return new WaitForSeconds(0.25f);

        anim.SetBool("catLand", false);
        anim.SetBool("catWalk", true);

        // ★★★ 落地後隨機決定向左或向右（-1 or 1）
        walkDirection = Random.value < 0.5f ? -1 : 1;

        isJumping = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Window") && !isJumping)
        {
            StartCoroutine(JumpOffWindow());
        }
    }
}
