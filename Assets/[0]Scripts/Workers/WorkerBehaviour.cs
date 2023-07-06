using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Pathfinding;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

[RequireComponent(typeof(CharacterData))]
public class WorkerBehaviour : MonoBehaviour
{
    [SerializeField] private WorkerType workerType = WorkerType.FabriquePlacer;
    [SerializeField] private Animator workerAnimator;
    [SerializeField] private Vector3 onTargetRotation;
    [SerializeField] private ObjectsHolder objectsHolder;
    [SerializeField] private Transform waitingPosition;
    [SerializeField] private ObjectSettings resourceToTarget;
    [SerializeField] private GameObject WorkerVisual;
    public FactoryShop shop;

    private readonly int Movement = Animator.StringToHash("Movement");
    private readonly int Holder = Animator.StringToHash("ObjectsInHands");

    private Transform currentTarget = null;
    private AIPath _aiPath;
    private CharacterData _data;
    public CharacterData Data
    {
        get
        {
            if (_data == null)
            {
                _data = GetComponent<CharacterData>();
            }
            
            return _data;
        }
    }

    private WorkerState _workerState = WorkerState.FillResources;
    private ObjectGenerator _honeyZone;
    private ExportMachineObject _exportMachine;
    private ObjectCreatorMachine _objectCreatorMachine;
    private Collider _colider;
    private Rigidbody _rigidbody;
    private bool _bought = false;
    private bool _canBuy = false;

    public void Initialize(bool bought, bool canBuy)
    {
        _aiPath = GetComponent<AIPath>();
        _data = GetComponent<CharacterData>();
        _colider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();

        _bought = bought;
        _canBuy = canBuy;
        
        objectsHolder.SetCharacterData(_data);

        if (_bought)
        {
            WorkerVisual.SetActive(true);
            _colider.enabled = true;
            _rigidbody.isKinematic = false;
            FindWorkerTarget();
        }
        else
        {
            _colider.enabled = false;
            _rigidbody.isKinematic = true;
            WorkerVisual.SetActive(false);
        }
    }

    public bool MatchType(WorkerType _type)
    {
        return _type == workerType;
    }

    public bool CheckShop(FactoryShop _shop)
    {
        return shop == _shop;
    }
    
    public bool CanBuy()
    {
        return _canBuy;
    }

    public bool IsBought()
    {
        return _bought;
    }

    public void Buy()
    {
        Initialize(true, false);
        UpgradeSaves.Instance.ActualizeUpgrades(this, _data, _data.CurrentUpgrades, _bought, _canBuy);
    }

    public void EnableObjectForBuy()
    {
        Initialize(false, true);
        UpgradeSaves.Instance.ActualizeUpgrades(this, _data, _data.CurrentUpgrades, _bought, _canBuy);
    }

    private void FindWorkerTarget()
    {
        switch (workerType)
        {
            case WorkerType.FabriquePlacer:
                currentTarget = GetFabriquePlacerTarget();
                break;
            case WorkerType.Cassier:
                currentTarget = GetCassierTarget();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (currentTarget == null)
        {
            return;
        }
        
        SetTargetDestination(currentTarget.position);
    }

    private Transform GetFabriquePlacerTarget()
    {
        if (_workerState == WorkerState.TakeResources && objectsHolder.HandsIsFull())
        {
            _workerState = WorkerState.FillResources;
            return FactorySaves.Instance.GetWorkerTargetByResource(resourceToTarget, shop);
        }
        
        if (_workerState == WorkerState.FillResources && objectsHolder.HandsIsEmpty())
        {
            _workerState = WorkerState.TakeResources;
            return shop.warehouse.GetWorkerTarget();
        }

        return null;
    }

    private Transform GetCassierTarget()
    {
        return shop.cassaObject.workerTarget;
    }
    
    public void TakeResource(CollectableObject resource, Vector3 scale)
    {
        var result = objectsHolder.AddToList(resource, scale);
        if (result)
        {
            workerAnimator.SetBool(Holder, true);
        }
        
        FindWorkerTarget();
    }

    public bool CheckBeforeTakeResource(ObjectSettings settings)
    {
        if (resourceToTarget != null)
        {
            return resourceToTarget == settings;
        }

        return true;
    }

    public void FindNextTarget()
    {
        
    }

    public int ObjectsCountInHand()
    {
        return objectsHolder.ObjectsCountInHand();
    }

    public CollectableObject GiveResource(ObjectSettings resource)
    {
        var result = objectsHolder.RemoveFromList(resource);
        if (objectsHolder.HandsIsEmpty())
        {
            workerAnimator.SetBool(Holder, false);
            FindWorkerTarget();
        }

        return result;
    }

    public bool CanTake()
    {
        return !objectsHolder.HandsIsFull();
    }
    
    private void SetTargetDestination(Vector3 destinationPosition)
    {
        _aiPath.SetPath(null);
        _aiPath.canMove = true;
        _aiPath.maxSpeed = _data.MovementSpeed;
        workerAnimator.SetBool(Movement, true);
        _aiPath.destination = destinationPosition;
        StartCoroutine(CheckReachDestination());
    }

    private void StopMovement()
    {
        _aiPath.SetPath(null);
        _aiPath.canMove = false;
        workerAnimator.SetBool(Movement, false);
    }

    private IEnumerator CheckReachDestination()
    {
        while (true)
        {
            if (_aiPath.reachedEndOfPath && !_aiPath.pathPending)
            {
                break;
            }

            yield return null;
        }
        
        StopMovement();
        RunEventOnReachDestination();

        yield return null;
    }
    
    private void RunEventOnReachDestination()
    {
        switch (workerType)
        {
            case WorkerType.FabriquePlacer:
                FindWorkerTarget();
                break;
            case WorkerType.Cassier:
                StopMovement();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
