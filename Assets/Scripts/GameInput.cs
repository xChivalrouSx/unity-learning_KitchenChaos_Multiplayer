using System;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class GameInput : MonoBehaviour
{

    public static GameInput Instance { get; private set; }

    public event EventHandler OnInteractAction;
    public event EventHandler OnInteractAlternateAction;
    public event EventHandler OnPauseAction;

    private PlayerInputActions playerInputActions;

    private void Awake()
    {
        Instance = this;

        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();

        playerInputActions.Player.Interact.performed += InteractPerformed;
        playerInputActions.Player.InteractAlternate.performed += InteractAlternatePerformed;
        playerInputActions.Player.Pause.performed += Pause_performed;
    }

    private void OnDestroy()
    {
        playerInputActions.Player.Interact.performed -= InteractPerformed;
        playerInputActions.Player.InteractAlternate.performed -= InteractAlternatePerformed;
        playerInputActions.Player.Pause.performed -= Pause_performed;

        playerInputActions.Dispose();
    }

    private void Pause_performed(CallbackContext obj)
    {
        OnPauseAction?.Invoke(this, EventArgs.Empty);
    }

    private void InteractAlternatePerformed(CallbackContext obj)
    {
        OnInteractAlternateAction?.Invoke(this, EventArgs.Empty);
    }

    private void InteractPerformed(CallbackContext obj)
    {
        OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    public Vector3 GetMovementVectorNormalized()
    {
        return playerInputActions.Player.Move.ReadValue<Vector3>().normalized;
    }

    //public Vector3 GetMovementVectorNormalizedOld()
    //{
    //    Vector3 inputVector = new Vector3(0, 0, 0);
    //    if (Input.GetKey(KeyCode.W))
    //    {
    //        inputVector.z += 1;
    //    }
    //    if (Input.GetKey(KeyCode.S))
    //    {
    //        inputVector.z -= 1;
    //    }
    //    if (Input.GetKey(KeyCode.A))
    //    {
    //        inputVector.x -= 1;
    //    }
    //    if (Input.GetKey(KeyCode.D))
    //    {
    //        inputVector.x += 1;
    //    }
    //    return inputVector.normalized;
    //}
}
