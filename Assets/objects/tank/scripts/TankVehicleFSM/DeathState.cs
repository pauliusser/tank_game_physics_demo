public class DeathState : IState<TankVehicleFSM>
{
    private bool isDead = false;

    public IState<TankVehicleFSM> DoState(TankVehicleFSM machine)
    {
        if (!isDead)
        {
            machine.deathHandler?.Die(); // call the handler
            isDead = true;
        }

        return this; // remain in death state
    }
}



// using UnityEngine;
// public class DeathState : IState<TankVehicleFSM>
// {
//     private bool isDead = false;
//     public IState<TankVehicleFSM> DoState(TankVehicleFSM machine)
//     {
//         if (!isDead)
//         {
//             Death(machine);
//             isDead = true;
//         }
//         return this;
//     }

//     private void Death(TankVehicleFSM machine)
//     {
//         if (machine.AIBehaviour != null) machine.AIBehaviour.enabled = false;
//         if (machine.deathHandler.explosionPrefab != null) machine.deathHandler.Explode();
//         int wreckageLayer = GetWreckageLayer(machine);
//         machine.gameObject.layer = wreckageLayer;
//         if (machine.deathHandler.root != null) DisableScriptsAndSetLayer(machine.deathHandler.root, wreckageLayer, machine);
        
//         // Handle regular parts
//         foreach (GameObject part in machine.deathHandler.parts)
//         {
//             if (part != null)
//             {
//                 part.transform.parent = null;
//                 part.layer = wreckageLayer;
                
//                 // Add Rigidbody if needed and assign physics material
//                 Rigidbody partRb = part.GetComponent<Rigidbody>();
//                 if (partRb == null) 
//                 {
//                     partRb = part.AddComponent<Rigidbody>();
//                 }
                
//                 // Assign physics material to colliders
//                 if (machine.deathHandler.wreckageMat != null)
//                 {
//                     Collider[] colliders = part.GetComponents<Collider>();
//                     foreach (Collider col in colliders)
//                     {
//                         col.material = machine.deathHandler.wreckageMat;
//                     }
//                 }
//             }
//         }
        
//         // Handle turret
//         if (machine.deathHandler.turret != null)
//         {
//             machine.deathHandler.turret.transform.parent = null;
//             machine.deathHandler.turret.layer = wreckageLayer;
            
//             // Add Rigidbody if needed and assign physics material
//             Rigidbody turretRb = machine.deathHandler.turret.GetComponent<Rigidbody>();
//             if (turretRb == null) 
//             {
//                 turretRb = machine.deathHandler.turret.AddComponent<Rigidbody>();
//             }
            
//             // Assign physics material to turret colliders
//             if (machine.deathHandler.wreckageMat != null)
//             {
//                 Collider[] turretColliders = machine.deathHandler.turret.GetComponents<Collider>();
//                 foreach (Collider col in turretColliders)
//                 {
//                     col.material = machine.deathHandler.wreckageMat;
//                 }
//             }
            
//             // Apply ejection force
//             Vector3 localUpForce = machine.deathHandler.turret.transform.up * machine.deathHandler.explosionForce;
//             turretRb.AddForce(localUpForce, ForceMode.Impulse);
            
//             // Add random rotation if enabled
//             if (machine.deathHandler.addRandomRotation)
//             {
//                 float min = -machine.deathHandler.maxRandomTorque;
//                 float max = machine.deathHandler.maxRandomTorque;
//                 Vector3 randomTorque = new Vector3(
//                     Random.Range(min, max),
//                     Random.Range(min, max),
//                     Random.Range(min, max)
//                 );
//                 turretRb.AddTorque(randomTorque, ForceMode.Impulse);
//             }
//         }
        
//         // Disable this script
//         machine.enabled = false;
//     }

//     private void DisableScriptsAndSetLayer(GameObject obj, int layer, TankVehicleFSM machine)
//     {
//         if (obj == null) return;
//         obj.layer = layer;
        
//         // Disable scripts
//         MonoBehaviour[] scripts = obj.GetComponents<MonoBehaviour>();
//         foreach (MonoBehaviour script in scripts)
//         {
//             if (script != null && script != machine && script.enabled) 
//                 script.enabled = false;
//         }
        
//         // Assign physics material to all colliders in hierarchy
//         if (machine.deathHandler.wreckageMat != null)
//         {
//             Collider[] colliders = obj.GetComponents<Collider>();
//             foreach (Collider col in colliders)
//             {
//                 col.material = machine.deathHandler.wreckageMat;
//             }
//         }
        
//         // Recursively process children
//         foreach (Transform child in obj.transform)
//         {
//             if (child != null) 
//                 DisableScriptsAndSetLayer(child.gameObject, layer, machine);
//         }
//     }

//         private int GetWreckageLayer(TankVehicleFSM machine)
//     {
//         int layer = LayerMask.NameToLayer(machine.deathHandler.wreckageLayerName);
//         if (layer == -1)
//         {
//             Debug.LogWarning($"Layer '{machine.deathHandler.wreckageLayerName}' not found! Using Default layer (0)");
//             layer = 0; 
//         }
//         return layer;
//     }
// }
