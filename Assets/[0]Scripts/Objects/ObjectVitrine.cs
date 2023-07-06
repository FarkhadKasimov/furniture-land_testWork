using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class ObjectVitrine : MagazineObjects
{
    [SerializeField] private ObjectSettings objectToView;
    [SerializeField] private CollectableObject objectToGenerate;
    [SerializeField] private TextMeshPro currentCountText;
    [SerializeField] private Transform[] customerTargets;
    [SerializeField] private float putObjectOnVitrineSpeed = 0.1f;

    [SerializeField] WorkerBehaviour globalWorker = null;
    private bool workerEnterTrigger = false;
    private Coroutine gettingResourcesProcess;
    private Coroutine gettingResourcesFromWorker;
    private int _currentCount = 0;
    private List<CollectableObject> _vitrineObjects = new List<CollectableObject>();

    [Button]
    private void FillVitrine(int flowersCount)
    {
        flowersCount = Mathf.Clamp(flowersCount, 0, objectsCount);
        
        ClearVitrine();

        for (int i = 0; i < flowersCount; i++)
        {
            GenerateTestVitrineObject();
        }
    }

    public override void Initialize(bool bought, bool canBuy, int moneyAdded = 0, int currentObjectsCount = 0)
    {
        base.Initialize(bought, canBuy, moneyAdded, currentObjectsCount);
        _currentCount = currentObjectsCount;
        IsAvailableVitrine = bought;
        
        currentCountText.text = "<sprite=0>" + _currentCount + "/" + objectsCount;
        
        if(_vitrineObjects.Count == 0)
            SpawnInitialResources();

        if (bought == false && _vitrineObjects.Count > 0)
        {
            foreach (var flower in _vitrineObjects)
            {
                Destroy(flower.gameObject);
            }
            
            _vitrineObjects.Clear();
        }

        if (gettingResourcesProcess != null)
        {
            StopCoroutine(gettingResourcesProcess);
            gettingResourcesProcess = null;
        }
        
        trigger.WorkerEnterEvent += OnWorkerEnter;
        trigger.WorkerExitEvent += OnWorkerExit;
    }
    
    public override void OnDisableEvent()
    {
        trigger.WorkerEnterEvent -= OnWorkerEnter;
        trigger.WorkerExitEvent -= OnWorkerExit;
    }

    private void ClearVitrine()
    {
        foreach (var obj in _vitrineObjects)
        {
            Destroy(obj.gameObject);
        }
        
        _vitrineObjects.Clear();

        _currentCount = 0;
        //FactorySaves.Instance.ActualizeFabriqueData(this, true, false, 0, _currentCount);
    }

    private void GenerateTestVitrineObject()
    {
        var obj = Instantiate(objectToGenerate);
        _vitrineObjects.Add(obj);
        obj.transform.SetParent(transform);
        obj.SetGlobalPosition(objectsToSpawnPositions[_vitrineObjects.Count - 1].position);
        obj.RotateToBaseValue();

        _currentCount++;
        currentCountText.text = "<sprite=0>" + _currentCount + "/" + objectsCount;
        //FactorySaves.Instance.ActualizeFabriqueData(this, true, false, 0, _currentCount);
    }
    
    public override void OnPlayerEnter(PlayerController controller)
    {
        GetResourcesFromPlayer(controller);
    }
    
    public override void OnPlayerExit(PlayerController controller)
    {
        if (gettingResourcesProcess != null)
        {
            StopCoroutine(gettingResourcesProcess);
            gettingResourcesProcess = null;
        }
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

        workerEnterTrigger = true;
        
        GetResourcesFromWorker(workerBehaviour);
    } 
    
    private void OnWorkerExit(WorkerBehaviour workerBehaviour)
    {
        if (workerBehaviour != globalWorker)
        {
            return;
        }
        
        if (gettingResourcesFromWorker != null)
        {
            StopCoroutine(gettingResourcesFromWorker);
            gettingResourcesFromWorker = null;
        }

        workerEnterTrigger = false;
    } 
    
    private void GetResourcesFromPlayer(PlayerController controller)
    {
        if (gettingResourcesProcess != null)
        {
            StopCoroutine(gettingResourcesProcess);
            gettingResourcesProcess = null;
        }
        
        var resourcesCount = controller.ObjectsCountInHand();
        gettingResourcesProcess = StartCoroutine(GetResourcesFromPlayerWithDelay(resourcesCount, controller, null));
    }
    
    private void GetResourcesFromWorker(WorkerBehaviour workerBehaviour)
    {
        if (gettingResourcesFromWorker != null)
        {
            StopCoroutine(gettingResourcesFromWorker);
            gettingResourcesFromWorker = null;
        }
        
        var resourcesCount = workerBehaviour.ObjectsCountInHand();
        gettingResourcesFromWorker = StartCoroutine(GetResourcesFromPlayerWithDelay(resourcesCount, null, workerBehaviour));
    }

    private IEnumerator GetResourcesFromPlayerWithDelay(int resourcesCount, PlayerController controller, WorkerBehaviour workerBehaviour)
    {
        for (int i = 0; i < resourcesCount; i++)
        {
            yield return new WaitForSeconds(putObjectOnVitrineSpeed);
            
            if (_vitrineObjects.Count >= objectsCount)
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
            
            _vitrineObjects.Add(resource);
            resource.transform.SetParent(transform);
            resource.SetGlobalPosition(objectsToSpawnPositions[_vitrineObjects.Count - 1].position);
            resource.RotateToBaseValue();

            _currentCount++;
            currentCountText.text = "<sprite=0>" + _currentCount + "/" + objectsCount;
            //FactorySaves.Instance.ActualizeFabriqueData(this, true, false, 0, _currentCount);
            
            //Tutorial.Instance.MoveNext(Tutorial.TutorialPath.GoToVitrine);
        }

        yield return null;
    }
    
    private void SpawnInitialResources()
    {
        _vitrineObjects.Clear();
        for (int i = 0; i < _currentCount; i++)
        {
            var obj = Instantiate(objectToGenerate, objectsToSpawnPositions[i].position, Quaternion.identity);
            _vitrineObjects.Add(obj);
            obj.transform.SetParent(transform);
            obj.RotateToBaseValue();
        }
    }
    
    public override ObjectSettings GetObjectsSettings()
    {
        return objectToView;
    }
    
    public override Vector3 GetCustomerTarget()
    {
        var randomIndex = Random.Range(0, customerTargets.Length);
        return customerTargets[randomIndex].position;
    }
    
    public override CollectableObject GiveResource()
    {
        if (_vitrineObjects.Count == 0)
        {
            return null;
        }

        var resource = _vitrineObjects[_vitrineObjects.Count - 1];
        _vitrineObjects.Remove(resource);
        
        _currentCount--;
        currentCountText.text = "<sprite=0>" + _currentCount + "/" + objectsCount;
        //FactorySaves.Instance.ActualizeFabriqueData(this, true, false, 0, _currentCount);

        if (_vitrineObjects.Count < objectsCount && globalWorker != null)
        {
            if (workerEnterTrigger)
            {
                GetResourcesFromWorker(globalWorker);
            }
            else
            {
                //globalWorker.VitrineIsEmpty();
            }
        }
        
        return resource;
    }
}
