public class IdleState : IState<EnemyTankBehaviourFSM>
{
    public IState<EnemyTankBehaviourFSM> DoState(EnemyTankBehaviourFSM machine)
    {
        if (machine.patrolState.EncounteredEnemy(machine))
        {
            return machine.attackState;
        }
        else
        {
            return this;
        }
    }
}