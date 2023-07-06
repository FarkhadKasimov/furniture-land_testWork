using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FactoryFramework;
using Sirenix.OdinInspector;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class Warehouse : MonoBehaviour
{
    [SerializeField] private int maxCapacity = 100;
    [SerializeField] private Vector2Int dimension;
    [SerializeField] private Vector3 offset = Vector3.one * 0.2f;
    [SerializeField] private ObjectSettings[] objectsToEnter;
    [SerializeField] private CollectableObject objectToInstantiate;
    [SerializeField] private Transform initPosition;
    [SerializeField] private float stackSpeed = 0.05f;

    [HideIf(nameof(getResourcesFromPlayer))] [SerializeField]
    private float resourceStackSpeed = 0.2f;
    [SerializeField] private TextMeshPro objectsCountText;
    [SerializeField] private bool getResourcesFromPlayer = true;
    [ShowIf(nameof(getResourcesFromPlayer))]
    [SerializeField] private float clearWoodTransferTime = 1f;
    [SerializeField] private FabriqueMachine[] woodCleanerMachines;
    [SerializeField] private GameObject boughtGameObject;
    [SerializeField] private PurchaserInfo purchaserGameObject;
    [SerializeField] private int price;
    [SerializeField] private Transform workerTarget;

    public UnityEvent TutorialEvent;

    public bool IsBought => _bought;
    public bool CanBuy => _canBuy;
    
    private bool _bought = false;
    private bool _canBuy = false;
    private int _addedMoney = 0;
    private Coroutine _stackCoroutine = null;
    private Coroutine _workerStackingProcess = null;
    private Coroutine _clearRoutine = null;
    private PlayerController _playerController = null;
    private Stack<CollectableObject> _warehouseObjects = new Stack<CollectableObject>();
    private Queue<WorkerBehaviour> _workersInQueue = new Queue<WorkerBehaviour>();

    private void OnEnable()
    {
        if (getResourcesFromPlayer)
        {
            return;
        }

        foreach (var woodCleanerMachine in woodCleanerMachines)
        {
            woodCleanerMachine.GenerationCompleteEvent += OnGenerationComplete;
        }
        
    }

    private void OnDisable()
    {
        if (getResourcesFromPlayer)
        {
            return;
        }

        foreach (var woodCleanerMachine in woodCleanerMachines)
        {
            woodCleanerMachine.GenerationCompleteEvent -= OnGenerationComplete;
        }
        purchaserGameObject.BuyCancelationEvent -= OnBuyCanceled;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out _playerController))
        {
            if (_bought == false)
            {
                return;
            }
            
            StartStacking();
            if (getResourcesFromPlayer)
            {
                StartClearingWood();
            }
        }
        
        else if(other.TryGetComponent(out WorkerBehaviour workerBehaviour))
        {
            _workersInQueue.Enqueue(workerBehaviour);
            StartWorkerStacking();
        }

        else if(other.TryGetComponent(out WorkerLogBehaviour worker))
        {
            if (_bought == false)
            {
                return;
            }
            
            StartStackingWorker(worker);
            StartClearingWood();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out _playerController))
        {
            if (_bought == false)
            {
                return;
            }
            
            StopStacking();
        }
        
        if (other.TryGetComponent(out WorkerLogBehaviour worker))
        {
            if (_bought == false)
            {
                return;
            }
            
            StopStackingWorker(worker);
        }
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
    
    public void ShowToBuy()
    {
        if(_bought==false && _canBuy == false)
            Initialize(false, true, 0);
    }
    
    public virtual void Initialize(bool bought, bool canBuy, int moneyAdded = 0, int currentObjectsCount = 0)
    {
        _bought = bought;
        _canBuy = !_bought && canBuy;
        _addedMoney = moneyAdded;
        
        boughtGameObject.SetActive(_bought);
        purchaserGameObject.gameObject.SetActive(false);

        InstantiateResources(currentObjectsCount);
        
        if (_canBuy)
        {
            purchaserGameObject.Initialize(price, _addedMoney);
            purchaserGameObject.BuyEvent += OnBuy;
            purchaserGameObject.BuyCancelationEvent += OnBuyCanceled;
        }

        if (Tutorial.Instance.TutorialCompleted == false && _bought)
        {
            TutorialEvent.Invoke();
        }

        if (_bought && getResourcesFromPlayer && _warehouseObjects.Count > 0)
        {
            StartClearingWood();
        }
        
        UpdateCounter();
    }
    
    [Button]
    public void InstantiateResources(int currentObjectsCount)
    {
        for (int i = 0; i < currentObjectsCount; i++)
        {
            var resource = Instantiate(objectToInstantiate, transform.position, quaternion.identity);
            //PositionateResource(resource);
            PositionateResourceInPyramid(resource);
        }

        if (_bought && getResourcesFromPlayer && _warehouseObjects.Count > 0)
        {
            StartClearingWood();
        }
    }

    public bool AveilableWarehouse()
    {
        return _bought && getResourcesFromPlayer == false;
    }

    public Transform GetWorkerTarget()
    {
        return workerTarget;
    }
    
    public bool HasEnoughtResources(int count)
    {
        return _warehouseObjects.Count >= count;
    }

    public CollectableObject TakeResource()
    {
        var resource = _warehouseObjects.Pop();
        
        UpdateCounter();
        return resource;
    }

    private void StartWorkerStacking()
    {
        if (_workerStackingProcess != null)
        {
            return;
        }

        _workerStackingProcess = StartCoroutine(WorkerGettingResources());
    }

    private void StopWorkerStacking()
    {
        if (_workerStackingProcess == null)
        {
            return;
        }
        
        StopCoroutine(_workerStackingProcess);
        _workerStackingProcess = null;
    }
    
    private void StartStacking()
    {
        if (_stackCoroutine != null)
        {
            return;
        }

        if (_playerController == null)
        {
            return;
        }

        _stackCoroutine = StartCoroutine(getResourcesFromPlayer ? StackingRoutine() : GettingRoutine());
    }
    
    private void StopStacking()
    {
        if (_stackCoroutine == null)
        {
            return;
        }
        
        StopCoroutine(_stackCoroutine);
        _stackCoroutine = null;
    }

    private void StartStackingWorker(WorkerLogBehaviour worker)
    {
        if (_workerStackingProcess != null)
        {
            return;
        }

        if (worker == null)
        {
            return;
        }

        _workerStackingProcess = StartCoroutine(WorkerStackingRoutine(worker));
    }

    private void StopStackingWorker(WorkerLogBehaviour worker)
    {
        if (_workerStackingProcess == null)
        {
            return;
        }
        
        StopCoroutine(_workerStackingProcess);
        _workerStackingProcess = null;
    }
    
    private void StartClearingWood()
    {
        if (_clearRoutine != null)
        {
            return;
        }

        _clearRoutine = StartCoroutine(ClearWood());
    }

    private void StopClearingWood()
    {
        if (_clearRoutine == null)
        {
            return;
        }
        
        StopCoroutine(_clearRoutine);
        _clearRoutine = null;
    }
    
    private IEnumerator StackingRoutine()
    {
        foreach (var objects in objectsToEnter)
        {
            while (_playerController.ObjectsCountInHand() > 0)
            {
                //if (_warehouseObjects.Count - 1 >= maxCapacity)
                //{
                //    break;
                //}

                CollectableObject resource = null;
                resource = _playerController.GiveResource(objects);

                if (resource == null)
                {
                    break;
                }

                //PositionateResource(resource);
                PositionateResourceInPyramid(resource);

                yield return new WaitForSeconds(stackSpeed);

                Tutorial.Instance.MoveNext(Tutorial.TutorialPath.GoToTheWarehouse);
            }
        }

        yield return null;
    }
    
    private IEnumerator WorkerStackingRoutine(WorkerLogBehaviour worker)
    {
        foreach (var objects in objectsToEnter)
        {
            while (worker.ObjectsCountInHand() > 0)
            {
                //if (_warehouseObjects.Count - 1 >= maxCapacity)
                //{
                //    break;
                //}

                CollectableObject resource = null;
                resource = worker.GiveResource(objects);

                if (resource == null)
                {
                    break;
                }

                //PositionateResource(resource);
                PositionateResourceInPyramid(resource);

                yield return new WaitForSeconds(stackSpeed);
            }
        }

        yield return null;
    }
    
    private IEnumerator GettingRoutine()
    {
        while (_playerController.CanTake())
        {
            if (_warehouseObjects.Count == 0)
            {
                yield return new WaitForSeconds(stackSpeed);
                continue;
            }
            
            var resource = TakeResource();
            if (resource == null)
            {
                break;
            }
            
            _playerController.TakeResource(resource, resource.transform.localScale);

            yield return new WaitForSeconds(stackSpeed);
        }
        
        yield return null;
    }

    private IEnumerator ClearWood()
    {
        yield return new WaitForSeconds(1f);
        
        while (_warehouseObjects.Count > 0)
        {
            var resource = TakeResource();
            
            if (resource == null)
            {
                break;
            }

            foreach (var woodCleanerMachine in woodCleanerMachines)
            {
                if (woodCleanerMachine.TakeObjectForWork(resource))
                {
                    break;
                }
            }
            

            var delay = Mathf.Clamp(clearWoodTransferTime - UpgradeSaves.Instance.processingSpeedUpgrades * 0.3f, 0.1f,
                clearWoodTransferTime);
            
            yield return new WaitForSeconds(delay);
        }

        yield return null;
        StopClearingWood();
    }

    private IEnumerator WorkerGettingResources()
    {
        while (_workersInQueue.Count > 0)
        {
            var worker = _workersInQueue.Dequeue();

            while (worker.CanTake())
            {
                if (_warehouseObjects.Count == 0)
                {
                    yield return new WaitForSeconds(stackSpeed);
                    continue;
                }
            
                var resource = TakeResource();
                if (resource == null)
                {
                    yield return new WaitForSeconds(stackSpeed);
                    continue;
                }
            
                worker.TakeResource(resource, resource.transform.localScale);
            
                yield return new WaitForSeconds(stackSpeed);
            }
        }

        yield return null;
        StopWorkerStacking();
    }
    
    private void PositionateResource(CollectableObject resource)
    {
        var positionToMove = initPosition.position;

        int rowsUpCount = _warehouseObjects.Count / (dimension.x * dimension.y);

        int columnIndex = _warehouseObjects.Count / dimension.y;
        columnIndex -= rowsUpCount * dimension.x;

        int rowIndex = _warehouseObjects.Count - (columnIndex * dimension.y);
        rowIndex -= rowsUpCount * dimension.y * dimension.x;

        positionToMove.x += offset.x * columnIndex;
        positionToMove.z -= offset.z * rowIndex;
        positionToMove.y += offset.y * rowsUpCount;

        resource.transform.DORotate(new Vector3(0,-90,90), 0.5f);
        resource.SetGlobalPosition(positionToMove);

        resource.transform.SetParent(transform);
        _warehouseObjects.Push(resource);
        UpdateCounter();
    }

    private void PositionateResourceInPyramid(CollectableObject resource)
    {
        var positionToMove = initPosition.position;
        int pyramidHeight = dimension.x;
        int pyramidHeightOffset = dimension.y;

        int currentHeight = 0;
        int maxPlaceableItems = pyramidHeight * pyramidHeightOffset;
        int currentPlaceableItems = 0;
        for (int i = 0; i < pyramidHeight; i++)
        {
            currentPlaceableItems += maxPlaceableItems - (i * pyramidHeightOffset);

            if (i == 0)
            {
                currentPlaceableItems = maxPlaceableItems;
            }
            
            if (currentPlaceableItems >= _warehouseObjects.Count + 1)
            {
                currentHeight = i;
                break;
            }
        }

        positionToMove.x += offset.x / 2 * currentHeight;
        positionToMove.y += offset.y * currentHeight;
        
        int maxPlaceableItemsOnHeight = 0;
        for (int i = 0; i < currentHeight; i++)
        {
            maxPlaceableItemsOnHeight += maxPlaceableItems - (i * pyramidHeightOffset);
        }

        int resourcesCountOnRow = _warehouseObjects.Count - maxPlaceableItemsOnHeight;
        int columnIndex = resourcesCountOnRow / pyramidHeightOffset;
        int rowIndex = resourcesCountOnRow - (columnIndex * pyramidHeightOffset);
        
        positionToMove.x += offset.x * columnIndex;
        positionToMove.z -= offset.z * rowIndex;

        if (currentHeight == 0 && _warehouseObjects.Count > maxPlaceableItems)
        {
            positionToMove = _warehouseObjects.Peek().transform.position;
        }
        
        resource.transform.DORotate(new Vector3(0,-90,90), 0.5f);
        if (getResourcesFromPlayer == false)
        {
            resource.SetGlobalPosition(positionToMove,resourceStackSpeed);
        }
        else
        {
            resource.SetGlobalPosition(positionToMove);
        }

        resource.transform.SetParent(transform);
        _warehouseObjects.Push(resource);
        UpdateCounter();
    }
    
    private void UpdateCounter()
    {
        var text = _warehouseObjects.Count.ToString();
        objectsCountText.text = text;
        FactorySaves.Instance.ActualizeWarehouseData(this, _bought, _canBuy,0, _warehouseObjects.Count);
    }

    private void OnGenerationComplete(FabriqueMachine machine)
    {
        //PositionateResource(machine.GetResource());
        PositionateResourceInPyramid(machine.GetResource());
    }
    
    public virtual void OnBuy()
    {
        purchaserGameObject.BuyEvent -= OnBuy;
        Initialize(true, false);
        FactorySaves.Instance.ActualizeWarehouseData(this, true, false, 0, _warehouseObjects.Count);
        Progress.Instance.CheckProgress(true);
    }

    public void EnableToBuy()
    {
        if (_canBuy || _bought)
        {
            return;
        }
        
        Initialize(false, true);
        FactorySaves.Instance.ActualizeWarehouseData(this, false, true, 0, _warehouseObjects.Count);
    }

    private void OnBuyCanceled(int moneyAdded)
    {
        _addedMoney = moneyAdded;
        FactorySaves.Instance.ActualizeWarehouseData(this, false, true, _addedMoney, _warehouseObjects.Count);
    }
}
