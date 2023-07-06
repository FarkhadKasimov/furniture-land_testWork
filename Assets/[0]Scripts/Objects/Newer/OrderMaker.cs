using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OrderMaker : MonoBehaviour
{
    [SerializeField] private FabriqueMachine[] machinesOrder;
    [SerializeField] private ObjectSettings resourceToTakeFromWarehouse;
    [SerializeField] private ObjectSettings resourceToTakeFromWarehouse_2;
    [SerializeField] private int neededObjectsCount = 1;
    [SerializeField] private ZoneTrigger zoneTrigger;
    [SerializeField] private Image progressBar = null;
    [SerializeField] private float fillProgressSpeed = 1f;
    [SerializeField] private bool playVibration = false;
    [SerializeField] private bool collectFromPlayer = false;
    [SerializeField] private WorkerBehaviour globalWorker;
    
    [SerializeField] private Vector2Int dimension = Vector2Int.one;
    [SerializeField] private Transform initPosition;
    [SerializeField] private Transform initPosition_2;
    [SerializeField] private Vector3 gridOffset;
    [SerializeField] private TextMeshPro counterText;
    [SerializeField] private TextMeshPro counterText_2;
    
    private float _fillAmount = 0;
    private PlayerController _controller;
    private WorkerBehaviour _workerBehaviour;
    private Coroutine _collectRoutine;
    private Coroutine _collectFromWorker;
    private Stack<CollectableObject> _objects = new Stack<CollectableObject>();
    private Stack<CollectableObject> _objects_2 = new Stack<CollectableObject>();

    private void OnEnable()
    {
        Initialize();
    }

    private void OnDisable()
    {
        foreach (var fabrique in machinesOrder)
        {
            fabrique.GenerationCompleteEvent -= OnGenerationComplete;
        }
        
        zoneTrigger.PlayerEnterEvent -= OnPlayerEnter;
        zoneTrigger.PlayerExitEvent -= OnPlayerExit;
        
        zoneTrigger.WorkerEnterEvent -= OnWorkerEnter;
        zoneTrigger.WorkerExitEvent -= OnWorkerExit;
    }

    private void Update()
    {
        if (_objects.Count + _objects_2.Count < neededObjectsCount)
        {
            return;
        }

        if (resourceToTakeFromWarehouse_2 != null)
        {
            if ((_objects.Count < neededObjectsCount / 2) || (_objects_2.Count < neededObjectsCount / 2))
            {
                return;
            }
        }
        
        if (machinesOrder[machinesOrder.Length - 1].IsBought == false)
        {
            return;
        }
        
        if (machinesOrder[machinesOrder.Length - 1].HasAnySpace() == false)
        {
            return;
        }
        
        FillProgressBar();
    }

    public void Initialize()
    {
        foreach (var fabrique in machinesOrder)
        {
            fabrique.GenerationCompleteEvent += OnGenerationComplete;
        }

        zoneTrigger.PlayerEnterEvent += OnPlayerEnter;
        zoneTrigger.PlayerExitEvent += OnPlayerExit;

        zoneTrigger.WorkerEnterEvent += OnWorkerEnter;
        zoneTrigger.WorkerExitEvent += OnWorkerExit;

        RefreshProgress();
        UpdateCounter();
    }

    private void MakeOrder()
    {
        if (_objects.Count + _objects_2.Count < neededObjectsCount)
        {
            return;
        }

        if (resourceToTakeFromWarehouse_2 != null)
        {
            if ((_objects.Count < neededObjectsCount / 2) || (_objects_2.Count < neededObjectsCount / 2))
            {
                return;
            }
        }
        
        if (machinesOrder[machinesOrder.Length - 1].IsBought == false)
        {
            return;
        }

        if (machinesOrder[machinesOrder.Length - 1].HasAnySpace() == false)
        {
            return;
        }

        var fabrique = machinesOrder[0];

        if (resourceToTakeFromWarehouse_2 != null)
        {
            for (int i = 0; i < neededObjectsCount / 2; i++)
            {
                fabrique.TakeObjectForWork(_objects.Pop(), true);
                fabrique.TakeObjectForWork(_objects_2.Pop(), true);
                UpdateCounter();
            }
        }
        else
        {
            for (int i = 0; i < neededObjectsCount; i++)
            {
                fabrique.TakeObjectForWork(_objects.Pop(), true);
                UpdateCounter();
            }
        }
    }

    private void OnGenerationComplete(FabriqueMachine fabrique)
    {
        int index = 0;
        foreach (var fabriqueMachine in machinesOrder)
        {
            index += 1;
            
            if (fabriqueMachine == fabrique)
            {
                break;
            }
        }
        
        if (index > machinesOrder.Length - 1 || index == 0)
        {
            return;
        }
        
        var machine = machinesOrder[index];
        for (int i = 0; i < fabrique.ExitObjectsCount; i++)
        {
            var resource = fabrique.GetResource();
            
            if (machine.isVitrine)
            {
                machine.EnqueueObject(resource);
            }

            machine.TakeObjectForWork(resource);
        }
    }

    private void OnPlayerEnter(PlayerController controller)
    {
        _controller = controller;

        if (collectFromPlayer && _collectRoutine == null)
        {
            _collectRoutine = StartCoroutine(CollectResources());
        }
    }

    private void OnPlayerExit(PlayerController controller)
    {
        if (collectFromPlayer && _collectRoutine != null)
        {
            StopCoroutine(_collectRoutine);
            _collectRoutine = null;
        }
    }

    private void OnWorkerEnter(WorkerBehaviour workerBehaviour)
    {
        if (workerBehaviour != globalWorker)
        {
            return;
        }
        
        if (collectFromPlayer && _collectFromWorker == null)
        {
            _collectFromWorker = StartCoroutine(CollectResourcesFromWorker());
        }
    }
    
    private void OnWorkerExit(WorkerBehaviour workerBehaviour)
    {
        if (workerBehaviour != globalWorker)
        {
            return;
        }
        
        if (collectFromPlayer && _collectFromWorker != null)
        {
            StopCoroutine(_collectFromWorker);
            _collectFromWorker = null;
        }
    }
    
    private IEnumerator CollectResources()
    {
        var delay = 0.05f;

        while (_controller.ObjectsCountInHand() > 0)
        {
            yield return new WaitForSeconds(delay);
            var resource = _controller.GiveResource(resourceToTakeFromWarehouse);

            if (resource != null)
            {
                PositionateResources(resource,initPosition.position);
            }

            if (resourceToTakeFromWarehouse_2 != null)
            {
                var resource2 = _controller.GiveResource(resourceToTakeFromWarehouse_2);

                if (resource2 != null)
                {
                    PositionateResources(resource2, initPosition_2.position);
                }
            }
        }

        yield return null;
    }
    
    private IEnumerator CollectResourcesFromWorker()
    {
        var delay = 0.05f;

        while (globalWorker.ObjectsCountInHand() > 0)
        {
            yield return new WaitForSeconds(delay);
            var resource = globalWorker.GiveResource(resourceToTakeFromWarehouse);

            if (resource != null)
            {
                PositionateResources(resource,initPosition.position);
            }

            if (resourceToTakeFromWarehouse_2 != null)
            {
                var resource2 = globalWorker.GiveResource(resourceToTakeFromWarehouse_2);

                if (resource2 != null)
                {
                    PositionateResources(resource2, initPosition_2.position);
                }
            }
        }

        yield return null;
    }

    private void FillProgressBar()
    {
        progressBar.fillAmount += _fillAmount;
        if (playVibration)
        {
            VibrationController.Instance.PlayVibration("Watering_Vibration");
        }

        if (progressBar.fillAmount >= 1)
        {
            MakeOrder();
            RefreshProgress();
        }
    }
    
    private void RefreshProgress()
    {
        progressBar.fillAmount = 0;
        _fillAmount = 1f / fillProgressSpeed * Time.deltaTime;
    }

    private void PositionateResources(CollectableObject resource, Vector3 pos)
    {
        var stack = resource.Settings == resourceToTakeFromWarehouse ? _objects : _objects_2;
        int objectsCount = stack.Count;
        
        int rowsUpCount = objectsCount / (dimension.x * dimension.y);

        int columnIndex = objectsCount / dimension.y;
        columnIndex -= rowsUpCount * dimension.x;

        int rowIndex = objectsCount - (columnIndex * dimension.y);
        rowIndex -= rowsUpCount * dimension.y * dimension.x;
        
        pos.x += gridOffset.x * columnIndex;
        pos.z -= gridOffset.z * rowIndex;
        pos.y += gridOffset.y * rowsUpCount;

        resource.transform.DORotate(new Vector3(0,-90,90), 0.5f);
        resource.transform.SetParent(transform);
        
        resource.SetGlobalPosition(pos);
        resource.SetScale(Vector3.one);

        stack.Push(resource);
        UpdateCounter();
    }

    private void UpdateCounter()
    {
        if (resourceToTakeFromWarehouse_2 != null)
        {
            var text = _objects.Count + "/" + (neededObjectsCount/2);
            counterText.text = text;
            
            text = _objects_2.Count + "/" + (neededObjectsCount/2);
            counterText_2.text = text;
        }
        else
        {
            var text = _objects.Count + "/" + neededObjectsCount;
            counterText.text = text;
        } 
    }

    private int ObjectsCount(ObjectSettings resource)
    {
        int count = 0;
        foreach (var res in _objects)
        {
            if (res.Settings == resource)
            {
                count += 1;
            }
        }

        return count;
    }
}
