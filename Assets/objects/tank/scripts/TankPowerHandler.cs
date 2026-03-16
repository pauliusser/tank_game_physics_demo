using UnityEngine;

public class TankPowerHandler : MonoBehaviour
{
    public float capacitorDischargeTimeSpan = 1f;
    public float capacitorChargeTimeSpan = 5f;
    private TankStats stats;
    private bool isTurboBtn = false;
    private bool isMoveInputs = false;
    private bool isTurboActive = false;
    void Awake()
    {
        stats = GetComponent<TankStats>();
    }
    void OnEnable()
    {
        GameEvents.OnTurboToggled.Subscribe(SetTurbo);
        GameEvents.OnMoveInputsToggled.Subscribe(SetMove);
    }
    void OnDisable()
    {
        GameEvents.OnTurboToggled.Unsubscribe(SetTurbo);
        GameEvents.OnMoveInputsToggled.Unsubscribe(SetMove);
    }
    void SetTurbo (bool active)
    {
        isTurboBtn = active;
        isTurboActive = isTurboBtn && isMoveInputs;
    }
    void SetMove (bool active)
    {
        isMoveInputs = active;
        isTurboActive = isTurboBtn && isMoveInputs;
    }
    void Start()
    {
        
    }
    void Update()
    {
        bool isDischargingCap = isTurboActive && stats.capacitor > 0f;

        if (isDischargingCap)
        {
            // discharge capacitor
            if (stats.capacitor < 0f) stats.capacitor = 0f;
        }
        else if (!isDischargingCap && stats.battery > 10f)
        {
            // discharge battery at higher rate
            // charge capacitor
        }
        else if (isMoveInputs)
        {
            // discharge battery at regular rate
        }
    }
}
