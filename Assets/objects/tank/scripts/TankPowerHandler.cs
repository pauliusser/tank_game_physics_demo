using UnityEngine;
using UnityEngine.PlayerLoop;

public class TankPowerHandler : MonoBehaviour
{
    public TankMovement tankMovement;
    public float capacitorDischargeTimeSpan = 1f;
    public float capacitorChargeTimeSpan = 5f;
    public float batteryDischargeRate = 3.33f;
    public float turboSpeedMultiplier = 2;
    private TankStats stats;
    private bool isTurboBtn = false;
    private bool isMoveInputs = false;
    private bool isTurboActive = false;
    private int prevCapacitor;
    private int curCapacitor;
    private int prevBattery;
    private int currBattery;
    private float inintialSpeed;
    void Awake()
    {
        stats = GetComponent<TankStats>();
        inintialSpeed = tankMovement.trackSpeedCmPerSecond;
    }
    public void SetTurbo (bool active)
    {
        isTurboBtn = active;
        isTurboActive = isTurboBtn && isMoveInputs;
    }
    public void SetMove (bool active)
    {
        isMoveInputs = active;
        isTurboActive = isTurboBtn && isMoveInputs;
    }
    void Update()
    {
        bool isDischargingCap = isTurboActive && stats.capacitor > 0f;

        if (isDischargingCap)
        {
            stats.capacitor -= Time.deltaTime * (stats.maxCapacitor / capacitorDischargeTimeSpan);
            curCapacitor = (int) stats.capacitor;
            if (stats.capacitor < 0f) stats.capacitor = 0f;
            tankMovement.trackSpeedCmPerSecond = inintialSpeed * turboSpeedMultiplier;
        }
        else if (!isDischargingCap && stats.battery > 10f && stats.capacitor < stats.maxCapacitor)
        {
            float chargeValue = Time.deltaTime * (stats.maxCapacitor / capacitorChargeTimeSpan);
            // discharge battery
            stats.battery -= chargeValue;
            currBattery = (int) stats.battery;
            // charge capacitor
            stats.capacitor += chargeValue;
            curCapacitor = (int) stats.capacitor;
            tankMovement.trackSpeedCmPerSecond = inintialSpeed;
        }
        else if (isMoveInputs)
        {
            float batteryTimeCapacity = stats.maxBattery / batteryDischargeRate;
            stats.battery -= Time.deltaTime * (stats.maxBattery / batteryTimeCapacity);
            currBattery = (int) stats.battery;
            tankMovement.trackSpeedCmPerSecond = inintialSpeed;
        }

        bool capUINeedUpdate = prevCapacitor != curCapacitor;
        bool battUINeedUpdate = prevBattery != currBattery;

        if (capUINeedUpdate) GameEvents.OnCapacitorUpdate.Invoke(stats.capacitor / stats.maxCapacitor);
        if (battUINeedUpdate) GameEvents.OnBatteryUpdate.Invoke(stats.battery / stats.maxBattery);

        prevBattery = currBattery;
        prevCapacitor = curCapacitor;
    }
}
