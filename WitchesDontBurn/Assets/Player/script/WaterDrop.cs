using System.Collections;
using System.Linq;
using UnityEngine;

public class WaterDrop : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject SmallWater;
    public GameObject BigWater;
    public Transform player;
    
    [Header("Spawn settings")]
    public float spawnYHeight = 10f; // 天空固定高度
    public float spawnWidthMin = -8f; // 水平範圍最小
    public float spawnWidthMax = 8f; // 水平範圍最大
    
    [Header("Drop physics")]
    [Tooltip("Gravity scale applied to spawned water (lower = slower fall)")]
    public float dropGravityScale = 1f;
    [Tooltip("Linear drag applied to spawned water (higher = slower fall)")]
    public float dropLinearDrag = 5f;
    [Header("Alternate fall mode")]
    [Tooltip("When true, spawned drops ignore gravity and use a constant downward speed.")]
    public bool useConstantFallSpeed = true;
    [Tooltip("Constant downward speed (units/sec) when using constant fall mode")]
    public float constantFallSpeed = 1f;
    
    private CharacterController characterController;
    private void Awake()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        characterController = GetComponent<CharacterController>();
    }

    
    private void OnEnable()
    {
        StartCoroutine(SmallWaterLoop());
        StartCoroutine(BigWaterLoop());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator SmallWaterLoop()
    {
        while (true)
        {
            if (characterController == null)
            {
                yield return new WaitForSeconds(1f);
                if (player != null)
                    characterController = player.GetComponent<CharacterController>();
                continue;
            }

            if (characterController.currentWater >=  characterController.maxWaterCapacity)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            if (characterController.currentWater <= 4f)
            {
                Debug.Log($"[WaterDrop] Spawning SmallWater, current={characterController.currentWater}");
                SpawnPrefab(SmallWater);
                yield return new WaitForSeconds(Random.Range(3f, 5f));
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }

    private IEnumerator BigWaterLoop()
    {
        while (true)
        {
            if (characterController == null)
            {
                yield return new WaitForSeconds(1f);
                if (player != null)
                    characterController = player.GetComponent<CharacterController>();
                continue;
            }

            if (characterController.currentWater >=  characterController.maxWaterCapacity)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            if (characterController.currentWater <= 2f)
            {
                Debug.Log($"[WaterDrop] Spawning BigWater, current={characterController.currentWater}");
                SpawnPrefab(BigWater);
                yield return new WaitForSeconds(Random.Range(5f, 7f));
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }
    private void SpawnPrefab(GameObject prefab)
    {
        if (prefab == null) return;

        float randomX = Random.Range(spawnWidthMin, spawnWidthMax);
        Vector2 spawnPos = new Vector2(randomX, spawnYHeight);

        GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity);
        Rigidbody2D rb = instance.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = instance.AddComponent<Rigidbody2D>();
        }
        // apply gravity scale and drag correctly for Rigidbody2D
        rb.gravityScale = dropGravityScale;
        rb.linearDamping = dropLinearDrag;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // optional: use a constant fall speed instead of physics
        if (useConstantFallSpeed)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(0f, -Mathf.Abs(constantFallSpeed));
        }

        // -------------------------------
        // Ignore collisions with walls
        // -------------------------------
        Collider2D waterCollider = instance.GetComponent<Collider2D>();
        if (waterCollider == null)
        {
            waterCollider = instance.AddComponent<BoxCollider2D>();
        }

        // 找到所有牆 wall colliders
        Collider2D[] wallColliders =
            GameObject.FindGameObjectsWithTag("wall")
                .SelectMany(go => go.GetComponents<Collider2D>())
                .ToArray();

        // 找到所有地板 ground colliders
        Collider2D[] groundColliders =
            GameObject.FindGameObjectsWithTag("ground")
                .SelectMany(go => go.GetComponents<Collider2D>())
                .ToArray();

        // 忽略牆
        foreach (Collider2D wall in wallColliders)
        {
            Physics2D.IgnoreCollision(waterCollider, wall, true);
        }

        // 忽略地板
        foreach (Collider2D ground in groundColliders)
        {
            Physics2D.IgnoreCollision(waterCollider, ground, true);
        }

    }
}