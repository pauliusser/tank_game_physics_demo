using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AINavigationController : MonoBehaviour
{
    [Header("Target")]
    public Transform navTarget;

    [Header("Pathfinding")]
    public float updateSpeed = 0.1f;
    public float stoppingDistance = 2f;

    [Header("Steering")]
    public float facingThreshold = 5f;
    public float turnInPlaceAngle = 20f;

    [Header("Stuck Recovery")]
    public BumperTrigger frontBumper;
    public float backupDuration = 0.2f;

    private IDrivable drivableVehicle;
    private Vector2 aiInput;          // calculated (x = turn, y = throttle)
    private bool isBackingUp;
    private float backupTimer;
    private NavMeshPath aiPath;       // for debug

    private void Awake()
    {
        drivableVehicle = GetComponent<IDrivable>();
        if (drivableVehicle == null)
        {
            Debug.LogError("AIController: No component implementing IDrivable found on " + gameObject.name);
        }
    }

    private void Start()
    {
        StartCoroutine(FollowTarget());
    }
    public void StopMoving()
    {
        drivableVehicle.DriveX = 0;
        drivableVehicle.DriveY = 0;
    }

    private void Update()
    {
        if (drivableVehicle == null || navTarget == null) return;

        // Obstacle avoidance / backing up
        if (isBackingUp)
        {
            backupTimer -= Time.deltaTime;
            if (backupTimer <= 0f)
                isBackingUp = false;

            // Keep steering, force reverse
            drivableVehicle.DriveX = aiInput.x;
            drivableVehicle.DriveY = -1f;
        }
        else
        {
            if (frontBumper != null && frontBumper.isTouchingWall)
            {
                isBackingUp = true;
                backupTimer = backupDuration;
                drivableVehicle.DriveX = aiInput.x;
                drivableVehicle.DriveY = -1f;
            }
            else
            {
                drivableVehicle.DriveX = aiInput.x;
                drivableVehicle.DriveY = aiInput.y;
            }
        }
    }

    private IEnumerator FollowTarget()
    {
        WaitForSeconds wait = new WaitForSeconds(updateSpeed);
        NavMeshPath path = new NavMeshPath();
        NavMeshHit startHit, targetHit;

        while (enabled)
        {
            if (navTarget != null)
            {
                // Sample positions on NavMesh
                Vector3 startPos = transform.position;
                if (NavMesh.SamplePosition(startPos, out startHit, 2f, NavMesh.AllAreas))
                    startPos = startHit.position;

                Vector3 targetPos = navTarget.position;
                if (NavMesh.SamplePosition(targetPos, out targetHit, 2f, NavMesh.AllAreas))
                    targetPos = targetHit.position;

                if (NavMesh.CalculatePath(startPos, targetPos, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    aiPath = path;

                    Vector3 nextTarget = transform.position;
                    if (path.corners.Length > 1)
                        nextTarget = path.corners[1];
                    else if (path.corners.Length == 1)
                        nextTarget = path.corners[0];

                    aiInput = CalculateAIMovement(nextTarget);
                }
                else
                {
                    aiPath = null;
                    aiInput = CalculateAIMovement(navTarget.position);
                }
            }
            yield return wait;
        }
    }

    private Vector2 CalculateAIMovement(Vector3 nextCorner)
    {
        float distanceToFinal = navTarget != null ? Vector3.Distance(transform.position, navTarget.position) : float.MaxValue;
        float distanceToNext = Vector3.Distance(transform.position, nextCorner);

        Vector3 tankForwardFlat = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 directionToNextFlat = Vector3.ProjectOnPlane((nextCorner - transform.position), Vector3.up).normalized;

        float angle = Vector3.SignedAngle(tankForwardFlat, directionToNextFlat, Vector3.up);
        float absAngle = Mathf.Abs(angle);

        float move = 0f;
        float turn = 0f;

        if (distanceToNext < 1f)
            turn = Mathf.Clamp(angle / 90f, -1f, 1f);
        else
            turn = Mathf.Lerp(0f, Mathf.Clamp(angle / 90f, -1f, 1f), distanceToNext);

        if (distanceToFinal > stoppingDistance)
        {
            float alignment = Mathf.Clamp01(1f - (absAngle / 90f));
            if (absAngle > turnInPlaceAngle && distanceToNext > 2f)
                move = 0.01f;
            else
                move = Mathf.Lerp(0.05f, 1f, alignment);
        }
        else
        {
            turn = 0f;
            move = 0f;
        }

        return new Vector2(turn, move);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        if (aiPath != null && aiPath.corners.Length > 0)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < aiPath.corners.Length; i++)
            {
                Gizmos.DrawSphere(aiPath.corners[i], 0.2f);
                if (i < aiPath.corners.Length - 1)
                    Gizmos.DrawLine(aiPath.corners[i], aiPath.corners[i + 1]);
            }
        }

        if (aiPath != null && aiPath.corners.Length > 0)
        {
            Vector3 nextTarget = aiPath.corners.Length > 1 ? aiPath.corners[1] : aiPath.corners[0];
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, nextTarget);

            Gizmos.color = Color.yellow;
            Vector3 right = Quaternion.Euler(0, facingThreshold, 0) * transform.forward;
            Vector3 left = Quaternion.Euler(0, -facingThreshold, 0) * transform.forward;
            Gizmos.DrawRay(transform.position, right * 2f);
            Gizmos.DrawRay(transform.position, left * 2f);
        }
    }
    public void NavigateTo(Transform goal)
    {
        if (goal == null)
        {
            Debug.LogWarning("NavigateTo: goal is null!");
            return;
        }
        StopMoving();
        navTarget = goal;
        Debug.Log($"Navigating to {goal.name}");
    }
}