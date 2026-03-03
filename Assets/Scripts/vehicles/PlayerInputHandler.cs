using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Action")]
    public InputAction moveAction;   // 2D vector: x = turn, y = throttle
    public InputAction recoverAction;
    private IDrivable drivableVehicle;
    private TankVehicleFSM fsm;

    private void OnEnable()
    {
        moveAction.Enable();
        recoverAction.Enable();
        recoverAction.performed += OnRecoverPerformed;

        fsm = GetComponent<TankVehicleFSM>();
    }
    private void OnDisable()
    {
        moveAction.Disable();
        recoverAction.Disable();
        recoverAction.performed -= OnRecoverPerformed;
    }
    private void OnRecoverPerformed(InputAction.CallbackContext ctx)
    {
        fsm.Recover();
    }

    private void Awake()
    {
        // Get the first component that implements IDrivable on this GameObject
        drivableVehicle = GetComponent<IDrivable>();
        if (drivableVehicle == null)
        {
            Debug.LogError("PlayerInputHandler: No component implementing IDrivable found on " + gameObject.name);
        }
    }

    private void Update()
    {
        if (drivableVehicle == null) return;

        Vector2 rawInput = moveAction.ReadValue<Vector2>();
        // Round for digital tank controls (keyboard)
        float inpX = Mathf.Round(rawInput.x);
        float inpY = Mathf.Round(rawInput.y);

        drivableVehicle.DriveX = inpX;
        drivableVehicle.DriveY = inpY;
    }
}