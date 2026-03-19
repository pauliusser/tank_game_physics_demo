using UnityEngine;

public class TankStats : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 300;
    public int Health { get; set; }
    public int maxShield = 200;
    public int shield = 200;
    public int lives = 3;
    public int score = 0;
    public float capacitor = 0f;
    public float maxCapacitor = 100f;
    public float battery = 0f;
    public float maxBattery = 3000f;

    void Awake()
    {
        Health = maxHealth;
    }
    void OnEnable()
    {
        PlayerEvents.OnPlayerDied.Subscribe(PlayerDied);
        PlayerEvents.OnPlayerScored.Subscribe(PlayerScored);
        if (gameObject.tag == "Player")
        {
            GameEvents.OnHealthUpdate.Invoke(1);
            GameEvents.OnShieldUpdate.Invoke(1);
        }
    }
    void OnDisable()
    {
        PlayerEvents.OnPlayerDied.Unsubscribe(PlayerDied);
        PlayerEvents.OnPlayerScored.Unsubscribe(PlayerScored);
    }
    void PlayerDied()
    {
        lives -= 1;
        GameEvents.OnLivesUpdate.Invoke(lives);
    }
    void PlayerScored(int points)
    {
        score += points;
        GameEvents.OnScoreUpdate.Invoke(score);
    }
    void Start()
    {
        GameEvents.OnRefreshHUD.Invoke();
        capacitor = maxCapacitor;
        battery = maxBattery;
    }
}
