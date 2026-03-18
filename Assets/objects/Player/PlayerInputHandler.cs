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
    private TankPowerHandler powerHandler;
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
        PlayerEvents.OnTankSpawn.Subscribe(SetPowerHandler);
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
        PlayerEvents.OnTankSpawn.Unsubscribe(SetPowerHandler);
    }

    private void SetPowerHandler(GameObject tank)
    {
        powerHandler = tank.GetComponent<TankPowerHandler>();
    }

    private void OnTurboPerformed(InputAction.CallbackContext ctx)
    {
        powerHandler.SetTurbo(true);
    }
    private void OnTurboCanceled(InputAction.CallbackContext ctx)
    {
        powerHandler.SetTurbo(false);
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

    private void Update()
    {
        if (drivableVehicle == null || powerHandler == null) return;

        Vector2 rawInput = moveAction.ReadValue<Vector2>();
        // Round for digital tank controls (keyboard)
        float inpX = Mathf.Round(rawInput.x);
        float inpY = Mathf.Round(rawInput.y);

        drivableVehicle.DriveX = inpX;
        drivableVehicle.DriveY = inpY;

        bool hasMoveInput = Mathf.Abs(inpX + inpY) > 0;

        if (hasMoveInput != didHaveMoveInput)
        {
            powerHandler.SetMove(hasMoveInput);
        }

        didHaveMoveInput = hasMoveInput;
    }
}