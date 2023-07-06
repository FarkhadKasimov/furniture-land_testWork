using System;
using DG.Tweening;
using Project.Internal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class SimpleJoystick : Singleton<SimpleJoystick>
{
    [SerializeField] private Image joystickImage;
    [SerializeField] private float joystickMovementSpeed = 5f;
    [SerializeField] private float joysticPaddingOffset = 1;

    public bool ChangePLayerSpeed = false;
    
    private Image _backgroundImage;
    private CanvasGroup _canvasGroup;
    private Vector3 _inputVector;
    private bool _moveJoystick = false;
    private bool _controlling = false;
    private PointerEventData _lastPointerData;
    private float _maximumMagnitude = 0f;

    private void Awake()
    {
        _backgroundImage = GetComponent<Image>();
        _canvasGroup = GetComponent<CanvasGroup>();
        
        var width = _backgroundImage.rectTransform.rect.width;
        var height = _backgroundImage.rectTransform.rect.height;
        
        _maximumMagnitude = (new Vector3(width/joysticPaddingOffset,height / joysticPaddingOffset , 0).magnitude) / 2f;
    }

    private void Update()
    {
        _moveJoystick = joystickImage.rectTransform.anchoredPosition.magnitude > _maximumMagnitude;
        
        if (_moveJoystick == false)
        {
            return;
        }
        
        PositionateJoystickSmoothly(_lastPointerData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ChangeJoystickFade(0.6f);
        PositionateJoystick(eventData);
        OnDrag(eventData);

        _controlling = true;
    }
    public void OnDrag(PointerEventData eventData)
    {
        var pos = Vector2.zero;
        var backgroundRect = _backgroundImage.rectTransform;
        var joystickREct = joystickImage.rectTransform;
        
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(backgroundRect, eventData.position,
            eventData.enterEventCamera, out pos) == false)
        {
            return;
        }
        
        var width = backgroundRect.rect.width;
        var height = backgroundRect.rect.height;
        
        pos.x = (pos.x / width);
        pos.y = (pos.y / height);

        _inputVector = new Vector3(pos.x * 2, 0, pos.y * 2);
        _inputVector = _inputVector.magnitude > 1f ? _inputVector.normalized : _inputVector;

        joystickREct.anchoredPosition = new Vector3(_inputVector.x * (width / joysticPaddingOffset), _inputVector.z * (height / joysticPaddingOffset));
        _lastPointerData = eventData;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ChangeJoystickFade(0);
        _moveJoystick = false;
        _inputVector = Vector3.zero;
        joystickImage.rectTransform.anchoredPosition = Vector3.zero;
        _controlling = false;
    }
    
    public float Horizontal()
    {
        return _inputVector.x != 0 ? _inputVector.x : Input.GetAxis("Horizontal");
    }

    public float Vertical()
    {
        return _inputVector.z != 0 ? _inputVector.z : Input.GetAxis("Vertical");
    }

    private void ChangeJoystickFade(float alpha)
    {
        alpha = Mathf.Clamp01(alpha);
        _canvasGroup.DOFade(alpha, 0.2f);
    }
    
    private void PositionateJoystick(PointerEventData eventData)
    {
        _backgroundImage.transform.position = eventData.position;
    }
    
    private void PositionateJoystickSmoothly(PointerEventData eventData)
    {
        var beginPos = _backgroundImage.transform.position;
        var endPos = eventData.position - (joystickImage.rectTransform.anchoredPosition * 1.3f);
            
        _backgroundImage.transform.position = Vector3.MoveTowards(beginPos, endPos, joystickMovementSpeed * Time.deltaTime);
    }

    public bool Contolling()
    {
        return _controlling;
    }
}
