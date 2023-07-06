using UnityEngine;
using Plane = UnityEngine.Plane;
using Vector3 = UnityEngine.Vector3;

public class CameraMovement : MonoBehaviour
{
    public bool CanControll = true;
    
    [SerializeField] private Camera mainCamera = null;
    
    [SerializeField] private float moveSpeed;
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float rotatingSpeed = 0.1f;

    [SerializeField] private Vector2Int zoomLimitations = new Vector2Int(30, 60);
    [SerializeField] private Vector2Int cameraLimitationsX;
    [SerializeField] private Vector2Int cameraLimitationsZ;

    [SerializeField] private bool debugFromUnityRemote = false;
    
    private Transform _cameraRigTransform;
    private float _cameraYPosition;
    private Vector3 _dragStartPosition;
    private Vector3 _dragCurrentPosition;
    private Vector3 _newCameraPosition;
    private bool _firstButtonPressed = false;

    private void Awake()
    {
        _cameraRigTransform = transform;
        _newCameraPosition = _cameraRigTransform.position;
        _cameraYPosition = _newCameraPosition.y;
    }

    private void Update()
    {
        if (CanControll == false)
        {
            return;
        }

        CheckTouchManipulation();
    }

    private void SwipeCamera()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (plane.Raycast(ray, out var entry))
            {
                _dragStartPosition = ray.GetPoint(entry);
                _firstButtonPressed = true;
            }
        }

        if (Input.GetMouseButton(0) && _firstButtonPressed)
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (plane.Raycast(ray, out var entry))
            {
                _dragCurrentPosition = ray.GetPoint(entry);
                _newCameraPosition = _cameraRigTransform.transform.position + _dragStartPosition - _dragCurrentPosition;
            }
        }

        if (Input.GetMouseButtonUp(0) && Input.touchCount > 0)
        {
            _firstButtonPressed = false;
        }

        _newCameraPosition.x = Mathf.Clamp(_newCameraPosition.x, cameraLimitationsX.x, cameraLimitationsX.y);
        _newCameraPosition.z = Mathf.Clamp(_newCameraPosition.z, cameraLimitationsZ.x, cameraLimitationsZ.y);
        _newCameraPosition.y = _cameraYPosition;

        _cameraRigTransform.position 
            = Vector3.Lerp(_cameraRigTransform.position,_newCameraPosition, moveSpeed * Time.deltaTime);
    }

    private void CheckTouchManipulation()
    {
        
        if (Input.touchCount >= 2)
        {
            var firstTouch = Input.GetTouch(0);
            var secondTouch = Input.GetTouch(1);

            Zooming(firstTouch, secondTouch);
            Rotating(firstTouch, secondTouch);
        }
        else if (Input.touchCount == 1)
        {
            SwipeCamera();
        }

#if UNITY_EDITOR
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            mainCamera.fieldOfView = Mathf.Clamp(mainCamera.fieldOfView - Input.GetAxis("Mouse ScrollWheel") * zoomSpeed, zoomLimitations.x, zoomLimitations.y);
            return;
        }

        if (Input.GetMouseButton(1))
        {
            _cameraRigTransform.Rotate(0, Input.GetAxis("Mouse X") * rotatingSpeed, 0);
            return;
        }
        
        if (debugFromUnityRemote == false)
        {
            SwipeCamera();
        }
#endif
    }
    
    private void Zooming(Touch firstTouch, Touch secondTouch)
    {
        var touchZeroPrevPos = firstTouch.position - firstTouch.deltaPosition;
        var touchOnePrevPos = secondTouch.position - secondTouch.deltaPosition;

        var prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
        var currentMagnitude = (firstTouch.position - secondTouch.position).magnitude;

        var difference = currentMagnitude - prevMagnitude;

        mainCamera.fieldOfView = Mathf.Clamp(mainCamera.fieldOfView - (difference*zoomSpeed), zoomLimitations.x, zoomLimitations.y);
    }

    private void Rotating(Touch firstTouch, Touch secondTouch)
    {
        var lastPosition = _cameraRigTransform.position;
        
        var firstTouchPosition = firstTouch.position;
        var secondTouchPosition = secondTouch.position;
        var firstTouchDistance = firstTouch.position - firstTouch.deltaPosition;
        var secondTouchDistance = secondTouch.position - secondTouch.deltaPosition;
        
        var angle = Vector3.SignedAngle(secondTouchDistance - firstTouchDistance, secondTouchPosition - firstTouchPosition, Vector3.forward) * rotatingSpeed;
        _cameraRigTransform.Rotate(0, angle, 0);
        _cameraRigTransform.position = lastPosition;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        
        Gizmos.DrawLine(new Vector3(cameraLimitationsX.x, _cameraYPosition, cameraLimitationsZ.x)
            ,new Vector3(cameraLimitationsX.y, _cameraYPosition, cameraLimitationsZ.x));
        
        Gizmos.DrawLine(new Vector3(cameraLimitationsX.y, _cameraYPosition, cameraLimitationsZ.x)
            ,new Vector3(cameraLimitationsX.y, _cameraYPosition, cameraLimitationsZ.y));
        
        Gizmos.DrawLine(new Vector3(cameraLimitationsX.y, _cameraYPosition, cameraLimitationsZ.y)
            ,new Vector3(cameraLimitationsX.x, _cameraYPosition, cameraLimitationsZ.y));
        
        Gizmos.DrawLine(new Vector3(cameraLimitationsX.x, _cameraYPosition, cameraLimitationsZ.y)
            ,new Vector3(cameraLimitationsX.x, _cameraYPosition, cameraLimitationsZ.x));
    }
}
