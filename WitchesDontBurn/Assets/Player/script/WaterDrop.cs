using System.Collections;
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
    public bool useConstantFallSpeed = false;
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
        StartCoroutine(SpawnLoop());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator SpawnLoop()
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

            float currentWater = characterController.currentWater; // 確保此欄位是 public 或 internal 可存取

            // 判斷要不要掉大水或小水：
            // 1) 如果水 <= 2 -> 持續掉大水直到 currentWater >= 6（可重複觸發）
            // 2) 否則如果水 <= 4 -> 持續掉小水直到 currentWater >= 6（可重複觸發）
            if (currentWater <= 2f)
            {
                while (characterController != null && characterController.currentWater < 6f)
                {
                    SpawnPrefab(BigWater);
                    float wait = Random.Range(5f, 7f);
                    yield return new WaitForSeconds(wait);
                }
                // 重新評估狀態
                continue;
            }

            if (currentWater <= 4f)
            {
                while (characterController != null && characterController.currentWater < 6f)
                {
                    SpawnPrefab(SmallWater);
                    float wait = Random.Range(3f, 5f);
                    yield return new WaitForSeconds(wait);
                }
                // 重新評估狀態
                continue;
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

        // optional: use a constant fall speed instead of physics
        if (useConstantFallSpeed)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(0f, -Mathf.Abs(constantFallSpeed));
        }
    }
}
