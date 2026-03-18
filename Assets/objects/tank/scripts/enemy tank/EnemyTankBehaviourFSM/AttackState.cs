public class AttackState : IState<EnemyTankBehaviourFSM>
{
    private bool isTarget = true;
    public IState<EnemyTankBehaviourFSM> DoState(EnemyTankBehaviourFSM machine)
    {
        if (machine.state != "Attack state") machine.state = "Attack state";
        DoAttackBehaviour (machine);

        if (!isTarget)
        {
            return machine.patrolState;
        }
        else
        {
            return this;
        }
    }
    void DoAttackBehaviour (EnemyTankBehaviourFSM machine)
    {
        isTarget = machine.turretInputs.FindNearestTarget
        (
            machine.targetLayerMask,
            machine.obstacleMask,
            machine.fireRange
        );
        machine.turretInputs.SetProjectileIndex(machine.defaultProjectileIndex);
        machine.turretInputs.AimTurretToCurrentTarget();
        machine.turretInputs.FireWhenAligned
        (
            machine.fireRate,
            machine.aimTolerance
        );
    }
}