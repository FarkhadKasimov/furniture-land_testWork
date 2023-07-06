using System;
using Project.Internal;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

public class ManagersSpawner : Singleton<ManagersSpawner>
{
    [System.Serializable]
    public class ManagerGenerationData
    {
        public ManagerType type;
        public float boostMultiplier;
    }
    
    public bool CanSpawnManagers = true;
    
    [SerializeField] private float boostTime = 60;
    [SerializeField] private float managerSpawnDelay = 180;
    [SerializeField] private float managerWaitingDelay = 180;
    [SerializeField] private int boosterPrice = 150;
    [SerializeField] private Manager managerPrefab;
    [SerializeField] private ManagerGenerationData[] managersToSpawn;
    [SerializeField] private Transform managerInstantiatePosition;
    [SerializeField] private Transform managerTargetPosition;
    [SerializeField] private Transform managerBoostMakePosition;

    private bool _managerSpawned = false;
    private bool _boostStarted = false;
    private bool _uiShown = false;
    private float _currentTime = 0;
    private Manager _spawnedManager = null;

    private void Update()
    {
        if (CanSpawnManagers == false)
        {
            return;
        }

        if (_managerSpawned == false)
        {
            CheckTimeToSpawnManager();
        }

        if (_managerSpawned && _uiShown == false && _boostStarted == false)
        {
            CheckToDespawnManager();
        }
        
        if (_boostStarted)
        {
            CheckTimeToDisableBoost();
        }
    }

    public float GetMultiplier(ManagerType managerType)
    {
        return GetMultiplierByType(managerType);
    }

    public void EnableBoost()
    {
        SetBoostState(true);
        CloseUI();
        _spawnedManager.GoToMakeBoostState();
        _spawnedManager.PressedEvent -= OnManagerPressed;
    }

    private void CheckTimeToSpawnManager()
    {
        IncreaseTimer();
        
        if (_currentTime >= managerSpawnDelay)
        {
            SpawnManager();
        }
    }

    private void CheckTimeToDisableBoost()
    {
        IncreaseTimer();
        
        if (_currentTime >= boostTime)
        {
            DisableBoost();
        }
    }

    private void CheckToDespawnManager()
    {
        IncreaseTimer();
        
        if (_currentTime >= managerWaitingDelay)
        {
            DisableBoost();
        }
    }
    
    private void SpawnManager()
    {
        var type = managersToSpawn[Random.Range(0, managersToSpawn.Length)].type;
        _spawnedManager = Instantiate(managerPrefab, managerInstantiatePosition.position, Quaternion.identity);
        _spawnedManager.Initialize(type, managerTargetPosition, managerBoostMakePosition,managerInstantiatePosition);

        _spawnedManager.DespawnEvent += OnManagerDespawn;
        _spawnedManager.PressedEvent += OnManagerPressed;
        
        SetManagerSpawnState(true);
    }

    private void DisableBoost()
    {
        SetBoostState(false);
        _spawnedManager.GoToDespawn();
    }

    private void IncreaseTimer()
    {
        _currentTime += Time.deltaTime;
    }

    private void SetManagerSpawnState(bool state)
    {
        _currentTime = 0;
        _managerSpawned = state;
    }

    private void SetBoostState(bool state)
    {
        _currentTime = 0;
        _boostStarted = state;
    }

    private float GetMultiplierByType(ManagerType type)
    {
        if (_boostStarted == false)
        {
            return 1f;
        }
        
        if (type != _spawnedManager.Type)
        {
            return 1f;
        }

        foreach (var data in managersToSpawn)
        {
            if (data.type == type)
            {
                return data.boostMultiplier;
            }
        }

        return 1f;
    }

    private void ShowUI()
    {
        _uiShown = true;
    }

    public void CloseUI()
    {
        _uiShown = false;
    }
    
    private void OnManagerDespawn()
    {
        _spawnedManager.DespawnEvent -= OnManagerDespawn;
        SetManagerSpawnState(false);
        
        _spawnedManager = null;
    }

    private void OnManagerPressed()
    {
        if (_uiShown)
        {
            return;
        }
        
        ShowUI();
    }
}
