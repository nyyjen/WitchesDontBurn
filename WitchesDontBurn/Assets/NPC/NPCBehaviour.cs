using UnityEngine;
using System.Collections;

public class NPCBehaviour : MonoBehaviour
{
    [Header("NPC Type")]
    [SerializeField] private bool isPet = false;
    [SerializeField] private bool isHuman = true;
    [SerializeField] private bool isFish = false;
    
    [Header("Walk Settings")]
    public float walkSpeed = 1.2f;
    public LayerMask Wall;
    public float wallCheckDistance = 0.2f;
    
    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.45f;
    
    private Animator animator;
    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private GameObject associatedWindow;
    private bool isBeingPickedUp = false;
    private bool isWalking = false;
    private int walkDirection = -1; // -1 = left, 1 = right
    private NPCfalling npcFalling;
    
    // Disappear delays
    private float disappearDelay = 0.6f; // Default for humans
    
    // Track position to detect if being moved by player
    private Vector3 lastPosition;
    private float positionCheckInterval = 0.1f;
    private float lastPositionCheckTime = 0f;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        npcFalling = GetComponent<NPCfalling>();
        
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 0;
        }
        
        // Set disappear delay based on NPC type
        if (isFish)
        {
            disappearDelay = 0.5f;
        }
        else if (isPet)
        {
            disappearDelay = 0.3f;
        }
        else if (isHuman)
        {
            disappearDelay = 0.6f;
        }
    }
    
    void Start()
    {
        lastPosition = transform.position;
        
        // Check if NPC is on ground (dropped by player) or at window
        bool isOnGround = CheckIfOnGround();
        
        if (isOnGround)
        {
            // NPC was dropped by player - start walking
            OnDropped();
        }
        else
        {
            // NPC is at window - set calling animation
            if (animator != null)
            {
                animator.SetTrigger("isCalling");
                PlayCallingSound();
            }
            
            // Find associated window
            FindAssociatedWindow();
            
            // Check if window has big fire
            CheckForBigFire();
        }
    }

    private void PlayCallingSound()
    {
        string npcName = gameObject.name.ToLower();

        // Map NPC names to carrying parameters
        if (npcName.Contains("boy"))
        {
            AkSoundEngine.PostEvent("VO_Help_Boy", gameObject);
        }
        else if (npcName.Contains("girl"))
        {
            AkSoundEngine.PostEvent("VO_Help_Girl", gameObject);
        }
        else if (npcName.Contains("men") || npcName.Contains("man"))
        {
            AkSoundEngine.PostEvent("VO_Help_Man", gameObject);
        }
        else if (npcName.Contains("women") || npcName.Contains("woman"))
        {
            AkSoundEngine.PostEvent("VO_Help_Woman", gameObject);
        }
        else if (npcName.Contains("dog") || npcName.Contains("puppy"))
        {
            AkSoundEngine.PostEvent("VO_Help_Dog", gameObject);
        }
        else if (npcName.Contains("elder"))
        {
            AkSoundEngine.PostEvent("VO_Help_Old", gameObject);
        }
    }

    private bool CheckIfOnGround()
    {
        if (col == null) return false;
        
        Vector2 footPos = new Vector2(col.bounds.center.x, col.bounds.min.y);
        RaycastHit2D hit = Physics2D.Raycast(footPos, Vector2.down, groundCheckDistance, groundLayer);
        
        return hit.collider != null;
    }
    
    void Update()
    {
        // Handle walking movement
        if (isWalking && animator != null && animator.GetBool("isWalking"))
        {
            WalkMovement();
        }
        
        // Detect if NPC is being moved by player (pickup)
        if (!isBeingPickedUp && !isWalking && Time.time - lastPositionCheckTime > positionCheckInterval)
        {
            CheckIfBeingPickedUp();
            lastPositionCheckTime = Time.time;
        }
    }
    
    private void CheckIfBeingPickedUp()
    {
        // Check if NPC is being moved rapidly (indicating player pickup)
        Vector3 currentPos = transform.position;
        float moveDistance = Vector3.Distance(currentPos, lastPosition);
        
        // If NPC moved significantly and is near a player, it's being picked up
        if (moveDistance > 0.01f && !isWalking)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                CharacterController cc = player.GetComponent<CharacterController>();
                DetectTrigger dt = player.GetComponentInChildren<DetectTrigger>();
                
                // Check if player is carrying NPC and this NPC is the one being carried
                // Also check if NPC is moving towards player (indicating pickup)
                if (cc != null)
                {
                    float distToPlayer = Vector2.Distance(transform.position, player.transform.position);
                    
                    // If player is carrying NPC or NPC is moving towards player and close enough
                    if ((dt != null && dt.inWindowRange && dt.npcOnWindow == gameObject) ||
                        (cc.isCarryingNPC && distToPlayer < 3f))
                    {
                        // NPC is being picked up by player
                        OnBeingPickedUp();
                    }
                }
            }
        }
        
        lastPosition = currentPos;
    }
    
    private void OnBeingPickedUp()
    {
        if (isBeingPickedUp) return;
        
        isBeingPickedUp = true;
        StopAllCoroutines();
        
        // Set player's carrying animation parameter based on NPC type
        SetPlayerCarryingAnimation();
        AkSoundEngine.PostEvent("SFX_ResidentRescued", gameObject);

        StartCoroutine(DisappearAfterDelay());
    }
    
    private void SetPlayerCarryingAnimation()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc == null) return;
        
        Animator playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator == null) return;
        
        // Get NPC type from gameObject name
        string npcName = gameObject.name.ToLower();
        string carryingParam = "";
        
        // Map NPC names to carrying parameters
        if (npcName.Contains("boy"))
        {
            carryingParam = "carryingBoy";
        }
        else if (npcName.Contains("girl"))
        {
            carryingParam = "carryingGirl";
        }
        else if (npcName.Contains("men") || npcName.Contains("man"))
        {
            carryingParam = "carryingMan";
        }
        else if (npcName.Contains("women") || npcName.Contains("woman"))
        {
            carryingParam = "carryingWoman";
        }
        else if (npcName.Contains("fish"))
        {
            carryingParam = "carryingFish";
        }
        else if (npcName.Contains("dog") || npcName.Contains("puppy"))
        {
            carryingParam = "carryingPuppy";
        }
        else if (npcName.Contains("elder"))
        {
            // Elder might use carryingMan or a separate parameter
            carryingParam = "carryingMan"; // Default to man if no specific parameter
        }
        
        // Set the carrying parameter to true
        if (!string.IsNullOrEmpty(carryingParam))
        {
            playerAnimator.SetBool(carryingParam, true);
        }
    }
    
    private IEnumerator DisappearAfterDelay()
    {
        yield return new WaitForSeconds(disappearDelay);
        
        // Reset player's carrying animation before destroying
        ResetPlayerCarryingAnimation();
        
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
    
    private void OnDestroy()
    {
        // Ensure carrying parameter is reset when NPC is destroyed
        ResetPlayerCarryingAnimation();
    }
    
    private void FindAssociatedWindow()
    {
        // Find the closest window to this NPC
        GameObject[] windows = GameObject.FindGameObjectsWithTag("Window");
        float closestDist = float.MaxValue;
        GameObject closestWindow = null;
        
        foreach (GameObject window in windows)
        {
            float dist = Vector2.Distance(transform.position, window.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestWindow = window;
            }
        }
        
        if (closestDist < 2f) // Within reasonable distance
        {
            associatedWindow = closestWindow;
        }
    }
    
    private void CheckForBigFire()
    {
        if (associatedWindow == null) return;
        
        WindowBehaviour windowBehaviour = associatedWindow.GetComponent<WindowBehaviour>();
        if (windowBehaviour == null) return;
        
        // Check if window is on fire and has been burning for >= 2.5 seconds (big fire)
        // Since timeOnFire is private, we'll use a coroutine to check periodically
        StartCoroutine(MonitorWindowFire());
    }
    
    private IEnumerator MonitorWindowFire()
    {
        if (associatedWindow == null || npcFalling == null) yield break;
        
        float fireStartTime = Time.time;
        bool wasOnFire = false;
        
        while (associatedWindow != null && !isBeingPickedUp && !isWalking)
        {
            WindowBehaviour windowBehaviour = associatedWindow.GetComponent<WindowBehaviour>();
            if (windowBehaviour != null && windowBehaviour.IsOnFire())
            {
                if (!wasOnFire)
                {
                    // Fire just started
                    fireStartTime = Time.time;
                    wasOnFire = true;
                }
                
                // Check if fire has been burning for >= 2.5 seconds (big fire)
                float timeOnFire = Time.time - fireStartTime;
                if (timeOnFire >= 2.5f && npcFalling != null && !npcFalling.enabled)
                {
                    // Enable NPCfalling component - window has big fire
                    npcFalling.enabled = true;
                    // Disable this component's normal behavior
                    enabled = false;
                    yield break;
                }
            }
            else
            {
                wasOnFire = false;
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    
    
    // Called when NPC is dropped by player (new instance created)
    public void OnDropped()
    {
        // Reset player's carrying animation parameter
        ResetPlayerCarryingAnimation();
        
        // Set isWalking trigger
        if (animator != null)
        {
            animator.SetTrigger("isWalking");
            animator.SetBool("isWalking", true);
        }
        
        isWalking = true;
        
        // Randomly choose walk direction
        walkDirection = Random.value < 0.5f ? -1 : 1;
        
        // For fish, disappear after 0.5 seconds
        if (isFish)
        {
            StartCoroutine(DisappearFish());
        }
    }
    
    private void ResetPlayerCarryingAnimation()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        Animator playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator == null) return;
        
        // Get NPC type from gameObject name
        string npcName = gameObject.name.ToLower();
        string carryingParam = "";
        
        // Map NPC names to carrying parameters
        if (npcName.Contains("boy"))
        {
            carryingParam = "carryingBoy";
        }
        else if (npcName.Contains("girl"))
        {
            carryingParam = "carryingGirl";
        }
        else if (npcName.Contains("men") || npcName.Contains("man"))
        {
            carryingParam = "carryingMan";
        }
        else if (npcName.Contains("women") || npcName.Contains("woman"))
        {
            carryingParam = "carryingWoman";
        }
        else if (npcName.Contains("fish"))
        {
            carryingParam = "carryingFish";
        }
        else if (npcName.Contains("dog") || npcName.Contains("puppy"))
        {
            carryingParam = "carryingPuppy";
        }
        else if (npcName.Contains("elder"))
        {
            carryingParam = "carryingMan"; // Default to man if no specific parameter
        }
        
        // Reset the carrying parameter to false
        if (!string.IsNullOrEmpty(carryingParam))
        {
            playerAnimator.SetBool(carryingParam, false);
        }
    }
    
    private IEnumerator DisappearFish()
    {
        yield return new WaitForSeconds(0.5f);
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
    
    private void WalkMovement()
    {
        if (rb == null) return;
        
        rb.linearVelocity = new Vector2(walkDirection * walkSpeed, 0);
        
        // Flip sprite based on direction
        transform.localScale = new Vector3(-walkDirection, 1, 1);
        
        // Check for walls
        if (col != null)
        {
            Vector2 origin = new Vector2(
                col.bounds.center.x + walkDirection * col.bounds.extents.x,
                col.bounds.center.y
            );
            
            Vector2 dir = new Vector2(walkDirection, 0);
            
            Debug.DrawRay(origin, dir * wallCheckDistance, Color.green);
            
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, dir, wallCheckDistance);
            
            foreach (var hit in hits)
            {
                if (hit.collider != null && hit.collider.CompareTag("wall"))
                {
                    Destroy(gameObject); // NPC disappears when hitting wall
                    return;
                }
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // If NPC is at window and player picks it up, this will be called
        // But the actual pickup is handled by PlayerInputHandler
    }
    
    // Transform NPC to cat
    public void TransformToCat(CharacterController playerController)
    {
        if (playerController == null || !playerController.CanTransformToCat()) return;
        if (isBeingPickedUp || isWalking) return; // Don't transform if NPC is being picked up or walking
        
        // Use transform count
        playerController.UseTransformCount();
        
        // Set player's casting animation
        SetPlayerCastingAnimation();
        
        // Stop all coroutines
        StopAllCoroutines();
        
        // Start transformation sequence
        StartCoroutine(TransformToCatSequence(playerController));
    }
    
    private void SetPlayerCastingAnimation()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        Animator playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator == null) return;
        
        // Set isCasting trigger
        playerAnimator.SetTrigger("isCasting");
    }
    
    private IEnumerator TransformToCatSequence(CharacterController playerController)
    {
        Vector3 npcPosition = transform.position;
        
        // NPC disappears immediately
        if (animator != null)
        {
            animator.enabled = false;
        }
        SpriteRenderer npcRenderer = GetComponent<SpriteRenderer>();
        if (npcRenderer != null)
        {
            npcRenderer.enabled = false;
        }
        if (col != null) col.enabled = false;
        
        // Disable all other components to make NPC invisible but keep it alive for the coroutine
        MonoBehaviour[] components = GetComponents<MonoBehaviour>();
        foreach (var comp in components)
        {
            if (comp != this && comp != null)
            {
                comp.enabled = false;
            }
        }
        
        // Spawn magic effect
        GameObject magicInstance = null;
        if (playerController.magicPrefab != null)
        {
            magicInstance = Instantiate(playerController.magicPrefab, npcPosition, Quaternion.identity);
        }
        
        // Wait for magic effect - check if magic has Animator, otherwise use default time
        float magicWaitTime = 1.0f; // Default wait time
        if (magicInstance != null)
        {
            Animator magicAnimator = magicInstance.GetComponent<Animator>();
            if (magicAnimator != null && magicAnimator.runtimeAnimatorController != null)
            {
                // If magic has animator, wait for animation to complete
                // Get the length of the first animation clip
                AnimationClip[] clips = magicAnimator.runtimeAnimatorController.animationClips;
                if (clips != null && clips.Length > 0)
                {
                    magicWaitTime = clips[0].length;
                }
            }
        }
        
        yield return new WaitForSeconds(magicWaitTime);
        
        // Destroy magic effect
        if (magicInstance != null)
        {
            Destroy(magicInstance);
        }
        
        // Spawn cat at the same position
        if (playerController.catPrefab != null)
        {
            Instantiate(playerController.catPrefab, npcPosition, Quaternion.identity);
        }
        
        // Destroy NPC
        Destroy(gameObject);
    }
}
