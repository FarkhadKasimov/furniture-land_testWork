using System;
using System.Collections.Generic;
using Project.Internal;
using Sirenix.OdinInspector;
using UnityEngine;

public class FactorySaves : Singleton<FactorySaves>
{
    private const string FactorySaveKey = "Factory_SAVEKEY";
    [System.Serializable]
    public class FactoryObjectData
    {
        public FabriqueMachine factoryMachine;
        public bool bought;
        public bool canBuy;
        public int moneyAdded;
        public int currentObjectsCount;

        public FactoryObjectData()
        {
            factoryMachine = null;
            bought = false;
            canBuy = false;
        }
    }

    [System.Serializable]
    public class WarehouseData
    {
        public Warehouse warehouse;
        public bool bought;
        public bool canBuy;
        public int moneyAdded;
        public int currentObjectsCount;

        public WarehouseData()
        {
            warehouse = null;
            bought = false;
            canBuy = false;
        }
    }
    
    [System.Serializable]
    public class MagazinesData
    {
        public MagazineObjects magazine;
        public bool bought;
        public bool canBuy;
        public int moneyAdded;
        public int currentObjectsCount;

        public MagazinesData()
        {
            magazine = null;
            bought = false;
            canBuy = false;
        }
    }

    [System.Serializable]
    public class FactoryShopsData
    {
        public FactoryShop shop;
        public bool bought;
        public bool canBuy;
        public int moneyAdded;

        public FactoryShopsData()
        {
            shop = null;
            bought = false;
            canBuy = false;
        }
    }
    
    private bool _exitgame = false;
    
    [SerializeField] private FactoryObjectData[] factoryObjects;
    [SerializeField] private WarehouseData[] warehouseObjects;
    [SerializeField] private MagazinesData[] magazineObjects;

    public FactoryShopsData[] factoryShops;

    [Button]
    private void BuyAll()
    {
        foreach (var magazine in factoryShops)
        {
            if (magazine.bought == false)
            {
                magazine.shop.OnBuy();
            }
        }
        
        foreach (var magazine in factoryObjects)
        {
            if (magazine.bought == false)
            {
                magazine.factoryMachine.OnBuy();
            }
        }
        
        foreach (var magazine in warehouseObjects)
        {
            if (magazine.bought == false)
            {
                magazine.warehouse.OnBuy();
            }
        }
        
        foreach (var magazine in magazineObjects)
        {
            if (magazine.bought == false)
            {
                magazine.magazine.OnBuy();
            }
        }
    }
    
    private void Awake()
    {
        if (ES3.KeyExists(FactorySaveKey))
        {
            ES3.LoadInto(FactorySaveKey, this);
        }
        else
        {
            ES3.Save(FactorySaveKey, this);
        }
    }

