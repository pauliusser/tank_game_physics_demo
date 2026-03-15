using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

public class MainMenu : MonoBehaviour
{
    private Button playButton;
    private Button exitButton;
    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        playButton = root.Q<Button>("play_btn");
        playButton.clicked += OnPlayClicked;

        exitButton = root.Q<Button>("exit_btn");
        exitButton.clicked += OnExitClicked;
    }
    private void OnPlayClicked()
    {
        // Debug.Log("play button clicked");
        // SceneManager.LoadScene("GameScene");
        GameEvents.OnGameStart.Invoke();
    }
    private void OnExitClicked()
    {
        Debug.Log("exit button clicked");
        #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
