public class IdleState : IState
{
    public IState DoState(EnemyTankStateMachine machine)
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