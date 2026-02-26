using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Action")]
    public InputAction moveAction;   // 2D vector: x = turn, y = throttle

    private IDrivable drivableVehicle;

    private void OnEnable() => moveAction.Enable();
    private void OnDisable() => moveAction.Disable();

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