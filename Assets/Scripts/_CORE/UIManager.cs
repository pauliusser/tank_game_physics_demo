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
    public UIDocument pause;

    
    public void ShowPause()
    {
        pause.enabled = true;
    }
    public void HidePause()
    {
        pause.enabled = false;
    }
    

}
