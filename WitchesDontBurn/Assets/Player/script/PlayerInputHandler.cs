using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private InputAction flyActions, broomActions, shootWater;
    private CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        flyActions = InputSystem.actions.FindAction("Move");
        broomActions = InputSystem.actions.FindAction("Interact");
        shootWater = InputSystem.actions.FindAction("Attack");


        // enable actions and subscribe safely
        if (flyActions != null)
        {
            flyActions.Enable();
        }


        if (broomActions != null)
        {
            broomActions.performed += Carry;
            broomActions.Enable();
        }

        if (shootWater != null)
        {
            shootWater.performed += ShootWater;
            shootWater.Enable();
        }
        

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
        if (flyActions == null || characterController == null)
        {
            return;
        }

        Vector2 moveInput = flyActions.ReadValue<Vector2>();
        characterController.Move(moveInput);
    }


}
