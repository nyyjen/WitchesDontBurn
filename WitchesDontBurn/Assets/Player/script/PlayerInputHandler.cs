using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private InputAction flyActions, broomActions, shootWater;
    private CharacterController characterController;
    private System.Action<InputAction.CallbackContext> carryCallback;
    private System.Action<InputAction.CallbackContext> shootCallback;
    // flags set by input callbacks, processed in Update to avoid doing game logic inside callbacks
    private bool broomRequested = false;
    private bool shootRequested = false;
    

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        flyActions = InputSystem.actions.FindAction("Move");
        broomActions = InputSystem.actions.FindAction("Interact");
        shootWater = InputSystem.actions.FindAction("Attack");
        // prepare the callbacks so removes will always match adds
        // callbacks only set a flag — real work runs in Update()
        carryCallback = (ctx) => {
            broomRequested = true;
        };

        shootCallback = (ctx) => {
            shootRequested = true;
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
            shootWater.performed -= shootCallback;
            shootWater.performed += shootCallback;
            shootWater.Enable();
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
            shootWater.performed -= shootCallback;
            shootWater.Disable();
        }

        if (flyActions != null)
            flyActions.Disable();
    }

    private void Carry(InputAction.CallbackContext context)
    {
        if (characterController != null)
        {
            characterController.UseBroom();
        }
    }

    private void ShootWater(InputAction.CallbackContext context)
    {
        if (characterController != null)
        {
            characterController.ShootWater();
        }
    }
    
    void Update()
    {
        if (flyActions != null && characterController != null)
        {
            Vector2 moveInput = flyActions.ReadValue<Vector2>();
            characterController.Move(moveInput);
        }

        // process requests queued by input callbacks — do gameplay work here (safe)
        if (broomRequested)
        {
            broomRequested = false;
            if (characterController != null)
                characterController.UseBroom();
        }

        if (shootRequested)
        {
            shootRequested = false;
            if (characterController != null)
                characterController.ShootWater();
        }
    }


}
