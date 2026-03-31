using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Stats")]
    public int score;
    public int lives = 3;
    
    [Header("Tank Settings")]
    public GameObject tankPrefab;
    public float respawnTime = 1f;
    private GameObject currentTank;
    private bool isVictory = false;

    
    // Tank interfaces
    private IDrivable currentDrivable;
    private ITurretControllable currentTurret;
    private TankVehicleFSM currentFSM;
    
    // Input handlers
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private PlayerTurretInputs turretInputs;

    void OnEnable()
    {
        PlayerEvents.OnPlayerDied.Subscribe(OnTankDestroyed);
        PlayerEvents.OnPlayerScored.Subscribe(PlayerScored);
        GameEvents.OnVictory.Subscribe(OnVictory);
    }
    void OnDisable()
    {
        PlayerEvents.OnPlayerDied.Unsubscribe(OnTankDestroyed);
        PlayerEvents.OnPlayerScored.Unsubscribe(PlayerScored);
        GameEvents.OnVictory.Unsubscribe(OnVictory);
    }
    void PlayerScored(int points)
    {
        score += points;
        GameEvents.OnScoreUpdate.Invoke(score);
    }

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
           StartCoroutine(RespawnTank());
        }
        else
        {
            Debug.Log("Game Over");
            GameEvents.OnGameOver.Invoke();
            GameEvents.OnFinalScore.Invoke(score);
        }
    }
    public void OnVictory()
    {
        isVictory = true;
    }
    System.Collections.IEnumerator RespawnTank()
    {
        yield return new WaitForSeconds(1f);
        SpawnNewTank();
        GameEvents.OnHealthUpdate.Invoke(1f);
        GameEvents.OnShieldUpdate.Invoke(1f);
    }
    void LateUpdate()
    {
      if (!isVictory) return;
        isVictory = false;
        GameEvents.OnFinalScore.Invoke(score);
    }
}