using UnityEngine;

public class StuckState : IState<TankVehicleFSM>
{
    private bool isRecovering = false;
    private float recoveryTimespan = 0.5f;
    private float recoveryStart;
    private Vector3 stuckPos;
    private Quaternion stuckRot;
    private Vector3 recoverPos;
    private Quaternion recoverRot;
    private bool isRecovered = false;
    private bool isRecoverable = true;
    private TankTurret turret;
    public IState<TankVehicleFSM> DoState(TankVehicleFSM machine)
    {
        if (!isRecovering)
        {
            Debug.Log("tankas apvirto");
            isRecovering = true;
            recoveryStart = Time.time;
            stuckPos = machine.transform.position;
            stuckRot = machine.transform.rotation;
            FindRecoverPosition(machine);
            turret = machine.deathHandler.turret.GetComponent<TankTurret>();
            turret.isEnabled = false;
        }
        else 
        {
            Recover(machine);
            turret.RotateTurretToHome();
            turret.RotateGunToHome();
        }

        if (!isRecoverable)
        {
            turret.isEnabled = true;
            return new DeathState();
        }

        if (isRecovered) 
        {
            turret.isEnabled = true;
            return new OperationalState();
        }

        return this;
    }
    private void Recover(TankVehicleFSM machine)
    {
        float recoveryPercentage = (Time.time - recoveryStart) / recoveryTimespan;
        float tPos = Mathf.Clamp01(recoveryPercentage * 2);
        float tRot = Mathf.Clamp01(recoveryPercentage * 2 - 1);
        machine.transform.position = Vector3.Lerp(stuckPos, recoverPos, tPos);
        machine.transform.rotation = Quaternion.Slerp(stuckRot, recoverRot, tRot);
        isRecovered = recoveryPercentage >= 1f;
    }
    private void FindRecoverPosition(TankVehicleFSM machine)
    {
        isRecoverable = NavMeshPointFinder.TryGetClosestNavMeshPoint(machine.gameObject,out recoverPos, 2f);
        recoverPos.y += 0.7f;

        Quaternion rotation = Quaternion.FromToRotation(machine.transform.up, Vector3.up);
        recoverRot = rotation * machine.transform.rotation;
    }
}
