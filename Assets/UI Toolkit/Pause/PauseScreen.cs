using UnityEngine;
using UnityEngine.UIElements;

public class PauseScreen : MonoBehaviour
{
    private Button restartButton;
    private Button mainMenuButton;
    private UIDocument uiDocument;
    private bool uiInitialized = false;
    public bool debug = false;
    
    void Start()
    {
        if (uiDocument == null) 
            uiDocument = GetComponent<UIDocument>();
    }
    
    // Called by UIManager after enabling the UI Document
    public void InitializeUI()
    {
        if (uiInitialized) return;
        
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                if (debug) Debug.LogError("PauseScreen: No UIDocument found!");
                return;
            }
        }
        
        if (!uiDocument.enabled)
        {
            if (debug) Debug.LogWarning("PauseScreen: UIDocument is not enabled");
            return;
        }
        
        if (uiDocument.rootVisualElement == null)
        {
            if (debug) Debug.LogError("PauseScreen: rootVisualElement is null - UI might not be ready");
            return;
        }

        var root = uiDocument.rootVisualElement;
        
        restartButton = root.Q<Button>("restart-btn");
        if (restartButton != null)
        {
            restartButton.clicked += RestartClick;
        }
        else
        {
            if (debug) Debug.LogError("PauseScreen: Could not find 'restart-btn'");
        }
        
        mainMenuButton = root.Q<Button>("MainMenu-btn");
        if (mainMenuButton != null)
        {
            mainMenuButton.clicked += MainMenuClick;
        }
        else
        {
            if (debug) Debug.LogError("PauseScreen: Could not find 'MainMenu-btn'");
        }
        
        uiInitialized = true;
        if (debug) Debug.Log("PauseScreen UI initialized successfully");
    }
    
    void OnDisable()
    {
        // Clean up event listeners when disabled
        if (restartButton != null)
            restartButton.clicked -= RestartClick;
        if (mainMenuButton != null)
            mainMenuButton.clicked -= MainMenuClick;
        
        uiInitialized = false;
    }
    
    private void RestartClick()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isRestart = true;
            GameEvents.OnGameStart.Invoke();
        }
    }
    
    private void MainMenuClick()
    {
        GameEvents.OnEnterMainMenu.Invoke();
    }
}