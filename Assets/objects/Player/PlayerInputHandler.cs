using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Action")]
    public InputAction moveAction;   // 2D vector: x = turn, y = throttle
    public InputAction turboAction;
    public InputAction recoverAction;
    public InputAction pauseAction;
    private IDrivable drivableVehicle;
    private TankVehicleFSM fsm;
    private bool didHaveMoveInput = false;

    public void SetTarget(IDrivable currentDrivable, TankVehicleFSM currentFSM)
    {
        drivableVehicle = currentDrivable;
        fsm = currentFSM;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        recoverAction.Enable();
        recoverAction.performed += OnRecoverPerformed;
        pauseAction.Enable();
        pauseAction.performed += OnPausePerformed;
        turboAction.Enable();
        turboAction.performed += OnTurboPerformed;
        turboAction.canceled += OnTurboCanceled;

        fsm = GetComponent<TankVehicleFSM>();

        GameEvents.OnPauseToggled.Subscribe(TogglePause);
    }
    private void OnDisable()
    {
        moveAction.Disable();
        recoverAction.Disable();
        recoverAction.performed -= OnRecoverPerformed;
        pauseAction.Disable();
        pauseAction.performed -= OnPausePerformed;
        turboAction.Disable();
        turboAction.performed -= OnTurboPerformed;
        turboAction.canceled -= OnTurboCanceled;

        GameEvents.OnPauseToggled.Unsubscribe(TogglePause);
    }
    private void OnTurboPerformed(InputAction.CallbackContext ctx)
    {
        GameEvents.OnTurboToggled.Invoke(true);
    }
    private void OnTurboCanceled(InputAction.CallbackContext ctx)
    {
        GameEvents.OnTurboToggled.Invoke(false);
    }
    private void OnRecoverPerformed(InputAction.CallbackContext ctx)
    {
        fsm.Recover();
    }
    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        GameEvents.OnPauseToggled.Invoke();
    }
    private void TogglePause()
    {
        Debug.Log("pause toggle");
        
    }

    // private void Awake()
    // {
    //     // Get the first component that implements IDrivable on this GameObject
    //     drivableVehicle = GetComponent<IDrivable>();
    //     if (drivableVehicle == null)
    //     {
    //         Debug.LogError("PlayerInputHandler: No component implementing IDrivable found on " + gameObject.name);
    //     }
    // }

    private void Update()
    {
        if (drivableVehicle == null) return;

        Vector2 rawInput = moveAction.ReadValue<Vector2>();
        // Round for digital tank controls (keyboard)
        float inpX = Mathf.Round(rawInput.x);
        float inpY = Mathf.Round(rawInput.y);

        drivableVehicle.DriveX = inpX;
        drivableVehicle.DriveY = inpY;

        bool hasMoveInput = (inpX + inpY) > 0;

        if (hasMoveInput != didHaveMoveInput)
        {
            GameEvents.OnMoveInputsToggled.Invoke(hasMoveInput);
        }

        didHaveMoveInput = hasMoveInput;
    }
}