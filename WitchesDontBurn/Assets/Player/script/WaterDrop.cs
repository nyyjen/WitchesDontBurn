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

            // 判斷要不要掉小水 or 大水，並設定下一次等待時間
            if (currentWater >= 4f && currentWater <= 6f)
            {
                SpawnPrefab(SmallWater);
                float wait = Random.Range(3f, 5f);
                yield return new WaitForSeconds(wait);
            }
            else if (currentWater >= 2f && currentWater <= 6f)
            {
                SpawnPrefab(BigWater);
                float wait = Random.Range(5f, 7f);
                yield return new WaitForSeconds(wait);
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

        // 隨機水平位置，固定高度
        float randomX = Random.Range(spawnWidthMin, spawnWidthMax);
        Vector2 spawnPos = new Vector2(randomX, spawnYHeight);

        GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity);

        // 確保 Rigidbody2D 存在
        Rigidbody2D rb = instance.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = instance.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;
        }
    }
}
