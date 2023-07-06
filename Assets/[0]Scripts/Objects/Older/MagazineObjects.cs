using Sirenix.OdinInspector;
using UnityEngine;

public class MagazineObjects : MonoBehaviour
{
    [SerializeField] private PurchaserInfo purchaserGameObject = null;
    [SerializeField] private GameObject boughtGameObject = null;
    public ZoneTrigger trigger = null;

    public Transform workerTarget;
    public Transform[] objectsToSpawnPositions;
    public int price = 0;

    [OnValueChanged(nameof(ResetPositionsCollection))]
    public int objectsCount = 1;

    internal bool IsAvailableVitrine = false;
    internal bool IsCassa = false;
    internal bool IsWrapper = false;
    internal bool IsPomp = false;
    internal bool IsHoneyZone = false;
    internal bool IsExportMachine = false;
    internal bool IsWorker = false;
    internal bool IsTeddyMachine = false;
    internal bool IsChocolateMachine = false;
    internal bool IsPerfumeMachine = false;

    private bool _bought = false;
    private bool _canBuy = false;
    private int _addedMoney = 0;

    [Button]
    [GUIColor(0, 1, 0)]
    private void EnableObject()
    {
        Initialize(true, false, 0, 0);
        FactorySaves.Instance.ActualizeMagazinesData(this, true, false, 0 ,0);
        Progress.Instance.CheckProgress();
    }

    [Button]
    [GUIColor(0, 1, 1)]
    private void DeleteSaves(bool canBuy)
    {
        Initialize(false, canBuy, 0, 0);
        FactorySaves.Instance.ActualizeMagazinesData(this, false, canBuy, 0 ,0);
        Progress.Instance.CheckProgress();
        
        trigger.PlayerEnterEvent -= OnPlayerEnter;
        trigger.PlayerExitEvent -= OnPlayerExit;

        if (canBuy == false)
        {
            purchaserGameObject.BuyCancelationEvent -= OnBuyCanceled;
            purchaserGameObject.BuyEvent -= OnBuy;
        }
        
    }
    
    public void ShowToBuy()
    {
        if(_bought==false && _canBuy == false)
            Initialize(false, true, 0);
    }
    
    private void ResetPositionsCollection()
    {
        if (objectsToSpawnPositions.Length != objectsCount)
        {
            objectsToSpawnPositions = new Transform[objectsCount];
        }
    }
    
    public virtual void Initialize(bool bought, bool canBuy, int moneyAdded = 0, int currentObjectsCount = 0)
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
        
        if (_bought)
        {
            trigger.PlayerEnterEvent += OnPlayerEnter;
            trigger.PlayerExitEvent += OnPlayerExit;
        }
    }

    public void DisablePurchaser()
    {
        purchaserGameObject.gameObject.SetActive(false);
    }

    public virtual ObjectSettings GetObjectsSettings()
    {
        return null;
    }

    public virtual Vector3 GetCustomerTarget()
    {
        return Vector3.zero;
    }

    public virtual CollectableObject GiveResource()
    {
        return null;
    }
    
    private void OnDisable()
    {
        trigger.PlayerEnterEvent -= OnPlayerEnter;
        trigger.PlayerExitEvent -= OnPlayerExit;
        purchaserGameObject.BuyCancelationEvent -= OnBuyCanceled;

        OnDisableEvent();
    }

    public virtual void OnDisableEvent()
    {
        
    }
    
    public bool IsBought()
    {
        return _bought;
    }

    public bool CanBuy()
    {
        return _canBuy;
    }

    public virtual void OnPlayerEnter(PlayerController controller)
    {
        
    }

    public virtual void OnPlayerExit(PlayerController controller)
    {
       
    }

    public virtual void OnBuy()
    {
        purchaserGameObject.BuyEvent -= OnBuy;
        Initialize(true, false);
        FactorySaves.Instance.ActualizeMagazinesData(this, true, false, 0);
    }

    private void OnBuyCanceled(int moneyAdded)
    {
        _addedMoney = moneyAdded;
        FactorySaves.Instance.ActualizeMagazinesData(this, false, true, _addedMoney);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for (int i = 0; i < objectsCount; i++)
        {
            if (objectsToSpawnPositions[i] == null)
            {
                continue;
            }
            
            Gizmos.DrawSphere(objectsToSpawnPositions[i].position, 0.1f);
        }
    }
}
