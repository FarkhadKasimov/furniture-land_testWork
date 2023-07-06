using System;
using DG.Tweening;
using PathCreation;
using UnityEngine;

public class CollectableObject : MonoBehaviour
{
    public event Action<CollectableObject> ReachedDestinationEvent;
        
    [SerializeField] private ObjectSettings objectSettings;
    [SerializeField] private Vector3 baseValue;

    public ObjectSettings Settings => objectSettings;

    private bool _move = false;
    private PathCreator _conveyor = null;
    private float _travelledDistance = 0f;
    private float _movementSpeed = 1f;

    private void Update()
    {
        if (_move == false)
        {
            return;
        }

        MoveAlongCurve();
    }

    public void SetPosition(Vector3 position)
    {
        transform.DOLocalMove(position, 0.2f).OnComplete(RotateToBaseValue);
    }

    public void SetPosition(Vector3 position, float duration)
    {
        transform.DOLocalMove(position, duration).OnComplete(RotateToBaseValue).SetEase(Ease.Linear);
    }
    
    public void SetPosition(Vector3 position, float duration, Action action)
    {
        transform.DOLocalMove(position, duration).OnComplete(()=>
        {
            action.Invoke();
            RotateToBaseValue();
        }).SetEase(Ease.Linear);
    }
    
    public void SetGlobalPosition(Vector3 position)
    {
        transform.DOMove(position, 0.2f);
    }
    
    public void SetGlobalPosition(Vector3 position, float duration)
    {
        transform.DOMove(position, duration);
    }

    public void RotateToBaseValue()
    {
        transform.localRotation = Quaternion.Euler(baseValue);
    }
    
    public void RotateToBaseValueGlobal()
    {
        transform.eulerAngles = baseValue;
    }


    public void RotateToBaseValue(float duration)
    {
        transform.DOLocalRotate(baseValue, duration).SetEase(Ease.Linear);
    }

    public void SetScale(Vector3 scale)
    {
        transform.localScale = scale;
    }

    public bool PropertyMatch(ObjectSettings resource)
    {
        return objectSettings == resource;
    }

    public void StartMovingAlongCurve(PathCreator conveyor, float speed)
    {
        _movementSpeed = speed;
        _travelledDistance = 0f;
        _conveyor = conveyor;
        _move = true;

        _conveyor.path.ReachDistance += StopMoving;
    }

    public void StopMoving()
    {
        _conveyor.path.ReachDistance -= StopMoving;
        _move = false;
        _travelledDistance = 0f;
        ReachedDestinationEvent?.Invoke(this);
    }

    private void MoveAlongCurve()
    {
        _travelledDistance += _movementSpeed * Time.deltaTime;
        
        transform.position = _conveyor.path.GetPointAtDistance(_travelledDistance, EndOfPathInstruction.Stop);
        //transform.rotation = _conveyor.path.GetRotationAtDistance(_travelledDistance, EndOfPathInstruction.Stop);
    }
}
