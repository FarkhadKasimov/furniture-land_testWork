using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ObjectGenerator : MagazineObjects
{
    [SerializeField] private CollectableObject objectToSpawn;
    [SerializeField] private float generationDelay = 1f;
    [SerializeField] private bool automaticGrowing = false;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private bool isHoneyZone = false;
    
    private CollectableObject[] _spawnedObjectsList;
    private PlayerController _playerController = null;
    private WorkerBehaviour _workerBehaviour = null;
    private Coroutine GrowRoutine;
    private int _currentResourcesCount = 0;
    private WorkerBehaviour GlobalWorker = null;

    private int _givedMoneyToBuy = 0;
    
    public override void Initialize(bool bought, bool canBuy, int moneyAdded = 0, int currentObjectsCount = 0)
    {
        base.Initialize(bought, canBuy, moneyAdded, currentObjectsCount);

        if (bought)
        {
            _spawnedObjectsList = new CollectableObject[objectsCount];

            if (automaticGrowing)
            {
                GrowRoutine = StartCoroutine(AutomaticGrowingWithDelay());
            }

            _currentResourcesCount = currentObjectsCount;
            
            if (countText != null)
            {
                countText.text = _currentResourcesCount + "/" + objectsCount;
            }

            IsHoneyZone = isHoneyZone;
        }
        else
        {
            IsHoneyZone = false;
        }

        if (canBuy && isHoneyZone)
        {
            //Tutorial.Instance.MoveNext(Tutorial.TutorialPath.StopArrow);
        }
        
        trigger.WorkerEnterEvent += OnWorkerEnter;
        trigger.WorkerExitEvent += OnWorkerExit;
    }
    
    public override void OnDisableEvent()
    {
        trigger.WorkerEnterEvent -= OnWorkerEnter;
        trigger.WorkerExitEvent -= OnWorkerExit;
    }

    private void OnWorkerEnter(WorkerBehaviour workerBehaviour)
    {
        _workerBehaviour = workerBehaviour;
    }

    private void OnWorkerExit(WorkerBehaviour workerBehaviour)
    {
        _workerBehaviour = null;
    }
    
    public void SetGlobalWorker(WorkerBehaviour globalWorker)
    {
        GlobalWorker = globalWorker;
    }

    public void SpawnResource()
    {
        
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
    
    private void SpawnObject(int index)
    {
        if (_spawnedObjectsList[index] != null)
        {
            return;
        }
        
        var resource = Instantiate(objectToSpawn, objectsToSpawnPositions[index].position, Quaternion.identity);
        var resourceTransform = resource.transform;
        var initialScale = resourceTransform.localScale;
        resourceTransform.localScale = Vector3.zero;
        resource.RotateToBaseValue();
        resourceTransform.DOScale(initialScale, 1f);
        resourceTransform.SetParent(transform);
        _spawnedObjectsList[index] = resource;
    }

    private void SpawnOnFirstEmpty()
    {
        for (int i = 0; i < objectsCount; i++)
        {
            if (_spawnedObjectsList[i] == null && automaticGrowing)
            {
                SpawnObject(i);
                break;
            }
        }
    }
    
    private IEnumerator AutomaticGrowingWithDelay()
    {
        while (true)
        {
            yield return new WaitForSeconds(generationDelay);
            if (_currentResourcesCount < objectsCount)
            {
                SpawnOnFirstEmpty();
                _currentResourcesCount++;
                if (countText != null)
                {
                    countText.text = _currentResourcesCount + "/" + objectsCount;
                }
            }

            yield return null;
        }
    }
    
    private void GiveResource()
    {
        CollectableObject resource = null;
        for (int i = 0; i < objectsCount; i++)
        {
            if (_spawnedObjectsList[i] != null)
            {
                resource = _spawnedObjectsList[i];
                _spawnedObjectsList[i] = null;
                _currentResourcesCount--;
                if (countText != null)
                {
                    countText.text = _currentResourcesCount + "/" + objectsCount;
                }
                break;
            }
        }

        if (resource == null)
        {
            return;
        }
        
        if (_workerBehaviour != null)
        {
            if (_workerBehaviour.CheckBeforeTakeResource(resource.Settings))
            {
                _workerBehaviour.TakeResource(resource, resource.transform.localScale);
            }
            
            return;
        }

        if (_playerController != null)
        {
            _playerController.TakeResource(resource, resource.transform.localScale);
            //Tutorial.Instance.MoveNext(Tutorial.TutorialPath.GoToFlower);
            
            if (GlobalWorker != null)
            {
                GlobalWorker.FindNextTarget();
            }
        }
    }

    public override void OnPlayerEnter(PlayerController controller)
    {
        _playerController = controller;
    }
    
    public override void OnPlayerExit(PlayerController controller)
    {
        _playerController = null;
    }

    public void GrowEmptyObjects()
    {
        if (automaticGrowing)
        {
            return;
        }
        
        if (IsBought() == false)
        {
            return;
        }

        for (int i = 0; i < objectsCount; i++)
        {
            if (_spawnedObjectsList[i] == null)
            {
                SpawnObject(i);
                _currentResourcesCount++;
            }
        }
    }

    public int GetGrowdResourcesCount()
    {
        int result = 0;
        for (int i = 0; i < objectsCount; i++)
        {
            if (_spawnedObjectsList[i] != null)
            {
                result++;
            }
        }

        return result;
    }

    public bool HaveGrownResources()
    {
        bool result = false;
        for (int i = 0; i < objectsCount; i++)
        {
            if (_spawnedObjectsList[i] != null)
            {
                result = true;
                break;
            }
        }

        return result;
    }
}
