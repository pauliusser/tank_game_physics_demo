using System.Collections;
using UnityEngine;

public class TankVehicleFSM : MonoBehaviour
{
    public string CurrentStateName;

    public EnemyTankBehaviourFSM AIBehaviour;
    public TankDamageHandler damageHandler;
    public TankDeathHandler deathHandler;
    public TankStats stats;
    public GameObject body;
    public GameObject turret;

    private IState<TankVehicleFSM> currentState;

    [Header("Recovery")]
    public float capsizethreshold = 0.1f;
    public float stuckTimeout = 3f;
    public float recoverCooldown = 10f;
    private bool isRecoveryOnCooldown = false;

    void Awake()
    {
        damageHandler = GetComponent<TankDamageHandler>();
        deathHandler = GetComponent<TankDeathHandler>();
        stats = GetComponent<TankStats>();
    }

    void Start()
    {
        currentState = new OperationalState();
    }

    void Update()
    {
        currentState = currentState.DoState(this);
        CurrentStateName = currentState?.GetType().Name;
    }

    public void ChangeState(IState<TankVehicleFSM> newState)
    {
        currentState = newState;
    }

    public void Recover()
    {
        if (isRecoveryOnCooldown)
        {
            Debug.Log("Recovery on cooldown!");
            return;
        }

        ChangeState(new StuckState());
        StartCoroutine(RecoveryCooldown());
    }

    private IEnumerator RecoveryCooldown()
    {
        isRecoveryOnCooldown = true;
        yield return new WaitForSeconds(recoverCooldown);
        isRecoveryOnCooldown = false;
        Debug.Log("Recovery ready!");
    }
}