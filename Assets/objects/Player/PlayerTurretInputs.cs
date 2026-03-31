using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerTurretInputs : MonoBehaviour
{
    [Header("Input Actions")]
    public InputAction fireAction;
    public InputAction cycleAction;
    public InputAction scrollAction;
    public InputAction digitAction;

    [Header("Settings")]
    public float scrollSensitivity = 1f;
    public Transform mouseTarget; // Assign the MouseTarget GameObject here
    private ITurretControllable turretController;
    public void SetTarget(ITurretControllable currentTurret)
    {
        turretController = currentTurret;
    }

    // private void Awake()
    // {
    //     turretController = GetComponent<ITurretControllable>();
    //     if (turretController == null)
    //         turretController = GetComponentInParent<ITurretControllable>();

    //     if (turretController == null)
    //         Debug.LogError("PlayerTurretInputs: No ITurretControllable found.");
    // }

    private void OnEnable()
    {
        fireAction.Enable();
        fireAction.performed += OnFirePerformed;

        cycleAction.Enable();
        cycleAction.performed += OnCyclePerformed;

        scrollAction.Enable();

        if (digitAction != null)
        {
            digitAction.Enable();
            digitAction.performed += OnDigitPerformed;
        }
    }

    private void OnDisable()
    {
        fireAction.performed -= OnFirePerformed;
        fireAction.Disable();

        cycleAction.performed -= OnCyclePerformed;
        cycleAction.Disable();

        scrollAction.Disable();

        if (digitAction != null)
        {
            digitAction.performed -= OnDigitPerformed;
            digitAction.Disable();
        }
    }

    private void Update()
    {
        if (turretController == null) return;

        // Set the target to the mouse cursor position
        if (mouseTarget != null)
            turretController.Target = mouseTarget;

        // Handle scroll
        Vector2 scroll = scrollAction.ReadValue<Vector2>();
        float delta = scroll.y * scrollSensitivity;
        if (Mathf.Abs(delta) > 0.01f) // small deadzone
            turretController.AddPitchDelta(delta);
    }

    private void OnFirePerformed(InputAction.CallbackContext ctx)
    {
        turretController?.Fire();
    }

    private void OnCyclePerformed(InputAction.CallbackContext ctx)
    {
        if (turretController == null || turretController.ProjectileCount == 0) return;
        int newIndex = (turretController.CurrentProjectileIndex + 1) % turretController.ProjectileCount;
        turretController.SetProjectileIndex(newIndex);
    }

    private void OnDigitPerformed(InputAction.CallbackContext ctx)
    {
        if (turretController == null) return;
        if (ctx.control.name.Length == 1 && char.IsDigit(ctx.control.name[0]))
        {
            int digit = int.Parse(ctx.control.name);
            if (digit >= 1 && digit <= 9)
            {
                int newIndex = Mathf.Clamp(digit - 1, 0, turretController.ProjectileCount - 1);
                turretController.SetProjectileIndex(newIndex);
            }
        }
    }
}