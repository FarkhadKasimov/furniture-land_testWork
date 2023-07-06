using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ExportMachineObject : MagazineObjects
{
    [SerializeField] private ObjectSettings objectToView;
    [SerializeField] private CollectableObject objectToGenerate;
    [SerializeField] private Money moneyObject;
    [SerializeField] private float exportProcessDuration = 20f;
    [SerializeField] private Transform positionToMove;
    [SerializeField] private Transform objectToMove;
    [SerializeField] private MoneyCollector moneyCollector;
    [SerializeField] private Transform moneyInitPosition;
    [SerializeField] private Vector3 moneyDistanceOffset;
    [SerializeField] private Vector2Int dimension = new Vector2Int(2, 2);
    
    private List<CollectableObject> _vitrineObjects = new List<CollectableObject>();
    private List<Money> _moneyList = new List<Money>();
    private int _currentCount = 0;
    private bool _exportProcessIsRunned = false;
    private Vector3 _initialPosition;
    private bool collectingStarted = false;
    private Coroutine moneyCollectingCoroutine;
    private Coroutine workerCheckCoroutine;
    private List<Money> moneyQueue = new List<Money>();
    
    public override void Initialize(bool bought, bool canBuy, int moneyAdded = 0, int currentObjectsCount = 0)
    {
        base.Initialize(bought, canBuy, moneyAdded, currentObjectsCount);

        _currentCount = currentObjectsCount;
        _initialPosition = objectToMove.position;

        IsExportMachine = bought;
        
        if(_vitrineObjects.Count == 0)
            SpawnInitialResources();
        
        if (_currentCount >= objectsCount)
        {
            StartCoroutine(RunExportProcess());
        }
        
        moneyCollector.Init(null, this);

        trigger.WorkerEnterEvent += OnWorkerEnter;
        trigger.WorkerExitEvent += OnWorkerExit;
    }

    public override void OnDisableEvent()
    {
        trigger.WorkerEnterEvent -= OnWorkerEnter;
        trigger.WorkerExitEvent -= OnWorkerExit;
    }
    
    public override void OnPlayerEnter(PlayerController controller)
    {
        if (_exportProcessIsRunned)
        {
            return;
        }
        
        GetResourcesFromPlayer(controller, null);
    }

    private void OnWorkerEnter(WorkerBehaviour workerBehaviour)
    {
        if (_exportProcessIsRunned)
        {
            workerCheckCoroutine = StartCoroutine(WorkerCheck(workerBehaviour));
            return;
        }
        
        GetResourcesFromPlayer(null, workerBehaviour);
    }

    private void OnWorkerExit(WorkerBehaviour workerBehaviour)
    {
        if (workerCheckCoroutine != null)
        {
            StopCoroutine(workerCheckCoroutine);
        } 
    }
    
    private void GetResourcesFromPlayer(PlayerController controller, WorkerBehaviour workerBehaviour)
    {
        int resourcesCount = 0;
        
        if (controller != null)
        {
            resourcesCount = controller.ObjectsCountInHand();
        }
        else if (workerBehaviour != null)
        {
            resourcesCount = workerBehaviour.ObjectsCountInHand();
        }
        
        for (int i = 0; i < resourcesCount; i++)
        {
            if (_vitrineObjects.Count >= objectsCount)
            {
                if (workerBehaviour != null)
                {
                    //workerBehaviour.FindNextTargetFiller();
                }
                break;
            }

            CollectableObject resource = null;
            if (controller != null)
            {
                resource = controller.GiveResource(objectToView);
            }
            else if (workerBehaviour != null)
            {
                resource = workerBehaviour.GiveResource(objectToView);
            }
            
            if (resource == null)
            {
                break;
            }
            
            _vitrineObjects.Add(resource);
            resource.transform.SetParent(objectToMove);
            resource.SetGlobalPosition(objectsToSpawnPositions[_vitrineObjects.Count - 1].position);
            resource.RotateToBaseValue();

            _currentCount++;
            //FactorySaves.Instance.ActualizeFabriqueData(this, true, false, 0, _currentCount);
        }

        if (_currentCount >= objectsCount)
        {
            StartCoroutine(RunExportProcess());
        }
        
    }
    
    private void SpawnInitialResources()
    {
        _vitrineObjects.Clear();
        for (int i = 0; i < _currentCount; i++)
        {
            var obj = Instantiate(objectToGenerate, objectsToSpawnPositions[i].position, Quaternion.identity);
            _vitrineObjects.Add(obj);
            obj.transform.SetParent(objectToMove);
            obj.RotateToBaseValue();
        }
    }

    private IEnumerator RunExportProcess()
    {
        _exportProcessIsRunned = true;
        yield return new WaitForSeconds(0.5f);
        objectToMove.DOMove(positionToMove.position, 10f);
        yield return new WaitForSeconds(exportProcessDuration);
        SpawnMoney();
        objectToMove.DOMove(_initialPosition, 10f).OnComplete(()=>
        {
            SortMoney();
            _exportProcessIsRunned = false;
        });
    }

    private IEnumerator WorkerCheck(WorkerBehaviour workerBehaviour)
    {
        while (_exportProcessIsRunned)
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        GetResourcesFromPlayer(null, workerBehaviour);
    }
    
    private void SpawnMoney()
    {
        for (int i = 0; i < _vitrineObjects.Count; i++)
        {
            var honey = _vitrineObjects[i];
            _currentCount--;

            var money = Instantiate(moneyObject, honey.transform.position, Quaternion.identity);
            
            money.transform.SetParent(objectToMove);
            money.Initialize(false, false, objectToView.cost);
            _moneyList.Add(money);
            
            Destroy(honey.gameObject);
        }
        
        _vitrineObjects.Clear();
        //FactorySaves.Instance.ActualizeFabriqueData(this, true, false, 0, 0);
    }
    
    public void StartCollecting(Transform player)
    {
        if (_moneyList.Count == 0)
        {
            return;
        }
        
        collectingStarted = true;
        moneyCollectingCoroutine = StartCoroutine(CollectMoney(player));
    }
    
    public void StopCollecting()
    {
        collectingStarted = false;
        if (moneyCollectingCoroutine != null)
        {
            StopCoroutine(moneyCollectingCoroutine);
        }

        moneyCollectingCoroutine = null;
    }
    
    private IEnumerator CollectMoney(Transform player)
    {
        float timeToCollect = 2f / (_moneyList.Count / 4);
        if (timeToCollect > 0.05f)
        {
            timeToCollect = 0.05f;
        }

        int moneyCount2 = 0;
        
        while (collectingStarted)
        {
            if (_moneyList.Count > 0)
            {
                var money = _moneyList[_moneyList.Count - 1];
                money.CollectMoney(player);
                _moneyList.Remove(money);
                moneyQueue.Remove(money);
                
                moneyCount2++;
                
                if (moneyCount2 > 4)
                {
                    VibrationController.Instance.PlayVibration("MoneyCollect_Vibration");
                    yield return new WaitForSeconds(timeToCollect);
                    moneyCount2 = 0;
                }
            }

            yield return null;
        }
        
        yield return null;
    }
    
    private void SortMoney()
    {
        List<Money> moneyQueue = new List<Money>();

        var moneyCount = _moneyList.Count;
        var moneyBegin = moneyQueue.Count > 0 ? moneyQueue.Count - 1 : 0;
        
        for (int i = moneyBegin; i < moneyCount; i++)
        {
            var money = _moneyList[i];
            money.transform.SetParent(null);
            var positionToMove = moneyInitPosition.position;
            int rowsUpCount = moneyQueue.Count / (dimension.x * dimension.y);
        
            int columnIndex = moneyQueue.Count / dimension.x;
            columnIndex -= rowsUpCount * dimension.x;
        
            int rowIndex = moneyQueue.Count - (columnIndex * dimension.y);
            rowIndex -= rowsUpCount * dimension.y * dimension.x;
        
            positionToMove.x += moneyDistanceOffset.x * columnIndex;
            positionToMove.z -= moneyDistanceOffset.z * rowIndex;
            positionToMove.y += moneyDistanceOffset.y * rowsUpCount;

            money.transform.DOMove(positionToMove, 0.2f);
            money.transform.DORotate(new Vector3(0, 90, 0), 0.5f);

            moneyQueue.Add(money);
        }
    }
}
