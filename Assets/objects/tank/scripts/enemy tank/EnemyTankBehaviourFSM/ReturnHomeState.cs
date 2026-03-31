using UnityEngine;

public class ReturnHomeState : IState<EnemyTankBehaviourFSM>
{
    private bool isReturning = false;

    public IState<EnemyTankBehaviourFSM> DoState(EnemyTankBehaviourFSM machine)
    {
        if (machine.state != "return home state") machine.state = "return home state";
        DoReturnToHomeBehaviour(machine);

        if (machine.turretInputs.FindNearestTarget(machine.targetLayerMask, machine.obstacleMask, machine.detectionRadius))
        {
            Reset(machine);
            return machine.attackState;
        }
        else if (IsHasReturned(machine))
        {
            Debug.Log("tank has returned");
            return new IdleState();
        }
        else
        {
            return this;
        }
    }

    private bool IsHasReturned(EnemyTankBehaviourFSM machine)
    {
        float distance = (machine.tankBody.transform.position - machine.homepoint.transform.position).magnitude;
        return distance <= machine.arivalDistance;
    }

    void DoReturnToHomeBehaviour(EnemyTankBehaviourFSM machine)
    {
        if (isReturning) return;
        machine.navController.navTarget = machine.homepoint;
        isReturning = true;
    }

    void Reset(EnemyTankBehaviourFSM machine)
    {
        isReturning = false;
        machine.navController.navTarget = null;
    }
}