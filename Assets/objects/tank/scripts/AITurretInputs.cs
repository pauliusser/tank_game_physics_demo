using UnityEngine;

public class AITurretController : MonoBehaviour
{
    [Header("Detection Settings")]
    

    private ITurretControllable turretController;
    private float fireCooldown;
    private Transform currentTarget;
    private Transform gunBarrel;

    private void Awake()
    {
        turretController = GetComponent<ITurretControllable>();
        gunBarrel = GetComponent<TankTurret>().firePoint;
        if (turretController == null)
        {
            turretController = GetComponentInParent<ITurretControllable>();
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

    public bool FindNearestTarget(LayerMask targetLayerMask, LayerMask obstacleMask, float detectionRadius = 20f)
    {
        if (turretController == null) return false;
        Collider[] hits = new Collider[20];
        int count = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, hits, targetLayerMask);

        if (count == 0)
        {
            currentTarget = null;
            return false;
        }

        Transform nearest = null;
        float nearestDistSqr = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            Transform candidate = hits[i].transform;
            Vector3 direction = candidate.position - gunBarrel.transform.position;
            float dist = direction.magnitude;
            if (Physics.Raycast(gunBarrel.transform.position, direction.normalized, out RaycastHit hit, dist, obstacleMask))
                continue;

            float distSqr = direction.sqrMagnitude;
            if (distSqr < nearestDistSqr)
            {
                nearestDistSqr = distSqr;
                nearest = candidate;
            }
        }

        currentTarget = nearest;
        return currentTarget != null;
    }
}