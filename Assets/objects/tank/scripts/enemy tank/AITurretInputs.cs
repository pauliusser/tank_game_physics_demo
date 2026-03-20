using UnityEngine;

public class AITurretController : MonoBehaviour
{
    public GameObject tankInstance;
    private GameObject turret;
    private ITurretControllable turretController;
    private float fireCooldown;
    private Transform currentTarget;
    private Transform gunBarrel;

    private void Awake()
    {
        turret = tankInstance.GetComponent<TankRefs>().turret;
        turretController = turret.GetComponent<ITurretControllable>();
        gunBarrel = turret.GetComponent<TankTurret>().firePoint;
        if (turretController == null)
        {
            // turretController = GetComponentInParent<ITurretControllable>();
            Debug.LogError("AITurretInputs: No ITurretControllable found.");
        }    
    }
    public void SetProjectileIndex(int index)
    {
        if (turretController != null)
        {
            int count = turretController.ProjectileCount;
            if (index >= count || index < 0) return;
            turretController.SetProjectileIndex(index);
        }
    }
    public void AimTurretToCurrentTarget()
    {
        if (turretController == null) return;
        turretController.Target = currentTarget;
    }

    public void FireWhenAligned(float aimTolerance = 5f, float fireRate = 2f)
    {
        if (turretController == null) return;
        if (currentTarget != null)
        {
            fireCooldown -= Time.deltaTime;
            // Only fire if aligned and cooldown ready
            if (fireCooldown <= 0f && turretController.AimError <= aimTolerance)
            {
                turretController.Fire();
                fireCooldown = 1f / fireRate;
            }
        }
        else
        {
            fireCooldown = 0f;
        }
    }

    // public bool FindNearestTarget(LayerMask targetLayerMask, LayerMask obstacleMask, float detectionRadius = 20f)
    // {
    //     if (turretController == null) return false;
    //     Collider[] hits = new Collider[20];
    //     int count = Physics.OverlapSphereNonAlloc(turret.transform.position, detectionRadius, hits, targetLayerMask);

    //     if (count == 0)
    //     {
    //         currentTarget = null;
    //         return false;
    //     }

    //     Transform nearest = null;
    //     float nearestDistSqr = float.MaxValue;

    //     for (int i = 0; i < count; i++)
    //     {
    //         Transform candidate = hits[i].transform;
    //         Vector3 direction = candidate.position - gunBarrel.transform.position;
    //         float dist = direction.magnitude;
    //         if (Physics.Raycast(gunBarrel.transform.position, direction.normalized, out RaycastHit hit, dist, obstacleMask))
    //             continue;

    //         float distSqr = direction.sqrMagnitude;
    //         if (distSqr < nearestDistSqr)
    //         {
    //             nearestDistSqr = distSqr;
    //             nearest = candidate;
    //         }
    //     }

    //     currentTarget = nearest;
    //     return currentTarget != null;
    // }

    public bool FindNearestTarget(LayerMask targetLayerMask, LayerMask obstacleMask, float detectionRadius = 20f)
    {
        if (turretController == null) return false;
        
        Collider[] hits = new Collider[20];
        int count = Physics.OverlapSphereNonAlloc(turret.transform.position, detectionRadius, hits, targetLayerMask);

        if (count == 0)
        {
            currentTarget = null;
            return false;
        }

        Transform nearest = null;
        float nearestDistSqr = float.MaxValue;
        
        // Get the radius of the projectile (you might want to add this as a serialized field)
        float projectileRadius = 0.1f; // Adjust this based on your projectile size

        for (int i = 0; i < count; i++)
        {
            Transform candidate = hits[i].transform;
            Vector3 direction = candidate.position - gunBarrel.transform.position;
            float distance = direction.magnitude;
            
            // SphereCast from gun barrel to target
            RaycastHit hitInfo;
            if (Physics.SphereCast(turret.transform.position, projectileRadius, direction.normalized, out hitInfo, distance, obstacleMask))
            {
                // Hit something - check if it's the target or an obstacle
                if (hitInfo.transform == candidate)
                {
                    // Direct hit on target - consider it valid
                    float distSqr = direction.sqrMagnitude;
                    if (distSqr < nearestDistSqr)
                    {
                        nearestDistSqr = distSqr;
                        nearest = candidate;
                    }
                }
                // If it hit something else, skip this target
                continue;
            }
            else
            {
                // Clear path to target
                float distSqr = direction.sqrMagnitude;
                if (distSqr < nearestDistSqr)
                {
                    nearestDistSqr = distSqr;
                    nearest = candidate;
                }
            }
        }

        currentTarget = nearest;
        return currentTarget != null;
    }
}