using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

public class CassaObject : MagazineObjects
{
    [SerializeField] private Transform customerTarget;
    [SerializeField] private MoneyCollector moneyCollector; 
    [SerializeField] private bool wrapper = false;
    [SerializeField] private float customerServiceTime = 2f;

    [ShowIf(nameof(wrapper))] [SerializeField] private bool useBox = false;
    [HideIf(nameof(wrapper))][SerializeField] private Money moneyPrefab;
    [HideIf(nameof(wrapper))][SerializeField] private Transform moneyInitPosition;
    [HideIf(nameof(wrapper))][SerializeField] private Vector3 moneyDistanceOffset;
    [HideIf(nameof(wrapper))][SerializeField] private Vector2Int dimension = new Vector2Int(2, 2);
    [HideIf(nameof(wrapper))] [SerializeField] private string moneySavekey = "MONEY_QUEUE_COUNT";
    [SerializeField] private WorkerBehaviour globalWorker;
    [SerializeField] private FabriqueMachine machineEnable = null;
    [SerializeField] private Warehouse warehouseEnable = null;

    private bool _collectingStarted = false;
    
    private Queue<Customer> _customersQueue = new Queue<Customer>();
    private Stack<Money> _moneyQueue = new Stack<Money>();
    
    private bool _exitTrigger = true;
    private bool _casierOntherPosition = false;
    private int _rowsCount = 0;
    private int _currentMoneyCount = 0;
    private Coroutine _moneyCollectingCoroutine;
    private Transform _playerTransform;
    private float _fillAmount = 0f;
    private int _moneyCount = 0;
    
    private ManagersSpawner _managerSpawner => ManagersSpawner.Instance;

    public override void Initialize(bool bought, bool canBuy, int moneyAdded = 0, int currentObjectsCount = 0)
    {
        base.Initialize(bought, canBuy, moneyAdded, currentObjectsCount);
        IsCassa = bought;
        IsWrapper = wrapper;
        
        FactorySaves.Instance.FindMainObjects();
        
        moneyCollector.Init(this, null);

        trigger.WorkerEnterEvent += OnWorkerEnter;
        trigger.WorkerExitEvent += OnWorkerExit;

        if (wrapper==false && IsBought())
        {
            _moneyCount = ES3.Load(moneySavekey, 0);
            for (int i = 0; i < _moneyCount; i++)
            {
                SpawnMoney(null);
            }
        }
    }

    public override void OnDisableEvent()
    {
        trigger.WorkerEnterEvent -= OnWorkerEnter;
        trigger.WorkerExitEvent -= OnWorkerExit;
    }
    
    public override Vector3 GetCustomerTarget()
    {
        var position = customerTarget.position;

        position.x -= _customersQueue.Count * 1.5f;
        
        return position;
    }

    public override void OnBuy()
    {
        base.OnBuy();
        globalWorker.EnableObjectForBuy();
        if (warehouseEnable != null && warehouseEnable.IsBought == false && warehouseEnable.CanBuy == false)
        {
            warehouseEnable.EnableToBuy();
        }
    }

    public void AddCustomerToQueue(Customer customer)
    {
        _customersQueue.Enqueue(customer);

        var position = customerTarget.position;
        
        foreach (var customerQ in _customersQueue)
        {
            customerQ.transform.DOMoveX(position.x, 0.2f);
            customerQ.transform.DORotate(new Vector3(0, 40, 0), 0.2f);
            position.x -= 1.5f;
        }
        
        StartCoroutine(SellingProcess());
    }
    
    public override void OnPlayerEnter(PlayerController controller)
    {
        _exitTrigger = false;
        if (_casierOntherPosition == false)
        {
            customerServiceTime = 3f;
        }
        
        _playerTransform = controller.transform;
        StartCoroutine(SellingProcess());
    }
    
    public override void OnPlayerExit(PlayerController controller)
    {
        _exitTrigger = true;
    }

    private void OnWorkerEnter(WorkerBehaviour worker)
    {
        _casierOntherPosition = true;
        if (wrapper)
        {
            customerServiceTime -= worker.Data.CurrentUpgrades * (customerServiceTime / (worker.Data.MaximumUpgrades*2f));
        }
        else
        {
            customerServiceTime -= worker.Data.CurrentUpgrades * (customerServiceTime / (worker.Data.MaximumUpgrades*2f));
        }
        
        customerServiceTime = Mathf.Clamp(customerServiceTime, 0.1f, 100);
        
        StartCoroutine(SellingProcess());
    }

    private void OnWorkerExit(WorkerBehaviour worker)
    {
        _casierOntherPosition = false;
    }
    
