using UnityEngine;
using UnityEngine.UIElements;

public class HUDControlls : MonoBehaviour
{
    private UIDocument HUD;
    private VisualElement healthBar;
    private VisualElement shieldBar;
    private VisualElement capacitordBar;
    private VisualElement batterydBar;
    private Label batteryValue;
    private Label scoreLabel;
    private Label livesLabel;
    private VisualElement root;

    [Range(0f, 1f)] public float health = 1f;
    [Range(0f, 1f)] public float shield = 1f;
    [Range(0f, 1f)] public float capacitor = 1f;
    [Range(0f, 1f)] public float battery = 1f;
    public int score;
    public int lives = 3;
    public bool testing = true;

    void OnEnable()
    {
        HUD = GetComponent<UIDocument>();
        root = HUD.rootVisualElement;
        healthBar = root.Q<VisualElement>("health-bar");
        shieldBar = root.Q<VisualElement>("armor-bar");
        capacitordBar = root.Q<VisualElement>("capacitor-bar");
        batterydBar = root.Q<VisualElement>("battery-bar");
        batteryValue = root.Q<Label>("battery-value-label");
        scoreLabel = root.Q<Label>("score-label");
        livesLabel = root.Q<Label>("lives-label");

        GameEvents.OnHealthUpdate.Subscribe(UpdateHealth);
        GameEvents.OnShieldUpdate.Subscribe(UpdateShield);
        GameEvents.OnLivesUpdate.Subscribe(UpdateLives);
        GameEvents.OnScoreUpdate.Subscribe(UpdateScore);
        GameEvents.OnCapacitorUpdate.Subscribe(UpdateCapacitor);
        GameEvents.OnBatteryUpdate.Subscribe(UpdateBattery);
        GameEvents.OnRefreshHUD.Subscribe(UpdateHUD);
    }
    void OnDisable()
    {
        GameEvents.OnHealthUpdate.Unsubscribe(UpdateHealth);
        GameEvents.OnShieldUpdate.Unsubscribe(UpdateShield);
        GameEvents.OnLivesUpdate.Unsubscribe(UpdateLives);
        GameEvents.OnScoreUpdate.Unsubscribe(UpdateScore);
        GameEvents.OnCapacitorUpdate.Unsubscribe(UpdateCapacitor);
        GameEvents.OnBatteryUpdate.Unsubscribe(UpdateBattery);
        GameEvents.OnRefreshHUD.Unsubscribe(UpdateHUD);
    }
    public void UpdateHUD()
    {
        if (HUD == null) Debug.Log("hud is null");
        Debug.Log($"health bar: {health}");
        healthBar.style.width = Length.Percent(health * 100f);
        shieldBar.style.width = Length.Percent(shield * 100f);
        capacitordBar.style.width = Length.Percent(capacitor * 100f);
        batterydBar.style.width = Length.Percent(battery * 100f);
        batteryValue.text = $"{(int)(battery * 100f)}%";
        scoreLabel.text = $"Score: {score:D4}";
        livesLabel.text = $"x{lives}";
    }
    void Update()
    {
        if (testing) UpdateHUD();
    }

    public void UpdateHealth(float healthPercent)
    {
        health = healthPercent;
        UpdateHUD();
    }
    public void UpdateShield(float shieldPercent)
    {
        shield = shieldPercent;
        UpdateHUD();
    }
    public void UpdateLives(int count)
    {
        lives = count;
        UpdateHUD();
    }
    public void UpdateScore(int points)
    {
        score = points;
        UpdateHUD();
    }
    public void UpdateCapacitor(float capPercent)
    {
        capacitor = capPercent;
        UpdateHUD();
    }
    public void UpdateBattery(float chargePercent)
    {
        battery = chargePercent;
        UpdateHUD();
    }
}