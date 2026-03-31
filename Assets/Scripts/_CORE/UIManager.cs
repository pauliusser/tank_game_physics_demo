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
    public GameObject cursor;
    public GameObject gameOver;
    public GameObject victory;

    
    public void ShowPause()
    {
        pause.GetComponent<UIDocument>().enabled = true;
        pause.GetComponent<PauseScreen>().InitializeUI();
        cursor.GetComponent<CursorTarget>().ResetCursor();
    }
    public void HidePause()
    {
        pause.GetComponent<UIDocument>().enabled = false;
        cursor.GetComponent<CursorTarget>().SetDotCursor();
    }
    public void ShowGameOver()
    {
        gameOver.GetComponent<UIDocument>().enabled = true;
        gameOver.GetComponent<GameOverScreen>().InitializeUI();
        cursor.GetComponent<CursorTarget>().ResetCursor();
    }
    public void ShowVictory()
    {
        victory.GetComponent<UIDocument>().enabled = true;
        victory.GetComponent<VictoryScreen>().InitializeUI();
        cursor.GetComponent<CursorTarget>().ResetCursor();
    }
}
