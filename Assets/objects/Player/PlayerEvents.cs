using UnityEngine;

public static class PlayerEvents
{
   public static Event<GameObject> OnTankSpawn = new Event<GameObject>();
   public static Event<int> OnPlayerScored = new Event<int>();
    public static Event OnPlayerDied = new Event();

}
