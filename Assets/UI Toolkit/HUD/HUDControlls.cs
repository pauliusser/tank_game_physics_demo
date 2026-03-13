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

    [Range(0f, 1f)] public float health = 1f;
    [Range(0f, 1f)] public float sheald = 1f;
    [Range(0f, 1f)] public float capacitor = 1f;
    [Range(0f, 1f)] public float battery = 1f;
    public int score;
    public int lives = 3;
    public bool testing = true;

    void Start()
    {
        HUD = GetComponent<UIDocument>();
        var root = HUD.rootVisualElement;
        healthBar = root.Q<VisualElement>("health-bar");
        shieldBar = root.Q<VisualElement>("armor-bar");
        capacitordBar = root.Q<VisualElement>("capacitor-bar");
        batterydBar = root.Q<VisualElement>("battery-bar");
        batteryValue = root.Q<Label>("battery-value-label");
        scoreLabel = root.Q<Label>("score-label");
        livesLabel = root.Q<Label>("lives-label");
    }
    public void UpdateHUD()
    {
        healthBar.style.width = Length.Percent(health * 100f);
        shieldBar.style.width = Length.Percent(sheald * 100f);
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
}