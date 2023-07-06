using Sirenix.OdinInspector;
using UnityEngine;

public class FactoryShop : MonoBehaviour
{
    public string Name;
    public Transform TeleportPosition;
    public CassaObject cassaObject;
    public Warehouse warehouse;
    
    public bool IsBought => _bought;
    public int WisitorsCount = 0;
    
    [SerializeField] private GameObject boughtGameObject;
    [SerializeField] private PurchaserInfo purchaserGameObject;
    [SerializeField] private int price;
    [SerializeField] private Transform[] customerSpawnPoints;

    private FabriqueMachine[] _shopMachines;
    private bool _bought = false;
    private bool _canBuy = false;
    private int _addedMoney = 0;
    
    public void ShowToBuy()
    {
        if (_bought == false && _canBuy == false)
        {
            Initialize(false, true, 0);
            FactorySaves.Instance.ActualizeShopsData(this, false, true, 0);
        }
            
    }
    
    public void Initialize(bool bought, bool canBuy, int moneyAdded = 0)
    {
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

        var child = transform.GetChild(0);
        _shopMachines = child.GetComponentsInChildren<FabriqueMachine>();
        var workers = child.GetComponentsInChildren<WorkerBehaviour>();
        foreach (var worker in workers)
        {
            worker.shop = this;
        }
    }

    public void OnCustomerSpawn()
    {
        
    }
    
    
    
    public Transform GetRandomSpawnPoint()
    {
        var index = Random.Range(0, customerSpawnPoints.Length);
        return customerSpawnPoints[index];
    }

    public bool IsPartOfShop(FabriqueMachine machine)
    {
        foreach (var shopMachine in _shopMachines)
        {
            if (machine == shopMachine)
            {
                return true;
            }
        }

        return false;
    }
    
    public virtual void OnBuy()
    {
        purchaserGameObject.BuyEvent -= OnBuy;
        Initialize(true, false);
        FactorySaves.Instance.ActualizeShopsData(this, true, false, 0);
    }

    private void OnBuyCanceled(int moneyAdded)
    {
        _addedMoney = moneyAdded;
        FactorySaves.Instance.ActualizeShopsData(this, false, true, _addedMoney);
    }
}
