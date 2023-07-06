using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using PathCreation;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class FabriqueMachine : MonoBehaviour
{
    public event Action<FabriqueMachine> GenerationCompleteEvent;
    public int NeededObjectsCount => enterObjectsCount;
    public int ExitObjectsCount => exitObjectsCount;

    public bool isVitrine = false;
    public bool IsAvailableVitrine => _bought && isVitrine;
    public bool IsBought => _bought;

    public bool CanBuy => _canBuy;

    [SerializeField] private Transform objectToAnimate;
    [SerializeField] private bool moveResourceFromFabrique = false;
    [SerializeField] private bool animate = true;
    [SerializeField] private PathCreator pathToFabrique;
    [ShowIf(nameof(moveResourceFromFabrique))] [SerializeField]
    private PathCreator pathFromFabrique;
    [SerializeField] private ObjectSettings[] objectsToEnter;
    [SerializeField] private CollectableObject objectToExit;
    [SerializeField] private int enterObjectsCount = 1;
    [SerializeField] private int exitObjectsCount = 1;
    [SerializeField] private float makeObjectTime = 1f;
    [SerializeField] private float moveObjectSpeed = 1f;
    [SerializeField] private Transform makeObjectPosition;
    [SerializeField] private GameObject boughtGameObject;
    [SerializeField] private PurchaserInfo purchaserGameObject;
    [SerializeField] private int price;
    [SerializeField] private Transform[] customerTargets;
    [SerializeField] private Transform workerTarget;
    [ShowIf(nameof(isVitrine))][SerializeField] private WorkerBehaviour globalWorker;
    [SerializeField] private ObjectSettings workerObjectSettings;
    [SerializeField] private bool affectedByUpgrades = false;
    [SerializeField] private ParticleSystem makeObjectParticles;

    [ShowIf(nameof(isVitrine))] [SerializeField]
    private Transform initPosition;

    [ShowIf(nameof(isVitrine))] [SerializeField]
    private Vector2Int dimension;

    [ShowIf(nameof(isVitrine))] [SerializeField]
    private Vector3 offset;
    
    public UnityEvent OnBuyEvent;

    private int _currentResourcesInWork = 0;
    private Queue<CollectableObject> _resourcesQueue = new Queue<CollectableObject>();
    private Stack<CollectableObject> _exitResources = new Stack<CollectableObject>();
    private bool _procesStarted = false;
    private Vector3 _initialScale = Vector3.one;
    private Coroutine _takeObjectRoutine = null;
    private Coroutine _instantiateObjectRoutine = null;

    private bool _bought = false;
    private bool _canBuy = false;
    private int _addedMoney = 0;
    private float _moveObjectSpeed = 0f;
    private float _makeobjectTime = 0f;
    [SerializeField] private bool needToCheckProgress = true;

    private void OnDisable()
    {
        purchaserGameObject.BuyCancelationEvent -= OnBuyCanceled;
        OnDisableEvent();
    }

    public virtual void OnDisableEvent()
    {
        
    }

    public void EnableForBuy()
    {
        Initialize(false, true);
        FactorySaves.Instance.ActualizeFabriqueData(this, false, true, 0);
    }

    public bool HasAnySpace()
    {
        if (!isVitrine)
        {
            return true;
        }

        return true;
    }
    
    public virtual void Initialize(bool bought, bool canBuy, int moneyAdded = 0, int currentObjectsCount = 0)
    {
        _currentResourcesInWork = 0;
        _initialScale = objectToAnimate.localScale;
        
        _bought = bought;
        _canBuy = !_bought && canBuy;
        _addedMoney = moneyAdded;
        
        boughtGameObject.SetActive(_bought);
        purchaserGameObject.gameObject.SetActive(false);

        if (_canBuy)
        {
            purchaserGameObject.Initialize(price, _addedMoney);
            purchaserGameObject.BuyEvent += OnBuy;
            purchaserGameObject.BuyCancelationEvent += OnBuyCanceled;
        }

        if (affectedByUpgrades)
        {
            _moveObjectSpeed = moveObjectSpeed + UpgradeSaves.Instance.processingSpeedUpgrades * 1f;
            _makeobjectTime = Mathf.Clamp(makeObjectTime - UpgradeSaves.Instance.processingSpeedUpgrades * 0.05f,
                0.1f, makeObjectTime);
        }
        else
        {
            _moveObjectSpeed = moveObjectSpeed;
            _makeobjectTime = makeObjectTime;
        }
        
        if(isVitrine)
            InstantiateResources(currentObjectsCount);
    }

    [Button]
    public void InstantiateResources(int currentObjectsCount)
    {
        for (int i = 0; i < currentObjectsCount; i++)
        {
            var resource = Instantiate(objectToExit, transform.position, Quaternion.identity);
            PositionateResource(resource);
        }
    }
    
    public virtual bool TakeObjectForWork(CollectableObject resource, bool direct = false)
    {
        if (_bought == false)
        {
            return false;
        }
        
        if (ValidResource(resource) == false)
        {
            return false;
        }

        if (direct)
        {
            resource.transform.DOMove(makeObjectPosition.position, 0.2f);
            AddResourceForWork(resource);
            return true;
        }

        if (affectedByUpgrades)
        {
            _moveObjectSpeed = moveObjectSpeed + UpgradeSaves.Instance.processingSpeedUpgrades * 1f;
            _makeobjectTime = Mathf.Clamp(makeObjectTime - UpgradeSaves.Instance.processingSpeedUpgrades * 0.3f,
                0.1f, makeObjectTime);
        }
        else
        {
            _moveObjectSpeed = moveObjectSpeed;
            _makeobjectTime = makeObjectTime;
        }

        resource.StartMovingAlongCurve(pathToFabrique, _moveObjectSpeed);
        resource.ReachedDestinationEvent += OnResourceReachedDestination;

        return true;
    }

    public virtual void OnResourceReachedDestination(CollectableObject resource)
    {
        resource.ReachedDestinationEvent -= OnResourceReachedDestination;

        if (isVitrine)
        {
            PasteOnVitrine(resource);
        }
        else
        {
            AddResourceForWork(resource);   
        }
    }

    public CollectableObject GetResource()
    {
        if (_exitResources.Count == 0)
        {
            return null;
        }
        
        var resource = _exitResources.Pop();
        FactorySaves.Instance.ActualizeFabriqueData(this, _bought, _canBuy, 0, _exitResources.Count);
        return resource;
    }

    public Transform GetCustomerTarget()
    {
        var index = Random.Range(0, customerTargets.Length);
        return customerTargets[index];
    }

    public ObjectSettings GetObjectsSettings()
    {
        return objectsToEnter[0];
    }

    public ObjectSettings GetWorkerObjectSettings()
    {
        return workerObjectSettings;
    }

    public Transform GetWorkerTarget()
    {
        return workerTarget;
    }
    
    private void MoveObjectToExit()
    {
        for (int i = 0; i < exitObjectsCount; i++)
        {
            var resource = GetResource();
            _exitResources.Push(resource);

            resource.StartMovingAlongCurve(pathFromFabrique, _moveObjectSpeed);
            resource.ReachedDestinationEvent += OnReachedExitDestination;
        }
    }

    private void OnReachedExitDestination(CollectableObject resource)
    {
        resource.ReachedDestinationEvent -= OnReachedExitDestination;
        GenerationCompleteEvent?.Invoke(this);
    }
    
    private bool ValidResource(CollectableObject resource)
    {
        foreach (var obj in objectsToEnter)
        {
            if (resource.PropertyMatch(obj))
            {
                return true;
            }
        }

        return false;
    }
    
    private void AddResourceForWork(CollectableObject resource)
    {
        _resourcesQueue.Enqueue(resource);
        _currentResourcesInWork += 1;
        print("CURRENT RESOURCES COUNT = " + _resourcesQueue.Count);
        
        if (_procesStarted == false)
        {
            MakeObject();
            ScaleAnimation();
        }
    }
    
    private void MakeObject()
    {
        if (_resourcesQueue.Count < enterObjectsCount)
        {
            _procesStarted = false;
            return;
        }

        _procesStarted = true;
        print("START ROUTINE!!");
        StartCoroutine(GenerateObjectsRoutine());
    }

    private IEnumerator GenerateObjectsRoutine()
    {
        var delay = new WaitForSeconds(_makeobjectTime);
        
        for (int i = 0; i < enterObjectsCount; i++)
        {
            var inputObject = _resourcesQueue.Dequeue();
            inputObject.transform.DOMove(makeObjectPosition.position, _makeobjectTime);
            _currentResourcesInWork -= 1;

            yield return delay;
            
            Destroy(inputObject.gameObject);
        }

        yield return StartCoroutine(InstantiateObjects());

        yield return null;
    }
    
    private IEnumerator InstantiateObjects()
    {
        var delay = new WaitForSeconds(_makeobjectTime);
        print("MAKE OBJECT!");
        
        for (int i = 0; i < exitObjectsCount; i++)
        {
            var resource = Instantiate(objectToExit, makeObjectPosition.position, Quaternion.identity);
            if (makeObjectParticles != null)
            {
                makeObjectParticles.Play();
            }
            
            resource.RotateToBaseValue();
            resource.transform.SetParent(transform);
            _exitResources.Push(resource);
        }
        
        yield return delay;
        
        MakeObject();
        if (moveResourceFromFabrique)
        {
            MoveObjectToExit();
        }
        else
        {
            GenerationCompleteEvent?.Invoke(this);
            Tutorial.Instance.MoveNext(Tutorial.TutorialPath.MakeChair);
            if (Tutorial.Instance.TutorialCompleted == false)
            {
                UIManager.Instance.bottomMenu.InteractionState(true);
            }
        }
        
        yield return null;
    }
    
    private void ScaleAnimation()
    {
        if (_procesStarted == false)
        {
            return;
        }

        if (isVitrine)
        {
            return;
        }

        if (animate == false)
        {
            return;
        }

        var scale = _initialScale + (Vector3.one * 0.05f);
        objectToAnimate.DOScale(scale, _makeobjectTime)
            .OnComplete(() =>
            {
                objectToAnimate.DOScale(_initialScale, _makeobjectTime).OnComplete(ScaleAnimation);
            });
    }

    private void PasteOnVitrine(CollectableObject resource)
    {
        PositionateResource(resource);
    }

    private void PositionateResource(CollectableObject resource)
    {
        var positionToMove = initPosition.position;

        int rowsUpCount = _exitResources.Count / (dimension.x * dimension.y);

        int columnIndex = _exitResources.Count / dimension.y;
        columnIndex -= rowsUpCount * dimension.x;

        int rowIndex = _exitResources.Count - (columnIndex * dimension.y);
        rowIndex -= rowsUpCount * dimension.y * dimension.x;

        positionToMove.x += offset.x * columnIndex;
        positionToMove.z -= offset.z * rowIndex;
        positionToMove.y += offset.y * rowsUpCount;

        resource.transform.DORotate(new Vector3(0,0,90), 0.5f);
        resource.SetGlobalPosition(positionToMove);

        resource.transform.SetParent(transform);
        _exitResources.Push(resource);
        FactorySaves.Instance.ActualizeFabriqueData(this, _bought, _canBuy, 0, _exitResources.Count);
    }
    
    public void EnqueueObject(CollectableObject resource)
    {
        
    }
    
    public virtual void OnBuy()
    {
        purchaserGameObject.BuyEvent -= OnBuy;
        Initialize(true, false);
        FactorySaves.Instance.ActualizeFabriqueData(this, true, false, 0);
        
        if (isVitrine)
        {
            globalWorker.EnableObjectForBuy();
        }
        
        if(needToCheckProgress)
            Progress.Instance.CheckProgress();

        OnBuyEvent.Invoke();
    }

    private void OnBuyCanceled(int moneyAdded)
    {
        _addedMoney = moneyAdded;
        FactorySaves.Instance.ActualizeFabriqueData(this, false, true, _addedMoney);
    }
}
