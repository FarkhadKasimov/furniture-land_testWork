using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ObjectCreatorMachine : MagazineObjects
{
    public Transform workerExitTarget;
    [SerializeField] private ObjectSettings objectToView;
    [SerializeField] private CollectableObject generatedObjectPrefab;
    [SerializeField] private ZoneTrigger exitProductZoneTrigger;
    [SerializeField] private TextMeshPro currentCountText;
    [SerializeField] private float putObjectOnVitrineSpeed = 0.2f;
    [SerializeField] private float objectMakeDelay = 0.5f;
    [SerializeField] private Transform objectMakePosition;
    [SerializeField] private Transform[] objectPositions;
    [SerializeField] private bool teddyMachine = false;
    [SerializeField] private bool chocolateMachine = false;
    [SerializeField] private bool perfumeMachine = false;

    private List<CollectableObject> _inputObjects = new List<CollectableObject>();
    private List<CollectableObject> _generatedObjects = new List<CollectableObject>();
    
    [SerializeField] private WorkerBehaviour globalWorker = null;
    private Coroutine gettingResourcesProcess = null;
    private Coroutine gettingResourcesFromWorker = null;
    private bool processTarted = false;
    private Vector3 initialScale;
    private PlayerController _playerController = null;
    private WorkerBehaviour _workerBehaviour = null;
    private int currentCount = 0;

    public override void Initialize(bool bought, bool canBuy, int moneyAdded = 0, int currentObjectsCount = 0)
    {
        base.Initialize(bought, canBuy, moneyAdded, currentObjectsCount);
        IsTeddyMachine = teddyMachine && bought;
        IsChocolateMachine = chocolateMachine && bought;
        IsPerfumeMachine = perfumeMachine && bought;

        initialScale = transform.localScale;
        currentCount = 0;
        currentCountText.text = "<sprite=0>" + currentCount + "/" + objectsCount;

        StopCollectingResourcesFromPlayer();
        StopCollectingResourcesFromWorker();

        trigger.WorkerEnterEvent += OnWorkerEnter;
        trigger.WorkerExitEvent += OnWorkerExit;
        
        exitProductZoneTrigger.PlayerEnterEvent += OnPlayerEnterExitTrigger;
        exitProductZoneTrigger.PlayerExitEvent += OnPlayerExitExitTrigger;
        exitProductZoneTrigger.WorkerEnterEvent += OnWorkerEnterExitTrigger;
        exitProductZoneTrigger.WorkerExitEvent += OnWorkerExitExitTrigger;
    }

    private void Update()
    {
        if (_playerController == null && _workerBehaviour == null)
        {
            return;
        }
        
        if (_playerController != null && IsBought() && _playerController.CanTake())
        {
            GiveResource();
        }

        if (_workerBehaviour != null && IsBought() && _workerBehaviour.CanTake())
        {
            GiveResource();
        }
    }
    
    public override void OnDisableEvent()
    {
        trigger.WorkerEnterEvent -= OnWorkerEnter;
        trigger.WorkerExitEvent -= OnWorkerExit;
        
        exitProductZoneTrigger.PlayerEnterEvent -= OnPlayerEnterExitTrigger;
        exitProductZoneTrigger.PlayerExitEvent -= OnPlayerExitExitTrigger;
        exitProductZoneTrigger.WorkerEnterEvent -= OnWorkerEnterExitTrigger;
        exitProductZoneTrigger.WorkerExitEvent -= OnWorkerExitExitTrigger;
    }
    
    private void OnPlayerEnterExitTrigger(PlayerController controller)
    {
        _playerController = controller;
    }

    private void OnPlayerExitExitTrigger(PlayerController controller)
    {
        _playerController = null;
    }

    private void OnWorkerEnterExitTrigger(WorkerBehaviour workerBehaviour)
    {
        _workerBehaviour = workerBehaviour;
    }
    
    private void OnWorkerExitExitTrigger(WorkerBehaviour workerBehaviour)
    {
        _workerBehaviour = null;
    }
    
    public override void OnPlayerEnter(PlayerController controller)
    {
        GetResourcesFromPlayer(controller);
    }
    
    public override void OnPlayerExit(PlayerController controller)
    {
        StopCollectingResourcesFromPlayer();
    }
    
    private void OnWorkerEnter(WorkerBehaviour workerBehaviour)
    {
        if (globalWorker == null)
        {
            globalWorker = workerBehaviour;
        }
        
        if (workerBehaviour != globalWorker)
        {
            return;
        }
        
        StopCollectingResourcesFromPlayer();
        
        var resourcesCount = workerBehaviour.ObjectsCountInHand();
        gettingResourcesProcess = StartCoroutine(GetResourcesFromPlayerWithDelay(resourcesCount, null, workerBehaviour));
    } 
    
    private void OnWorkerExit(WorkerBehaviour workerBehaviour)
    {
        if (workerBehaviour != globalWorker)
        {
            return;
        }
        
        StopCollectingResourcesFromWorker();
    } 
    
    private void GetResourcesFromPlayer(PlayerController controller)
    {
        StopCollectingResourcesFromPlayer();
        
        var resourcesCount = controller.ObjectsCountInHand();
        gettingResourcesProcess = StartCoroutine(GetResourcesFromPlayerWithDelay(resourcesCount, controller, null));
    }

    private void StopCollectingResourcesFromPlayer()
    {
        if (gettingResourcesProcess == null)
        {
            return;
        }
        
        StopCoroutine(gettingResourcesProcess);
        gettingResourcesProcess = null;
    }

    private void StopCollectingResourcesFromWorker()
    {
        if (gettingResourcesFromWorker == null)
        {
            return;
        }
        
        StopCoroutine(gettingResourcesFromWorker);
        gettingResourcesFromWorker = null;
    }
    
    private IEnumerator GetResourcesFromPlayerWithDelay(int resourcesCount, PlayerController controller, WorkerBehaviour workerBehaviour)
    {
        for (int i = 0; i < resourcesCount; i++)
        {
            yield return new WaitForSeconds(putObjectOnVitrineSpeed);
            
            if (_inputObjects.Count >= objectsCount)
            {
                if (workerBehaviour != null)
                {
                    //workerBehaviour.VitrineIsFull();
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
            
            _inputObjects.Add(resource);
            currentCount++;
            resource.transform.SetParent(transform);
            resource.SetGlobalPosition(objectsToSpawnPositions[_inputObjects.Count - 1].position);
            resource.RotateToBaseValue();

            currentCountText.text = "<sprite=0>" + currentCount + "/" + objectsCount;
            //FactorySaves.Instance.ActualizeFabriqueData(this, true, false, 0, _inputObjects.Count);
        }

        if (processTarted == false)
        {
            MakeObject();
            ScaleAnimation();
        }

        yield return null;
    }

    private void MakeObject()
    {
        if (_inputObjects.Count == 0 || _generatedObjects.Count >= objectsCount)
        {
            processTarted = false;
            return;
        }

        processTarted = true;
        currentCount--;
        currentCountText.text = "<sprite=0>" + currentCount + "/" + objectsCount;

        var inputObject = _inputObjects[_inputObjects.Count - 1];
        inputObject.transform.DOMove(objectMakePosition.position, objectMakeDelay)
            .OnComplete(() => InstantiateObject(inputObject));
    }

    private void InstantiateObject(CollectableObject inputObject)
    {
        DOVirtual.DelayedCall(objectMakeDelay * 5f, () =>
        {
            _inputObjects.Remove(inputObject);

            var obj = Instantiate(generatedObjectPrefab, inputObject.transform.position, Quaternion.identity);
            obj.RotateToBaseValue();
            obj.transform.SetParent(transform);

            Destroy(inputObject.gameObject);
            
            obj.transform.DOMove(objectPositions[_generatedObjects.Count].position, objectMakeDelay)
                .OnComplete(() =>
                {
                    _generatedObjects.Add(obj);
                    MakeObject();
                });
        });
    }

    private void GiveResource()
    {
        if (_generatedObjects.Count == 0)
        {
            return;
        }

        var resource = _generatedObjects[_generatedObjects.Count - 1];
        
        if (_workerBehaviour != null)
        {
            _workerBehaviour.TakeResource(resource, resource.transform.localScale);
        }

        if (_playerController != null)
        {
            _playerController.TakeResource(resource, resource.transform.localScale);
            
            if (globalWorker != null)
            {
                globalWorker.FindNextTarget();
            }
        }

        _generatedObjects.Remove(resource);
        
        if (processTarted == false)
        {
            MakeObject();
            ScaleAnimation();
        }
        
        if (_inputObjects.Count < objectsCount && globalWorker != null)
        {
            //globalWorker.VitrineIsEmpty();
        }
    }

    private void ScaleAnimation()
    {
        if (processTarted == false)
        {
            return;
        }

        transform.DOScale(initialScale + (Vector3.one * 0.05f), objectMakeDelay)
            .OnComplete(() =>
            {
                transform.DOScale(initialScale, objectMakeDelay).OnComplete(ScaleAnimation);
            });
    }

    public override ObjectSettings GetObjectsSettings()
    {
        return objectToView;
    }
}
