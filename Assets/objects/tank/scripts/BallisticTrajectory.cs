using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(TankTurret))]
public class BallisticTrajectory : MonoBehaviour
{
    public int maxSegments = 50;
    public float lineWidth = 0.05f;
    public LayerMask collisionMask;
    public Vector3 crossPoint;
    public bool isColidingEnemy;
    public LayerMask enemyLayerMask;

    [Header("Trajectory Trimming (world units)")]
    public float startOffset = 0.2f;      // distance from fire point to start drawing (avoids barrel)
    public float endOffset = 0f;          // distance from impact point to stop drawing (0 = exactly at target)

    private LineRenderer line;
    private TankTurret turret;
    private List<Vector3> points;         // stores the full trajectory

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        turret = GetComponent<TankTurret>();
        line.useWorldSpace = true;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        
        line.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        line.material.color = Color.white;
        line.material.mainTexture = null;          // if you don't need a texture
        line.startColor = Color.white;
        line.endColor = Color.white;
        line.useWorldSpace = true;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        points = new List<Vector3>();
    }
    void OnEnable()
    {
        PlayerEvents.OnPlayerDied.Subscribe(OnPlayerDied);
    }

    void OnDisable()
    {
        PlayerEvents.OnPlayerDied.Unsubscribe(OnPlayerDied);
    }

    void OnPlayerDied()
    {
        line.positionCount = 0;
    }

    void Update()
    {
        if (turret.isBalisticLine && turret.shotTarget != null)
        {
            float segmentLength = (turret.shotTarget.position - transform.position).magnitude / 50;
            DrawTrajectory(turret.forceVector, segmentLength);
        }
        else
        {
            line.positionCount = 0;   // hide when not aiming
        }
    }

    void DrawTrajectory(Vector3 velocity, float segHorLen)
    {
        Vector3 startPos = turret.firePoint.position;
        Vector3 gravity = Physics.gravity;

        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
        float horizontalSpeed = horizontalVelocity.magnitude;

        if (horizontalSpeed < 0.001f)
        {
            line.positionCount = 0;
            return;
        }

        float timeStep = segHorLen / horizontalSpeed;

        // 1. Build the full list of trajectory points (from fire point to hit point)
        points.Clear();
        points.Add(startPos);

        Vector3 previousPos = startPos;

        for (int i = 1; i <= maxSegments; i++)
        {
            float t = i * timeStep;
            Vector3 currentPos = startPos + velocity * t + 0.5f * gravity * t * t;

            Vector3 direction = currentPos - previousPos;
            float distance = direction.magnitude;

            if (Physics.Raycast(previousPos, direction.normalized, out RaycastHit hit, distance, collisionMask))
            {
                points.Add(hit.point);
                crossPoint = hit.point;
                isColidingEnemy = ((1 << hit.transform.gameObject.layer) & enemyLayerMask) != 0;
                break;   // stop at impact
            }
            else
            {
                points.Add(currentPos);
                crossPoint = currentPos;
                isColidingEnemy = false;
            }

            previousPos = currentPos;
        }

        // 2. If we have fewer than 2 points, nothing to draw
        if (points.Count < 2)
        {
            line.positionCount = 0;
            return;
        }

        // 3. Compute cumulative distances along the trajectory
        float[] cumulativeDist = new float[points.Count];
        cumulativeDist[0] = 0f;
        for (int i = 1; i < points.Count; i++)
        {
            cumulativeDist[i] = cumulativeDist[i - 1] + Vector3.Distance(points[i - 1], points[i]);
        }
        float totalLength = cumulativeDist[points.Count - 1];

        // 4. Apply offsets and clamp them to the valid range
        float startDist = Mathf.Clamp(startOffset, 0f, totalLength);
        float endDist   = Mathf.Clamp(totalLength - endOffset, startDist, totalLength);

        // 5. Find the indices that contain these distances
        int startIdx = 0, endIdx = points.Count - 1;
        for (int i = 0; i < points.Count - 1; i++)
        {
            if (cumulativeDist[i] <= startDist && startDist <= cumulativeDist[i + 1])
                startIdx = i;
            if (cumulativeDist[i] <= endDist && endDist <= cumulativeDist[i + 1])
                endIdx = i;
        }

        // 6. Build the final list of points for the trimmed line
        List<Vector3> finalPoints = new List<Vector3>();

        // Start point (possibly interpolated)
        if (Mathf.Approximately(startDist, cumulativeDist[startIdx]))
        {
            finalPoints.Add(points[startIdx]);
        }
        else
        {
            float t = (startDist - cumulativeDist[startIdx]) / (cumulativeDist[startIdx + 1] - cumulativeDist[startIdx]);
            Vector3 interpolated = Vector3.Lerp(points[startIdx], points[startIdx + 1], t);
            finalPoints.Add(interpolated);
        }

        // Add all full points between startIdx+1 and endIdx
        for (int i = startIdx + 1; i <= endIdx; i++)
        {
            finalPoints.Add(points[i]);
        }

        // End point (possibly interpolated)
        if (!Mathf.Approximately(endDist, cumulativeDist[endIdx]) && endIdx < points.Count - 1)
        {
            // If we added points[endIdx] in the loop, remove it and replace with interpolated end
            if (finalPoints.Count > 0 && finalPoints[finalPoints.Count - 1] == points[endIdx])
                finalPoints.RemoveAt(finalPoints.Count - 1);

            float t = (endDist - cumulativeDist[endIdx]) / (cumulativeDist[endIdx + 1] - cumulativeDist[endIdx]);
            Vector3 interpolated = Vector3.Lerp(points[endIdx], points[endIdx + 1], t);
            finalPoints.Add(interpolated);
        }

        // 7. Assign the final points to the LineRenderer
        line.positionCount = finalPoints.Count;
        line.SetPositions(finalPoints.ToArray());
    }
}