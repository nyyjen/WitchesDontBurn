using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private InputAction flyActions, broomActions;
    private CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        flyActions = InputSystem.actions.FindAction("Move");
        broomActions = InputSystem.actions.FindAction("Interact");


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

    }

    private void Carry(InputAction.CallbackContext context)
    {
        if (characterController != null)
        {
            characterController.UseBroom();
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
