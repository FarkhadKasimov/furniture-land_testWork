using UnityEngine;
using UnityEngine.EventSystems;

public class JoystickController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private SimpleJoystick _joystick => SimpleJoystick.Instance;
    
    public void OnPointerDown(PointerEventData eventData)
    {
        _joystick.OnPointerDown(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _joystick.OnPointerUp(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        _joystick.OnDrag(eventData);
    }
}
