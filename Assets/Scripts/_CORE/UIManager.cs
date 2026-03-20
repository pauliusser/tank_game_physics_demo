using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [Header("References")]

    public GameObject HUD;
    public GameObject pause;
    public GameObject gameOver;

    
    public void ShowPause()
    {
        pause.GetComponent<UIDocument>().enabled = true;
        pause.GetComponent<PauseScreen>().InitializeUI();
    }
    public void HidePause()
    {
        pause.GetComponent<UIDocument>().enabled = false;
    }
    public void ShowGameOver()
    {
        gameOver.GetComponent<UIDocument>().enabled = true;
        gameOver.GetComponent<GameOverScreen>().InitializeUI();
    }
}
