using Project.Internal;
using UnityEngine.Events;

public class EventsHolder : Singleton<EventsHolder>
{ 
    public UnityEvent DieEvent;
    public UnityEvent LoseEvent;
    public UnityEvent WinEvent;
    public UnityEvent RespawnEvent;
}
