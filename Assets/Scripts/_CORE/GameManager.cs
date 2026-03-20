using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);  // <-- persists across scene loads

        CurrentState = GameState.MainMenu; // initialize state
        Debug.Log("GameManager singleton created");
    }


    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    public bool isRestart = false;
    public GameState CurrentState { get; private set; }

    private void OnEnable()
    {
        GameEvents.OnGameStart.Subscribe(StartGame);
        GameEvents.OnGameOver.Subscribe(GameOver);
        GameEvents.OnPauseToggled.Subscribe(TogglePause);
        GameEvents.OnEnterMainMenu.Subscribe(EnterMainMenu);
    }

    private void OnDisable()
    {
        GameEvents.OnGameStart.Unsubscribe(StartGame);
        GameEvents.OnGameOver.Unsubscribe(GameOver);
        GameEvents.OnPauseToggled.Unsubscribe(TogglePause);
        GameEvents.OnEnterMainMenu.Unsubscribe(EnterMainMenu);
    }

    private void StartGame() => SetState(GameState.Playing);
    private void GameOver() => SetState(GameState.GameOver);
    private void TogglePause() => SetState(CurrentState == GameState.Playing ? GameState.Paused : GameState.Playing);
    private void EnterMainMenu() => SetState(GameState.MainMenu);

    private void SetState(GameState newState)
    {
        if (CurrentState == newState) return;

        ExitState(CurrentState);
        CurrentState = newState;
        EnterState(CurrentState);
    }

    private void EnterState(GameState state)
    {
        switch(state)
        {
            case GameState.MainMenu:
                SceneManager.LoadScene("MenuScene");
                break;
            case GameState.Playing:
                if (SceneManager.GetActiveScene().name != "GameScene" || isRestart)
                {
                    isRestart = false;
                    SceneManager.LoadScene("GameScene");
                }
                break;
            case GameState.Paused:
                Debug.Log("pause state on");
                Time.timeScale = 0f;
                UIManager.Instance.ShowPause();
                break;
            case GameState.GameOver:
                UIManager.Instance.ShowGameOver();
                break;
        }
    }

    private void ExitState(GameState state)
    {
        switch(state)
        {
            case GameState.MainMenu:
                // UIManager.HideMenu();
                break;
            case GameState.Playing:
                // optional cleanup
                break;
            case GameState.Paused:
                Debug.Log("pause state off");
                Time.timeScale = 1f;
                UIManager.Instance.HidePause();
                break;
            case GameState.GameOver:
                // optional cleanup
                break;
        }
    }
}
