using UnityEngine;

public class TankDeathHandler : MonoBehaviour
{
    [Header("Explosion")]
    public GameObject explosionPrefab;

    [Header("Detached Parts")]
    public GameObject[] parts;
    public GameObject turret;
    public GameObject root;

    [Header("Physics")]
    public PhysicsMaterial wreckageMat;

    [Header("Turret Ejection")]
    public float explosionForce = 15f;
    public bool addRandomRotation = true;
    public float maxRandomTorque = 3f;

    [Header("Wreckage Layer")]
    [Tooltip("Assign a single layer for wreckage. Inspector dropdown via Editor.")]
    public int wreckageLayer = 0;

    private bool hasDied = false;

    /// <summary>
    /// Call this to execute death logic.
    /// </summary>
    public void Die()
    {
        if (hasDied) return;
        hasDied = true;
        GameEvents.OnPlayerDied.Invoke();

        DisableAI();
        Explode();
        SetLayerAndPhysics();
        DisableScriptsRecursively(root);
        DetachParts();
        EjectTurret();

        // Optionally disable this GameObject scripts
    }

    private void DisableAI()
    {
        var ai = GetComponent<MonoBehaviour>(); // replace with actual AI script if needed
        if (ai != null) ai.enabled = false;
    }

    public void Explode()
    {
        if (explosionPrefab == null) return;

        Vector3 pos = turret != null ? turret.transform.position : transform.position;
        Instantiate(explosionPrefab, pos, Quaternion.identity);
    }

    private void SetLayerAndPhysics()
    {
        SetLayerRecursively(root, wreckageLayer);

        // Apply physics materials recursively
        if (wreckageMat != null)
        {
            ApplyPhysicsMaterialRecursively(root, wreckageMat);
        }
    }

    private void DetachParts()
    {
        foreach (var part in parts)
        {
            if (part == null) continue;

            part.transform.parent = null;
            SetupRigidbody(part);
        }
    }

    private void EjectTurret()
    {
        if (turret == null) return;

        turret.transform.parent = null;
        Rigidbody rb = SetupRigidbody(turret);

        // Add ejection force
        rb.AddForce(turret.transform.up * explosionForce, ForceMode.Impulse);

        // Add random torque
        if (addRandomRotation)
        {
            rb.AddTorque(new Vector3(
                Random.Range(-maxRandomTorque, maxRandomTorque),
                Random.Range(-maxRandomTorque, maxRandomTorque),
                Random.Range(-maxRandomTorque, maxRandomTorque)
            ), ForceMode.Impulse);
        }
    }

    /// <summary>
    /// Add Rigidbody if missing and return it
    /// </summary>
    private Rigidbody SetupRigidbody(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = obj.AddComponent<Rigidbody>();
        }
        return rb;
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        if (obj == null) return;
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private void ApplyPhysicsMaterialRecursively(GameObject obj, PhysicsMaterial mat)
    {
        if (obj == null || mat == null) return;

        foreach (var col in obj.GetComponents<Collider>())
        {
            col.material = mat;
        }

        foreach (Transform child in obj.transform)
        {
            ApplyPhysicsMaterialRecursively(child.gameObject, mat);
        }
    }

    private void DisableScriptsRecursively(GameObject obj)
    {
        if (obj == null) return;

        foreach (var mb in obj.GetComponents<MonoBehaviour>())
        {
            if (mb != this && mb.enabled)
                mb.enabled = false;
        }

        foreach (Transform child in obj.transform)
        {
            DisableScriptsRecursively(child.gameObject);
        }
    }
}