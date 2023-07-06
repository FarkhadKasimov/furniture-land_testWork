using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CustomerGenerationFabrique))]
public class CustomerGenerator : MonoBehaviour
{
    [System.Serializable]
    public struct CustomerGenerationSettings
    {
        public int vitrinesCount;
        public int maxCustomers;
        public Vector2Int wantedProductsCountRange;

        public CustomerGenerationSettings(int _vitrinesCount = 0, int _maxCustomers = 0)
        {
            vitrinesCount = _vitrinesCount;
            maxCustomers = _maxCustomers;
            wantedProductsCountRange = Vector2Int.zero;
        }
    }

    [SerializeField] private CustomerGenerationSettings[] generationSettings;
    [SerializeField] private float spawnDelay = 1f;
    [Range(0, 100)] [SerializeField] private float wrappingChance = 5f;
    [SerializeField] private float minimumCountForWrapping = 3;
    [SerializeField] private int maxObjectsStacking = 2;

    private List<ObjectSettings> _availableResources = new List<ObjectSettings>();
    private CustomerGenerationFabrique _generationFabrique;
    private float _wrappingChance = 0f;

    private ManagersSpawner _managerSpawner => ManagersSpawner.Instance;

    private void Awake()
    {
        _generationFabrique = GetComponent<CustomerGenerationFabrique>();
    }

    private void Start()
    {
        StartCoroutine(SpawnProcess());
    }

    private IEnumerator SpawnProcess()
    {
        while (true)
        {
            //yield return new WaitForSeconds(spawnDelay / _managerSpawner.GetMultiplier(ManagerType.DoubleCustomers));
            yield return new WaitForSeconds(spawnDelay);
            foreach (var shopsData in FactorySaves.Instance.factoryShops)
            {
                GenerateCustomer(shopsData.shop);
            }
        }
    }
    
    private void GenerateCustomer(FactoryShop shop)
    {
        if (shop.cassaObject.IsBought() == false)
        {
            return;
        }
        
        var vitrines = FactorySaves.Instance.GetAvailableVitrines(shop);
        if (vitrines.Count == 0)
        {
            return;
        }

        _availableResources.Clear();
        foreach (var vitrine in vitrines)
        {
            _availableResources.Add(vitrine.GetObjectsSettings());
        }
        
        var settings = GetGenerationSettings(vitrines.Count);

        //if (_customersCount >= settings.maxCustomers  * _managerSpawner.GetMultiplier(ManagerType.DoubleCustomers))
        if (shop.WisitorsCount >= settings.maxCustomers)
        {
            return;
        }

        var spawnPoint = shop.GetRandomSpawnPoint();
        var customer = _generationFabrique.GenerateRandomCustomer(spawnPoint.position, 
                GenerateWantedProductList(settings, false), 0, shop);
        
        customer.CustomerRemoveEvent += OnCustomerRemove;
        shop.WisitorsCount += 1;
    }

    private CustomerGenerationSettings GetGenerationSettings(int magazinesCount)
    {
        CustomerGenerationSettings settings = new CustomerGenerationSettings();
        foreach (var generationSetting in generationSettings)
        {
            if (generationSetting.vitrinesCount <= magazinesCount)
            {
                settings = generationSetting;
            }
        }

        return settings;
    }

    private List<WantedResource> GenerateWantedProductList(CustomerGenerationSettings settings, bool wrapExists)
    {
        var resourcesCount = 0;
        var randomResourcesCount = 
            Random.Range(settings.wantedProductsCountRange.x, settings.wantedProductsCountRange.y + 1);

        List<WantedResource> result = new List<WantedResource>();

        while (resourcesCount < randomResourcesCount)
        {
            var randomResource = Random.Range(0, _availableResources.Count);
            var found = false;
            var item = _availableResources[randomResource];

            var candAddResource = true;
            foreach (var order in result)
            {
                if (order.type == item)
                {
                    if (order.count >= maxObjectsStacking)
                    {
                        candAddResource = false;
                    }
                    else
                    {
                        order.count += 1;
                    }
                    
                    found = true;
                    break;
                }
            }

            if (found == false)
            {
                result.Add(new WantedResource(item, 1));
            }

            if (candAddResource)
            {
                resourcesCount++;
            }
        }

        if (resourcesCount >= minimumCountForWrapping)
        {
            _wrappingChance = wrappingChance/100f;
        }
        else
        {
            _wrappingChance = 0;
        }

        if (wrapExists == false)
        {
            _wrappingChance = 0;
        }
        
        return result;
    }

    private void OnCustomerRemove(Customer customer, FactoryShop shop)
    {
        shop.WisitorsCount -= 1;
        customer.CustomerRemoveEvent -= OnCustomerRemove;
    }
}
