using System.Collections;
using UnityEngine;

public class TankVehicleFSM : MonoBehaviour, IDamagable
{
    private IState<TankVehicleFSM> currentState; 
    public string CurrentStateName; 
    public int maxHealth = 1000;
    public int Health { get; set; }
    
    [Header("After death explosion detached parts")]
    public GameObject[] parts;
    public GameObject turret;
    public GameObject root;
    public GameObject explosionPrefab;
    public EnemyTankBehaviourFSM AIBehaviour;
    public PhysicsMaterial wreckageMat;
    
    [Header("Turret Ejection")]
    public float explosionForce = 15f;
    public bool addRandomRotation = true;
    public float maxRandomTorque = 3f;
    
    [Header("Wreckage Layer")]
    public string wreckageLayerName = "Wreckage";

    [Header("Stuck Settings")]
    public float capsizethreshold = 0.1f;
    public float stuckTimeout = 3f;
    public float recoverCooldown = 10f;
    private bool isRecoveryOnCooldown = false;

    void Start()
    {
        Health = maxHealth;
        if (root == null) root = gameObject;
        currentState = new OperationalState();
    }
    void Update()
    {
        currentState = currentState.DoState(this);
        CurrentStateName = currentState?.GetType().Name;
    }
     public void Recover()
    {
        if (isRecoveryOnCooldown)
        {
            Debug.Log($"Recovery on cooldown!");
            return;
        }
        currentState = new StuckState();
        StartCoroutine(RecoveryCooldown());
    }

    private IEnumerator RecoveryCooldown()
    {
        isRecoveryOnCooldown = true;
        
        // Wait for cooldown duration
        yield return new WaitForSeconds(recoverCooldown);
        
        isRecoveryOnCooldown = false;
        Debug.Log("Recovery ready!");
    }
    
    public void Damage(Damage.Request d)
    {
        if (Health <= 0) return;
        Debug.Log($"Damage method called! Type: {d.type}, Damage: {d.damage}");
        Debug.Log($"current health {Health}");
        
        if (d.type == "kinetic") Health -= d.damage;
        if (d.type == "explosive") Health -= d.damage;
        if (Health <= 0) currentState = new DeathState();
    }
    
    
    public void Explosion()
    {
        Vector3 explosionPos = turret != null ? turret.transform.position : transform.position;
        Instantiate(explosionPrefab, explosionPos, Quaternion.identity);
    }
    

    
    // public void TriggerDeath()
    // {
    //     if (Health > 0)
    //     {
    //         Health = 0;
    //         currentState = new DeathState();
    //     }
    // }
}