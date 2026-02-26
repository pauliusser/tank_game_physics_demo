public interface IState
{
    // This method does the state's work and returns the next state
    IState DoState(EnemyTankStateMachine fsm);
}