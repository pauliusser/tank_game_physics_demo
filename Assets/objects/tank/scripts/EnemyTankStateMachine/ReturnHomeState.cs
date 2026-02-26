using UnityEngine;

public class ReturnHomeState : IState
{
    private bool isReturning = false;
    public IState DoState(EnemyTankStateMachine machine)
    {
        if (machine.state != "return home state") machine.state = "return home state";
        DoReturnToHomeBehaviour (machine);

        if (machine.patrolState.EncounteredEnemy(machine))
        {
            Reset(machine);
            return machine.attackState;
        }
        else if (isHasReturned(machine))
        {
            Debug.Log("tank has returned");
            return new IdleState();
        }
        else
        {
            return this; // stay in BarState
        }
    }
    private bool isHasReturned(EnemyTankStateMachine machine)
    {
        float distance = (machine.tankBody.transform.position - machine.homepoint.transform.position).magnitude;
        return distance <= machine.arivalDistance;
    }
    void DoReturnToHomeBehaviour (EnemyTankStateMachine machine)
    {
        if (isReturning) return;
        machine.navController.navTarget = machine.homepoint;
        isReturning = true;
    }
    void Reset(EnemyTankStateMachine machine)
    {
        isReturning = false;
        machine.navController.navTarget = null;
    }
}