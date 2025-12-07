using UnityEngine;
using System.Collections;

public class NPCfalling : MonoBehaviour
{
    [Header("Jump Settings")]
    public float horizontalJumpSpeed = 2f;
    public float verticalJumpSpeed = 4f;
    public float fallGravity = 2f;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.45f;

    private Rigidbody2D rb;
    private CapsuleCollider2D col;

    private bool isJumping = false;
    private bool hasLanded = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();

        rb.freezeRotation = true;  // ★ 不旋轉（最重要）
        rb.gravityScale = 0;       // 平常不掉
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Window") && !isJumping)
        {
            StartCoroutine(JumpOffWindow());
        }
    }

    private IEnumerator JumpOffWindow()
    {
        isJumping = true;
        hasLanded = false;

        yield return new WaitForSeconds(0.05f);

        // 啟動重力，給予初速
        rb.gravityScale = fallGravity;
        rb.linearVelocity = new Vector2(-horizontalJumpSpeed, verticalJumpSpeed);

        // ---- 落地偵測 ----
        while (!hasLanded)
        {
            Vector2 footPos = new Vector2(col.bounds.center.x, col.bounds.min.y);
            RaycastHit2D hit = Physics2D.Raycast(footPos, Vector2.down, groundCheckDistance, groundLayer);

            Debug.DrawRay(footPos, Vector2.down * groundCheckDistance, Color.yellow);

            if (hit.collider != null)
                hasLanded = true;

            yield return null;
        }

        // ---- 落地後停止 ----
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;

        isJumping = false;
    }
}
