using UnityEngine;

public class PatrolState : IState
{
    private int currentWaypointIndex = 0;
    private float distanceToWaypoint;
    private bool isPatrolInterupted = false;
    private bool isPatrolFinished = false;
    public IState DoState(EnemyTankStateMachine machine)
    {
        if (machine.state != "patrol state") machine.state = "patrol state";

        DoPatrollBehaviour(machine);

        if (EncounteredEnemy(machine))
        {
            isPatrolInterupted = true;
            Reset(machine);  
            return machine.attackState;  // new state
        }
        else if (isPatrolFinished) {
            return new ReturnHomeState();
        }
        else
        {
            return this; // stay in FooState
        }
    }
    void DoPatrollBehaviour(EnemyTankStateMachine machine)
    {
        distanceToWaypoint = Vector3.Magnitude(machine.waypoints[currentWaypointIndex].position - machine.tankBody.transform.position);

        if (distanceToWaypoint < machine.arivalDistance)
        {
            currentWaypointIndex ++;
            if (currentWaypointIndex >= machine.waypoints.Length)
            {
                if (!isPatrolInterupted) isPatrolFinished = true;
                isPatrolInterupted = false;
                currentWaypointIndex = 0;
            }
        }
        if (machine.waypoints.Length > 0)
        {
            machine.navController.navTarget = machine.waypoints[currentWaypointIndex];
        }
    }
    public bool EncounteredEnemy(EnemyTankStateMachine machine)
    {
        Collider[] hits = new Collider[20];
        int count = Physics.OverlapSphereNonAlloc
        (
            machine.tankBody.transform.position,
            machine.detectionRadius,
            hits,
            machine.targetLayerMask
        );

        if (count == 0)
        {
            return false;
        }

        Transform nearest = null;
        float nearestDistSqr = float.MaxValue;
        Vector3 turretPos = machine.tankTurret.transform.position;

        for (int i = 0; i < count; i++)
        {
            Transform candidate = hits[i].transform;
            Vector3 direction = candidate.position - turretPos;
            float dist = direction.magnitude;
            if (Physics.Raycast(turretPos, direction.normalized, out RaycastHit hit, dist, machine.obstacleMask))
                continue;

            float distSqr = direction.sqrMagnitude;
            if (distSqr < nearestDistSqr)
            {
                nearestDistSqr = distSqr;
                nearest = candidate;
            }
        }

        return nearest != null;
    }
    public void Reset(EnemyTankStateMachine machine)
    {
        // stop patrolling
        machine.navController.navTarget = null;
        machine.navController.StopMoving();
    }
}