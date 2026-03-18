using UnityEngine;

public class EnemyTankBehaviourFSM : MonoBehaviour
{
    [Header("Initialisation")]
    private IState<EnemyTankBehaviourFSM> currentState; 
    public GameObject tankBody;
    public GameObject tankTurret;
    public AINavigationController navController;
    public AITurretController turretInputs;
    private TankTurret turretSettings;
    
    [Header("homepoint settings")]
    public Transform homepoint;
    public LayerMask groundLayerMask;

    [Header("current state")]
    public string state = "";

    [Header("Patrol Settings")]
    public Transform[] waypoints;
    public float arivalDistance = 0.5f;

    [Header("Enemy detection Settings")]
    public float detectionRadius = 8f;
    public LayerMask targetLayerMask;
    public LayerMask obstacleMask;
    
    [Header("Firing Settings")]
    public float fireRate = 2f;
    public int defaultProjectileIndex = 0;
    public float aimTolerance = 5f; 
    public float fireRange = 10f;

    // cached states
    public PatrolState patrolState;
    public AttackState attackState;

    void Start()
    {
        if (navController == null) navController = tankBody.GetComponent<AINavigationController>();
        if (turretInputs == null) turretInputs = tankTurret.GetComponent<AITurretController>();
        navController.stoppingDistance = arivalDistance;
        turretSettings = tankTurret.GetComponent<TankTurret>();
        homepoint = CreateHomepoint(0.2f);
        
        patrolState = new PatrolState();
        attackState = new AttackState();
        currentState = patrolState;

    }
    void Update()
    {
        currentState = currentState.DoState(this);
    }
    Transform CreateHomepoint(float height)
    {
        RaycastHit hit;
        Vector3 rayStart = tankBody.transform.position;
        if (Physics.Raycast(rayStart, Vector3.down, out hit, Mathf.Infinity, groundLayerMask))
        {
            Vector3 homepointPosition = hit.point + new Vector3(0f, height, 0f);
            GameObject homepoint = new GameObject("homepoint_of_" + gameObject.name);
            homepoint.transform.position = homepointPosition;
            EmptyGizmoCube gizmo = homepoint.AddComponent<EmptyGizmoCube>();
            gizmo.gizmoColor = Color.green; 
            gizmo.size = 0.1f; 
            Debug.Log($"Homepoint created at {homepointPosition} for {gameObject.name}");
            return homepoint.transform;
        }
        else
        {
            Debug.LogWarning($"CreateHomepoint: No ground found beneath {gameObject.name}");
            GameObject homepoint = new GameObject("homepoint_of_" + gameObject.name);
            return homepoint.transform;
        }
    }
}