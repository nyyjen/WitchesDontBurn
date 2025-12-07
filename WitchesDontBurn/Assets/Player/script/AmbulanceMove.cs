using UnityEngine;
using System.Collections;

public class AmbulanceMove : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Speed of the ambulance")]
    public float speed = 5f;
    
    [Header("Spawn Settings")]
    [Tooltip("X position where ambulance starts (left side)")]
    public float startX = -15f;
    [Tooltip("X position where ambulance ends (right side)")]
    public float endX = 15f;
    [Tooltip("Y position of the ambulance (ground level)")]
    public float groundY = 0f;
    
    [Header("NPC Detection")]
    [Tooltip("Distance to check for NPCs on ground")]
    public float npcDetectionDistance = 2f;
    [Tooltip("Layer mask for NPCs")]
    public LayerMask npcLayer;
    
    private Camera mainCamera;
    private bool hasStarted = false;
    
    void Start()
    {
        mainCamera = Camera.main;
        
        // Set initial position to left side
        Vector3 startPos = new Vector3(startX, groundY, 0f);
        
        // If camera exists, adjust start position relative to camera view
        if (mainCamera != null)
        {
            Vector3 cameraLeft = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0.5f, mainCamera.nearClipPlane));
            startPos.x = cameraLeft.x - 2f; // Start slightly off-screen left
        }
        
        transform.position = startPos;
        
        // Start movement
        StartCoroutine(MoveAmbulance());
    }
    
    private IEnumerator MoveAmbulance()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos;
        
        // Calculate end position
        if (mainCamera != null)
        {
            Vector3 cameraRight = mainCamera.ViewportToWorldPoint(new Vector3(1f, 0.5f, mainCamera.nearClipPlane));
            endPos = new Vector3(cameraRight.x + 2f, groundY, 0f); // End slightly off-screen right
        }
        else
        {
            endPos = new Vector3(endX, groundY, 0f);
        }
        
        float distance = Vector3.Distance(startPos, endPos);
        float duration = distance / speed;
        float elapsed = 0f;
        
        // Play ambulance sound
        AkSoundEngine.PostEvent("SFX_AMBULANCE", gameObject);
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Move ambulance
            transform.position = Vector3.Lerp(startPos, endPos, t);
            
            // Check for NPCs on ground
            CheckForNPCs();
            
            yield return null;
        }
        
        // Destroy ambulance when it reaches the end
        Destroy(gameObject);
    }
    
    private void CheckForNPCs()
    {
        // Check for NPCs that have fallen to the ground
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, npcDetectionDistance);
        
        foreach (Collider2D hit in hits)
        {
            if (hit == null) continue;
            
            GameObject npcObject = hit.gameObject;
            
            // Check if it's an NPC that has fallen from window (has NPCfalling component)
            NPCfalling npcFalling = npcObject.GetComponent<NPCfalling>();
            if (npcFalling != null)
            {
                // Check if NPC has landed on ground
                Rigidbody2D npcRb = npcObject.GetComponent<Rigidbody2D>();
                CapsuleCollider2D npcCol = npcObject.GetComponent<CapsuleCollider2D>();
                
                if (npcRb != null && npcCol != null)
                {
                    // NPC has landed if gravity is 0 and velocity is near zero (from NPCfalling logic)
                    bool hasLanded = npcRb.gravityScale == 0 && npcRb.linearVelocity.magnitude < 0.1f;
                    
                    if (hasLanded)
                    {
                        // Verify NPC is on ground by checking collision with ground
                        Vector2 footPos = new Vector2(npcCol.bounds.center.x, npcCol.bounds.min.y);
                        RaycastHit2D groundHit = Physics2D.Raycast(footPos, Vector2.down, 0.5f);
                        
                        if (groundHit.collider != null && groundHit.collider.CompareTag("ground"))
                        {
                            // NPC is on ground and ambulance is passing by - destroy NPC
                            Destroy(npcObject);
                        }
                    }
                }
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, npcDetectionDistance);
    }
}
