using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(TankTurret))]
public class BallisticTrajectory : MonoBehaviour
{
    public int maxSegments = 50;
    public float lineWidth = 0.05f;
    public LayerMask collisionMask;
    private LineRenderer line;
    private TankTurret turret;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        turret = GetComponent<TankTurret>(); // cached ONCE
        line.useWorldSpace = true;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.material = new Material(Shader.Find("Sprites/Default"));
    }

    void Update()
    {
        if (turret.isBalisticLine && turret.shotTarget != null)
        {
            float segmentLength = (turret.shotTarget.position - transform.position).magnitude / 50;
            drawBalisticTrajectory(turret.forceVector, segmentLength);
        }
    }

    public void drawBalisticTrajectory(Vector3 velocity, float segHorLen)
    {
        Vector3 startPos = turret.firePoint.position;
        Vector3 gravity = Physics.gravity;

        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
        float horizontalSpeed = horizontalVelocity.magnitude;

        if (horizontalSpeed < 0.001f)
            return;

        float timeStep = segHorLen / horizontalSpeed;

        line.positionCount = maxSegments + 1;
        line.SetPosition(0, startPos);

        Vector3 previousPos = startPos;

        for (int i = 1; i <= maxSegments; i++)
        {
            float t = i * timeStep;

            Vector3 currentPos =
                startPos +
                velocity * t +
                0.5f * gravity * t * t;

            // 🔴 Raycast between segments
            Vector3 direction = currentPos - previousPos;
            float distance = direction.magnitude;

            if (Physics.Raycast(previousPos, direction.normalized, out RaycastHit hit, distance, collisionMask))
            {
                line.positionCount = i + 1;
                line.SetPosition(i, hit.point);
                return;
            }

            line.SetPosition(i, currentPos);
            previousPos = currentPos;
        }
    }
}
