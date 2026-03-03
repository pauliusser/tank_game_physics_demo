using UnityEngine;
public class DeathState : IState<TankVehicleFSM>
{
    private bool isDead = false;
    public IState<TankVehicleFSM> DoState(TankVehicleFSM machine)
    {
        if (!isDead)
        {
            Death(machine);
            isDead = true;
        }
        return this;
    }

    private void Death(TankVehicleFSM machine)
    {
        if (machine.AIBehaviour != null) machine.AIBehaviour.enabled = false;
        if (machine.explosionPrefab != null) machine.Explosion();
        int wreckageLayer = GetWreckageLayer(machine);
        machine.gameObject.layer = wreckageLayer;
        if (machine.root != null) DisableScriptsAndSetLayer(machine.root, wreckageLayer, machine);
        
        // Handle regular parts
        foreach (GameObject part in machine.parts)
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
                if (machine.wreckageMat != null)
                {
                    Collider[] colliders = part.GetComponents<Collider>();
                    foreach (Collider col in colliders)
                    {
                        col.material = machine.wreckageMat;
                    }
                }
            }
        }
        
        // Handle turret
        if (machine.turret != null)
        {
            machine.turret.transform.parent = null;
            machine.turret.layer = wreckageLayer;
            
            // Add Rigidbody if needed and assign physics material
            Rigidbody turretRb = machine.turret.GetComponent<Rigidbody>();
            if (turretRb == null) 
            {
                turretRb = machine.turret.AddComponent<Rigidbody>();
            }
            
            // Assign physics material to turret colliders
            if (machine.wreckageMat != null)
            {
                Collider[] turretColliders = machine.turret.GetComponents<Collider>();
                foreach (Collider col in turretColliders)
                {
                    col.material = machine.wreckageMat;
                }
            }
            
            // Apply ejection force
            Vector3 localUpForce = machine.turret.transform.up * machine.explosionForce;
            turretRb.AddForce(localUpForce, ForceMode.Impulse);
            
            // Add random rotation if enabled
            if (machine.addRandomRotation)
            {
                Vector3 randomTorque = new Vector3(
                    Random.Range(-machine.maxRandomTorque, machine.maxRandomTorque),
                    Random.Range(-machine.maxRandomTorque, machine.maxRandomTorque),
                    Random.Range(-machine.maxRandomTorque, machine.maxRandomTorque)
                );
                turretRb.AddTorque(randomTorque, ForceMode.Impulse);
            }
        }
        
        // Disable this script
        machine.enabled = false;
    }

    private void DisableScriptsAndSetLayer(GameObject obj, int layer, TankVehicleFSM machine)
    {
        if (obj == null) return;
        obj.layer = layer;
        
        // Disable scripts
        MonoBehaviour[] scripts = obj.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != null && script != machine && script.enabled) 
                script.enabled = false;
        }
        
        // Assign physics material to all colliders in hierarchy
        if (machine.wreckageMat != null)
        {
            Collider[] colliders = obj.GetComponents<Collider>();
            foreach (Collider col in colliders)
            {
                col.material = machine.wreckageMat;
            }
        }
        
        // Recursively process children
        foreach (Transform child in obj.transform)
        {
            if (child != null) 
                DisableScriptsAndSetLayer(child.gameObject, layer, machine);
        }
    }

        private int GetWreckageLayer(TankVehicleFSM machine)
    {
        int layer = LayerMask.NameToLayer(machine.wreckageLayerName);
        if (layer == -1)
        {
            Debug.LogWarning($"Layer '{machine.wreckageLayerName}' not found! Using Default layer (0)");
            layer = 0; 
        }
        return layer;
    }
}
