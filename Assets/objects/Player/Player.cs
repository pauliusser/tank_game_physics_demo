using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Stats")]
    public int score;
    public int lives = 3;
    
    [Header("Tank Settings")]
    public GameObject tankPrefab;
    private GameObject currentTank;
    
    // Tank interfaces
    private IDrivable currentDrivable;
    private ITurretControllable currentTurret;
    private TankVehicleFSM currentFSM;
    
    // Input handlers
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private PlayerTurretInputs turretInputs;
    
    private void Start()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
        turretInputs = GetComponent<PlayerTurretInputs>();
        if (currentTank == null)
        {
            SpawnNewTank();
        }
    }

    private void SpawnNewTank()
    {
        currentTank = Instantiate(tankPrefab, transform.position, transform.rotation);
        currentTank.transform.SetParent(transform.parent);

        PlayerEvents.OnTankSpawn.Invoke(currentTank);

        TankInstanceSetUp tankSetUp = GetComponent<TankInstanceSetUp>();
        if (tankSetUp != null) 
        {
            tankSetUp.tankInstance = currentTank;
            tankSetUp.SetUpTank();
        }
        
        // Get interfaces
        
        currentFSM = currentTank.GetComponent<TankVehicleFSM>();
        currentDrivable = currentFSM.body.GetComponent<IDrivable>();
        currentTurret = currentFSM.turret.GetComponent<ITurretControllable>();
        
        // Connect input handlers to tank
        inputHandler.SetTarget(currentDrivable, currentFSM);
        turretInputs.SetTarget(currentTurret);
    }
    
    public void OnTankDestroyed()
    {
        lives--;
        
        if (lives > 0)
        {
            SpawnNewTank();
        }
        else
        {
            Debug.Log("Game Over");
            // Game over logic here
        }
    }
}