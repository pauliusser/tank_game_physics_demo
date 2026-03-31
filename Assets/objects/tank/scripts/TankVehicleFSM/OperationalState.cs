using UnityEngine;

public class OperationalState : IState<TankVehicleFSM>
{
    private bool isUpsideDown = false;
    private float capsizedTimer = 0f;
    public IState<TankVehicleFSM> DoState(TankVehicleFSM machine)
    {
        if (machine.stats.Health <= 0) return new DeathState();
        if (IsCapsized(machine)) return new StuckState();
        return this;
    }

    private bool IsCapsized(TankVehicleFSM machine)
    {
        isUpsideDown = machine.body.transform.up.y <= machine.capsizethreshold;
        if (isUpsideDown) capsizedTimer += Time.deltaTime;
        else capsizedTimer = 0f;
        return capsizedTimer >= machine.stuckTimeout;
    }
}
