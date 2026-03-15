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
    public int capacitor = 100;
    public int battery = 100;

    void Awake()
    {
        Health = maxHealth;
    }
    void OnEnable()
    {
        GameEvents.OnPlayerDied.Subscribe(PlayerDied);
        GameEvents.OnPlayerScored.Subscribe(PlayerScored);
    }
    void OnDisable()
    {
        GameEvents.OnPlayerDied.Unsubscribe(PlayerDied);
        GameEvents.OnPlayerScored.Unsubscribe(PlayerScored);
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
    }
}
