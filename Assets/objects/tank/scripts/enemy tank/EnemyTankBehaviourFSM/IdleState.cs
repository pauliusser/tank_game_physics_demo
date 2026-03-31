public class IdleState : IState<EnemyTankBehaviourFSM>
{
    public IState<EnemyTankBehaviourFSM> DoState(EnemyTankBehaviourFSM machine)
    {
        if (machine.state != "idle state") machine.state = "idle state";

        if (machine.turretInputs.FindNearestTarget(machine.targetLayerMask, machine.obstacleMask, machine.detectionRadius))
        {
            return machine.attackState;
        }
        else
        {
            return this;
        }
    }
}