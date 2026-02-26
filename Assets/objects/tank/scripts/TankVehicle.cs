using UnityEngine;

public class TankVehicle : MonoBehaviour, IDamagable
{
    public int maxHealth = 1000;
    public int Health { get; set; }
    
    [Header("After death explosion detached parts")]
    public GameObject[] parts;
    public GameObject turret;
    public GameObject root;
    public GameObject explosionPrefab;
    public EnemyTankStateMachine stateMachine;
    public PhysicsMaterial wreckageMat;
    
    [Header("Turret Ejection")]
    public float explosionForce = 15f;
    public bool addRandomRotation = true;
    public float maxRandomTorque = 3f;
    
    [Header("Wreckage Layer")]
    public string wreckageLayerName = "Wreckage";

    void Start()
    {
        Health = maxHealth;
        if (root == null) root = gameObject;
    }
    
    public void Damage(Damage.Request d)
    {
        Debug.Log($"Damage method called! Type: {d.type}, Damage: {d.damage}");
        Debug.Log($"current health {Health}");
        
        if (d.type == "kinetic") Health -= d.damage;
        if (d.type == "explosive") Health -= d.damage;
        if (Health <= 0) Death();
    }
    
    private void Death()
    {
        if (stateMachine != null) stateMachine.enabled = false;
        Vector3 explosionPos = turret != null ? turret.transform.position : transform.position;
        if (explosionPrefab != null) Instantiate(explosionPrefab, explosionPos, Quaternion.identity);
        int wreckageLayer = GetWreckageLayer();
        gameObject.layer = wreckageLayer;
        if (root != null) DisableScriptsAndSetLayer(root, wreckageLayer);
        
        // Handle regular parts
        foreach (GameObject part in parts)
        {
            if (part != null)
            {
                part.transform.parent = null;
                part.layer = wreckageLayer;
                
                // Add Rigidbody if needed and assign physics material
                Rigidbody partRb = part.GetComponent<Rigidbody>();
                if (partRb == null) 
                {
                    partRb = part.AddComponent<Rigidbody>();
                }
                
                // Assign physics material to colliders
                if (wreckageMat != null)
                {
                    Collider[] colliders = part.GetComponents<Collider>();
                    foreach (Collider col in colliders)
                    {
                        col.material = wreckageMat;
                    }
                }
            }
        }
        
        // Handle turret
        if (turret != null)
        {
            turret.transform.parent = null;
            turret.layer = wreckageLayer;
            
            // Add Rigidbody if needed and assign physics material
            Rigidbody turretRb = turret.GetComponent<Rigidbody>();
            if (turretRb == null) 
            {
                turretRb = turret.AddComponent<Rigidbody>();
            }
            
            // Assign physics material to turret colliders
            if (wreckageMat != null)
            {
                Collider[] turretColliders = turret.GetComponents<Collider>();
                foreach (Collider col in turretColliders)
                {
                    col.material = wreckageMat;
                }
            }
            
            // Apply ejection force
            Vector3 localUpForce = turret.transform.up * explosionForce;
            turretRb.AddForce(localUpForce, ForceMode.Impulse);
            
            // Add random rotation if enabled
            if (addRandomRotation)
            {
                Vector3 randomTorque = new Vector3(
                    Random.Range(-maxRandomTorque, maxRandomTorque),
                    Random.Range(-maxRandomTorque, maxRandomTorque),
                    Random.Range(-maxRandomTorque, maxRandomTorque)
                );
                turretRb.AddTorque(randomTorque, ForceMode.Impulse);
            }
        }
        
        // Disable this script
        this.enabled = false;
    }
    
    private void DisableScriptsAndSetLayer(GameObject obj, int layer)
    {
        if (obj == null) return;
        obj.layer = layer;
        
        // Disable scripts
        MonoBehaviour[] scripts = obj.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != null && script != this && script.enabled) 
                script.enabled = false;
        }
        
        // Assign physics material to all colliders in hierarchy
        if (wreckageMat != null)
        {
            Collider[] colliders = obj.GetComponents<Collider>();
            foreach (Collider col in colliders)
            {
                col.material = wreckageMat;
            }
        }
        
        // Recursively process children
        foreach (Transform child in obj.transform)
        {
            if (child != null) 
                DisableScriptsAndSetLayer(child.gameObject, layer);
        }
    }
    
    private int GetWreckageLayer()
    {
        int layer = LayerMask.NameToLayer(wreckageLayerName);
        if (layer == -1)
        {
            Debug.LogWarning($"Layer '{wreckageLayerName}' not found! Using Default layer (0)");
            layer = 0; 
        }
        return layer;
    }
    
    public void TriggerDeath()
    {
        if (Health > 0)
        {
            Health = 0;
            Death();
        }
    }
}