using System;
public class GameEvent<T>
{
    private event Action<T> listeners;

    public void Subscribe(Action<T> listener) => listeners += listener;
    public void Unsubscribe(Action<T> listener) => listeners -= listener;
    public void Invoke(T data) => listeners?.Invoke(data);
}

public class GameEvent
{
    private event Action listeners;

    public void Subscribe(Action listener) => listeners += listener;
    public void Unsubscribe(Action listener) => listeners -= listener;
    public void Invoke() => listeners?.Invoke();
}

public static class GameEvents
{
    public static GameEvent OnGameStart = new GameEvent();
    public static GameEvent OnPauseToggled = new GameEvent();
    public static GameEvent OnPlayerDied = new GameEvent();


    public static GameEvent OnRefreshHUD = new GameEvent();
    public static GameEvent<float> OnHealthUpdate = new GameEvent<float>();
    public static GameEvent<float> OnShieldUpdate = new GameEvent<float>();
    public static GameEvent<int> OnLivesUpdate = new GameEvent<int>();
    public static GameEvent<int> OnScoreUpdate = new GameEvent<int>();
    public static GameEvent<float> OnCapacitorUpdate = new GameEvent<float>();
    public static GameEvent<float> OnBatteryUpdate = new GameEvent<float>();
    
    public static GameEvent<int> OnPlayerScored = new GameEvent<int>();
}