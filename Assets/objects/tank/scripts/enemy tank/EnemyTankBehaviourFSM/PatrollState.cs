using UnityEngine;

public class PatrolState : IState<EnemyTankBehaviourFSM>
{
    private int currentWaypointIndex = 0;
    private float distanceToWaypoint;
    private bool isPatrolInterupted = false;
    private bool isPatrolFinished = false;

    public IState<EnemyTankBehaviourFSM> DoState(EnemyTankBehaviourFSM machine)
    {
        if (machine.state != "patrol state") machine.state = "patrol state";

        DoPatrollBehaviour(machine);

        // Same function as attack state, just with the smaller patrol detection radius
        bool enemySpotted = machine.turretInputs.FindNearestTarget(
            machine.targetLayerMask,
            machine.obstacleMask,
            machine.detectionRadius
        );

        if (enemySpotted)
        {
            isPatrolInterupted = true;
            Reset(machine);
            return machine.attackState;
        }
        else if (isPatrolFinished)
        {
            return new ReturnHomeState();
        }
        else
        {
            return this;
        }
    }

    void DoPatrollBehaviour(EnemyTankBehaviourFSM machine)
    {
        distanceToWaypoint = Vector3.Magnitude(
            machine.waypoints[currentWaypointIndex].position - machine.tankBody.transform.position
        );

        if (distanceToWaypoint < machine.arivalDistance)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= machine.waypoints.Length)
            {
                if (!isPatrolInterupted) isPatrolFinished = true;
                isPatrolInterupted = false;
                currentWaypointIndex = 0;
            }
        }

        if (machine.waypoints.Length > 0)
            machine.navController.navTarget = machine.waypoints[currentWaypointIndex];
    }

    public void Reset(EnemyTankBehaviourFSM machine)
    {
        machine.navController.navTarget = null;
        machine.navController.StopMoving();
    }
}