    private IEnumerator SellingProcess()
    {
        bool playerInsideTrigger = !_exitTrigger;
        bool check = playerInsideTrigger || _casierOntherPosition;

        if (check == false || _customersQueue.Count == 0)
        {
            yield break;
        }

        _fillAmount = 1 / customerServiceTime * Time.deltaTime;

        if ((_exitTrigger == false || _casierOntherPosition) && _customersQueue.Count > 0)
        {
            List<Customer> customers = new List<Customer>();
            while (_customersQueue.Count > 0)
            {
                var customer = _customersQueue.Dequeue();
                customers.Add(customer);
            }

            foreach (var customer in customers)
            {
                customer.FillProgress(_fillAmount);
                GetMoneyFromCustomer(customer, customerServiceTime);
            }
            
            yield return new WaitForSeconds(customerServiceTime);
            
            foreach (var customer in customers)
            {
                customer.GoToExit();
                customer.StopFillProgress();
            }
            
            if (machineEnable != null && machineEnable.IsBought == false && machineEnable.CanBuy == false)
            {
                machineEnable.Initialize(false, true);
                Progress.Instance.CheckProgress();
            }
            
            yield return new WaitForSeconds(0.1f);
        }

        yield return null;
    }

    private void GetMoneyFromCustomer(Customer customer, float serviceTime)
    {
        var moneyCount = 0;
        foreach (var resource in customer.CustomerOrder)
        {
            moneyCount += resource.type.cost * resource.count;
        }
        
        Tutorial.Instance.MoveNext(Tutorial.TutorialPath.GoToCassa);

        var delay = serviceTime / moneyCount;
        StartCoroutine(MoneyCollectionFromCustomer(moneyCount, delay, customer));

        if (!_exitTrigger)
        {
            VibrationController.Instance.PlayVibration("PurchaseEnd_Vibration");
            //AudioManager.Instance.PlayBuySound("Buy_Sound");
        }
    }

    private IEnumerator MoneyCollectionFromCustomer(int count, float delay, Customer customer)
    {
        for (int i = 0; i < count; i++)
        {
            yield return new WaitForSeconds(delay);
            SpawnMoney(customer);
        }
    }
    
    private void SpawnMoney(Customer customer)
    {
        var money = Instantiate(moneyPrefab);
        money.Initialize(false, false);
        //money.SetMultiplier((int)_managerSpawner.GetMultiplier(ManagerType.DoubleMoney));
        
        if (customer != null)
        {
            money.transform.position = customer.transform.position;
        }
        var positionToMove = moneyInitPosition.position;

        int rowsUpCount = _moneyQueue.Count / (dimension.x * dimension.y);
        
        int columnIndex = _moneyQueue.Count / dimension.x;
        columnIndex -= rowsUpCount * dimension.x;
        
        int rowIndex = _moneyQueue.Count - (columnIndex * dimension.y);
        rowIndex -= rowsUpCount * dimension.y * dimension.x;
        
        positionToMove.x += moneyDistanceOffset.x * columnIndex;
        positionToMove.z -= moneyDistanceOffset.z * rowIndex;
        positionToMove.y += moneyDistanceOffset.y * rowsUpCount;

        if (customer != null)
        {
            money.transform.DOMove(positionToMove, 0.2f);
        }
        else
        {
            money.transform.position = positionToMove;
        }
        
        _moneyQueue.Push(money);
    }

    public void StartCollecting(Transform player)
    {
        if (_collectingStarted)
        {
            return;
        }

        if (_moneyQueue.Count == 0)
        {
            return;
        }
        
        _collectingStarted = true;
        _moneyCollectingCoroutine = StartCoroutine(CollectMoney(player));
    }

    public void StopCollecting()
    {
        _collectingStarted = false;
        if (_moneyCollectingCoroutine != null)
        {
            StopCoroutine(_moneyCollectingCoroutine);
            _moneyCollectingCoroutine = null;
        }
    }

    private IEnumerator CollectMoney(Transform player)
    {
        float timeToCollect = 1f / (_moneyQueue.Count / 4f);
        if (timeToCollect > 0.02f)
        {
            timeToCollect = 0.02f;
        }

        int moneyCount = 0;
        
        while (_moneyQueue.Count > 0)
        {
            var money = _moneyQueue.Pop();
            money.CollectMoney(player.position, timeToCollect);
            moneyCount++;
                
            if (moneyCount > 4) 
            {
                VibrationController.Instance.PlayVibration("MoneyCollect_Vibration");
                yield return new WaitForSeconds(timeToCollect);
                moneyCount = 0;
            }

            yield return null;
        }
        
        StopCollecting();
        yield return null;
    }

    private void OnApplicationQuit()
    {
        _moneyCount = _moneyQueue.Count;
        ES3.Save(moneySavekey ,_moneyCount);
    }
}
