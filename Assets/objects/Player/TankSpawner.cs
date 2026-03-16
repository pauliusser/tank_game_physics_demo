using UnityEngine;

public class TankSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerTankPrefab;
    [SerializeField] private Transform[] spawnPoints;
    
    public static TankSpawner Instance { get; private set; }
    
    private void Awake()
    {
        Instance = this;
    }
    
    public GameObject SpawnPlayerTank()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        return Instantiate(playerTankPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}