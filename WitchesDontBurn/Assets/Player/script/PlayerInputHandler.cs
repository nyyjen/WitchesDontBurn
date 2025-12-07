using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerInputHandler : MonoBehaviour
{
    private InputAction flyActions, broomActions, shootWater, rightClickAction;
    private CharacterController characterController;
    private System.Action<InputAction.CallbackContext> carryCallback;
    // flags set by input callbacks, processed in Update to avoid doing game logic inside callbacks
    private bool broomRequested = false;
    private bool rightClickRequested = false;
    private float holdTime = 0f;      
    private bool hasFired = false;
    private bool hasTriggeredWatering = false; // Track if isWatering trigger has been set  
    

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        flyActions = InputSystem.actions.FindAction("Move");
        broomActions = InputSystem.actions.FindAction("Interact");
        shootWater = InputSystem.actions.FindAction("Attack");
        rightClickAction = InputSystem.actions.FindAction("RightClick");
        // prepare the callbacks so removes will always match adds
        // callbacks only set a flag — real work runs in Update()
        carryCallback = (ctx) => {
            broomRequested = true;
        };
    }

    private void OnEnable()
    {
        if (flyActions != null)
            flyActions.Enable();

        if (broomActions != null)
        {
            broomActions.performed -= carryCallback;
            broomActions.performed += carryCallback;
            broomActions.Enable();
        }

        if (shootWater != null)
        {
            shootWater.performed -= carryCallback;
            shootWater.performed += carryCallback;
            shootWater.Enable();
        }
        
        if (rightClickAction != null)
        {
            rightClickAction.performed += OnRightClick;
            rightClickAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (broomActions != null)
        {
            broomActions.performed -= carryCallback;
            broomActions.Disable();
        }

        if (shootWater != null)
        {
            shootWater.performed -= carryCallback;
            shootWater.Disable();
        }
        
        if (rightClickAction != null)
        {
            rightClickAction.performed -= OnRightClick;
            rightClickAction.Disable();
        }

        if (flyActions != null)
            flyActions.Disable();
    }
    
    private void OnRightClick(InputAction.CallbackContext context)
    {
        rightClickRequested = true;
    }

    private void Carry(InputAction.CallbackContext context)
    {
        if (characterController != null)
        {
            characterController.UseBroom();
        }
    }

    private void DropNPC()
    {
        characterController.isCarryingNPC = false;

        Vector3 dropPos = characterController.transform.position + new Vector3(0, -1f, 0);
        Instantiate(characterController.npcPrefabToDrop, dropPos, Quaternion.identity);
    }


    private void ShootWater(InputAction.CallbackContext context)
    {
        if (characterController != null)
        {
            characterController.ShootWater();
        }
    }

    private IEnumerator PickupNPC(GameObject npc)
    {
        characterController.isCarryingNPC = true;

        Vector3 start = npc.transform.position;
        Vector3 end = characterController.transform.position;

        float t = 0f;
        float duration = 0.6f;

        while (t < duration)
        {
            npc.transform.position = Vector3.Lerp(start, end, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        Destroy(npc);
        characterController.npcOnWindow = null;
    }

    
    void Update()
    {
        if (flyActions != null && characterController != null)
        {
            Vector2 moveInput = flyActions.ReadValue<Vector2>();
            characterController.Move(moveInput);
        }

        if (shootWater != null)
        {
            float attackValue = shootWater.ReadValue<float>(); // 0 or 1

            if (attackValue > 0.5f)   
            {
                holdTime += Time.deltaTime;
                
                // Set isWatering trigger when charging (before firing) - only once
                if (holdTime > 0f && holdTime < 0.5f && !hasFired && !hasTriggeredWatering)
                {
                    if (characterController != null)
                    {
                        characterController.SetWateringTrigger();
                        hasTriggeredWatering = true;
                    }
                }

                if (!hasFired && holdTime >= 0.5f)
                {
                    if (characterController != null)
                    {
                        characterController.ShootWater();
                    }
                    hasFired = true;
                }
            }
            else             
            {
                // Reset when released
                if (holdTime > 0f && hasFired)
                {
                    // Set isFlying when done shooting
                    if (characterController != null)
                    {
                        characterController.SetFlyingBool(true);
                    }
                }
                holdTime = 0f;
                hasFired = false;
                hasTriggeredWatering = false; // Reset watering trigger flag
            }
        }

        // process requests queued by input callbacks — do gameplay work here (safe)
        if (broomRequested)
        {
            broomRequested = false;
            DetectTrigger dt = GetComponentInChildren<DetectTrigger>();
            if (!characterController.isCarryingNPC && dt.inWindowRange && dt.npcOnWindow != null)
            {
                StartCoroutine(PickupNPC(dt.npcOnWindow));
                return;
            }

            // 情況 2：抱著 NPC 且站在地上 → 放下
            if (characterController.isCarryingNPC && dt.inGroundRange)
            {
                DropNPC();
                return;
            }
        }
        
        // Handle right click to transform NPC to cat
        if (rightClickRequested)
        {
            rightClickRequested = false;
            HandleRightClickTransform();
        }
    }
    
    private void HandleRightClickTransform()
    {
        if (!characterController.CanTransformToCat()) return;
        
        // Check if mouse is over an NPC
        if (Mouse.current == null) return;
        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        
        Camera cam = Camera.main;
        if (cam == null) return;
        
        Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 0f));
        mouseWorld.z = 0f;
        
        // Raycast to find NPC under mouse
        RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero, 0.1f);
        
        if (hit.collider != null)
        {
            GameObject hitObject = hit.collider.gameObject;
            NPCBehaviour npcBehaviour = hitObject.GetComponent<NPCBehaviour>();
            
            // Also check parent
            if (npcBehaviour == null && hitObject.transform.parent != null)
            {
                npcBehaviour = hitObject.transform.parent.GetComponent<NPCBehaviour>();
                hitObject = hitObject.transform.parent.gameObject;
            }
            
            if (npcBehaviour != null)
            {
                // Transform NPC to cat
                npcBehaviour.TransformToCat(characterController);
            }
        }
    }


}