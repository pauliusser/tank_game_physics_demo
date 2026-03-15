using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class CursorTarget : MonoBehaviour
{
    public Texture2D dotCursorTexture;
    public BallisticTrajectory trajectory;
    private UIDocument UI;
    private VisualElement dot;
    private VisualElement cross;
    private Vector3 crossScreenPos;
    private Vector2 offset;
    private Vector2 crossTr;
    private Vector2 mouseScreenPos;
    private Vector2 dotTr;
    public bool isLockedOnTarget = false;
    void Start()
    {
        UI = GetComponent<UIDocument>();
        dot = UI.rootVisualElement.Q<VisualElement>("dot");
        cross = UI.rootVisualElement.Q<VisualElement>("cross");

        offset.x = Screen.width / 2f;
        offset.y = Screen.height / 2f;

        if (dotCursorTexture != null)
        {
            UnityEngine.Cursor.SetCursor(dotCursorTexture, new Vector2(16, 16), CursorMode.Auto);
        }
    }

    void Update()
    {
        mouseScreenPos = Mouse.current.position.ReadValue();
        dotTr.x = mouseScreenPos.x - offset.x;
        dotTr.y = -mouseScreenPos.y + offset.y;

        crossScreenPos = Camera.main.WorldToScreenPoint(trajectory.crossPoint);
        crossTr.x = crossScreenPos.x - offset.x;
        crossTr.y = -crossScreenPos.y + offset.y;

        isLockedOnTarget = trajectory.isColidingEnemy;

        dot.style.translate = dotTr;
        cross.style.translate = crossTr;
        cross.style.rotate = isLockedOnTarget ? Quaternion.Euler(0f, 0f, 45f) : Quaternion.identity;
    }
}
