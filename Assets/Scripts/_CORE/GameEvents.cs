using System;

public static class GameEvents
{
    private static event Action OnGameStart;       
    public static void SubToOnGameStart(Action observer) => OnGameStart += observer;
    public static void UnsubFromOnGameStart(Action observer) => OnGameStart -= observer;
    public static void InvokeOnGameStart() => OnGameStart?.Invoke();


    private static event Action OnPauseToggled;       
    public static void SubToOnPauseToggled(Action observer) => OnPauseToggled += observer;
    public static void UnsubFromOnPauseToggled(Action observer) => OnPauseToggled -= observer;
    public static void InvokeOnPauseToggled() => OnPauseToggled?.Invoke();


    private static event Action OnPlayerDied;       
    public static void SubToOnOnPlayerDied(Action observer) => OnPlayerDied += observer;
    public static void UnsubFromOnPlayerDied(Action observer) => OnPlayerDied -= observer;
    public static void InvokeOnOnPlayerDied() => OnPlayerDied?.Invoke();
}