    private void Start()
    {
        _exitgame = false;
        AnalyticsManager.Instance.OnGameStarted();

        foreach (var shopData in factoryShops)
        {
            var magazine = shopData.shop;
            magazine.Initialize(shopData.bought,shopData.canBuy, shopData.moneyAdded);
        }
        
        foreach (var warehouseData in warehouseObjects)
        {
            var warehouse = warehouseData.warehouse;
            warehouse.Initialize(warehouseData.bought, warehouseData.canBuy, warehouseData.moneyAdded, warehouseData.currentObjectsCount);
        }
        
        foreach (var magazineData in magazineObjects)
        {
            var magazine = magazineData.magazine;
            magazine.Initialize(magazineData.bought,magazineData.canBuy, magazineData.moneyAdded, magazineData.currentObjectsCount);
        }
        
        foreach (var factoryObjectData in factoryObjects)
        {
            var machine = factoryObjectData.factoryMachine;
            machine.Initialize(factoryObjectData.bought, factoryObjectData.canBuy, factoryObjectData.moneyAdded, factoryObjectData.currentObjectsCount);
        }

        UpgradeSaves.Instance.Init();
        
        FindMainObjects();
        Progress.Instance.Initialize();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            if (_exitgame == false)
            {
                _exitgame = true;
                //AnalyticsManager.Instance.OnGameFinished();
            }
            
            ES3.Save(FactorySaveKey, this);
        }
    }

    private void OnApplicationQuit()
    {
        if (_exitgame == false)
        {
            _exitgame = true;
            //AnalyticsManager.Instance.OnGameFinished();
        }
        
        ES3.Save(FactorySaveKey, this);

    }

    public void ActualizeFabriqueData(FabriqueMachine machine, bool bought, bool canBuy, int moneyAdded, int currentObjectsCount = 0)
    {
        FactoryObjectData factoryObjectData = null;
        foreach (var data in factoryObjects)
        {
            if (data.factoryMachine == machine)
            {
                factoryObjectData = data;
                break;
            }
        }

        if (factoryObjectData == null)
        {
            return;
        }

        factoryObjectData.bought = bought;
        factoryObjectData.canBuy = canBuy;
        factoryObjectData.moneyAdded = moneyAdded;
        factoryObjectData.currentObjectsCount = currentObjectsCount;
    }
    
    public void ActualizeWarehouseData(Warehouse machine, bool bought, bool canBuy, int moneyAdded, int currentObjectsCount = 0)
    {
        WarehouseData warehouseData = null;
        foreach (var data in warehouseObjects)
        {
            if (data.warehouse == machine)
            {
                warehouseData = data;
                break;
            }
        }

        if (warehouseData == null)
        {
            return;
        }

        warehouseData.bought = bought;
        warehouseData.canBuy = canBuy;
        warehouseData.moneyAdded = moneyAdded;
        warehouseData.currentObjectsCount = currentObjectsCount;
    }
    
    public void ActualizeMagazinesData(MagazineObjects magazine, bool bought, bool canBuy, int moneyAdded, int currentObjectsCount = 0)
    {
        MagazinesData magazineData = null;
        foreach (var data in magazineObjects)
        {
            if (data.magazine == magazine)
            {
                magazineData = data;
                break;
            }
        }

        if (magazineData == null)
        {
            return;
        }

        magazineData.bought = bought;
        magazineData.canBuy = canBuy;
        magazineData.moneyAdded = moneyAdded;
        magazineData.currentObjectsCount = currentObjectsCount;
    }
    
    public void ActualizeShopsData(FactoryShop magazine, bool bought, bool canBuy, int moneyAdded)
    {
        FactoryShopsData shopData = null;
        foreach (var data in factoryShops)
        {
            if (data.shop == magazine)
            {
                shopData = data;
                break;
            }
        }

        if (shopData == null)
        {
            return;
        }

        shopData.bought = bought;
        shopData.canBuy = canBuy;
        shopData.moneyAdded = moneyAdded;
    }

    public List<FabriqueMachine> GetAvailableVitrines()
    {
        List<FabriqueMachine> machines = new List<FabriqueMachine>();
        foreach (var machinesData in factoryObjects)
        {
            var machine = machinesData.factoryMachine;
            if (machine.IsAvailableVitrine)
            {
                machines.Add(machine);
            }
        }

        return machines;
    }

    public List<FabriqueMachine> GetAvailableVitrines(FactoryShop shop)
    {
        List<FabriqueMachine> machines = new List<FabriqueMachine>();
        foreach (var machinesData in factoryObjects)
        {
            var machine = machinesData.factoryMachine;
            if (shop.IsPartOfShop(machine) && machine.IsAvailableVitrine)
            {
                machines.Add(machine);
            }
        }

        return machines;
    }
    
    public Transform GetCustomerTargetByResource(ObjectSettings resource, FactoryShop shop)
    {
        var machines = GetAvailableVitrines(shop);
        foreach (var machine in machines)
        {
            if (machine.GetObjectsSettings() == resource)
            {
                return machine.GetCustomerTarget();
            }
        }

        return null;
    }
    
    public Transform GetWorkerTargetByResource(ObjectSettings resource, FactoryShop shop)
    {
        foreach (var factory in factoryObjects)
        {
            if (shop.IsPartOfShop(factory.factoryMachine) 
                && factory.bought 
                && factory.factoryMachine.isVitrine == false 
                && factory.factoryMachine.GetWorkerObjectSettings() == resource)
            {
                return factory.factoryMachine.GetWorkerTarget();
            }
        }

        return null;
    }

    public void FindMainObjects()
    {
        
    }
    
    public FabriqueMachine GetFabriqueByType(ObjectSettings resource, FactoryShop shop)
    {
        var magazines = GetAvailableVitrines(shop);
        foreach (var magaz in magazines)
        {
            if (magaz.GetObjectsSettings() == resource)
            {
                return magaz;
            }
        }

        return null;
    }
    
    public void EnableMagazineBuy(FabriqueMachine magazine)
    {
        FactoryObjectData factoryObjectData = new FactoryObjectData();
        foreach (var data in factoryObjects)
        {
            if (data.factoryMachine == magazine)
            {
                factoryObjectData = data;
            }
        }
        
        factoryObjectData.canBuy = true;
        factoryObjectData.factoryMachine.Initialize(factoryObjectData.bought, factoryObjectData.canBuy, factoryObjectData.moneyAdded, factoryObjectData.currentObjectsCount);
    }
}
