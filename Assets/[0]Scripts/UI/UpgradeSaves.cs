using System.Collections.Generic;
using Project.Internal;
using UnityEngine;

public class UpgradeSaves : Singleton<UpgradeSaves>
{
    [System.Serializable]
    public class UpgradeSavesData
    {
        public WorkerBehaviour behaviour;
        public CharacterData data;
        public int currentUpgrade;
        public bool bought;
        public bool canBuy;
    }

    public int processingSpeedUpgrades = 0;
    public int playerChoppingSpeedUpgrades = 0;
    public int playerCapacityUpgrades = 0;
    public int playerSawScaleUpgrade = 0;
    public int playerSawFuel = 0;

    private const string UpgradesSavekey = "UPGRADES_SAVEKEY";
    public List<UpgradeSavesData> upgradeSaves = new List<UpgradeSavesData>();

    private void Awake()
    {
        if (ES3.KeyExists(UpgradesSavekey))
        {
            ES3.LoadInto(UpgradesSavekey, this);
        }
        else
        {
            ES3.Save(UpgradesSavekey, this);
        }
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            ES3.Save(UpgradesSavekey, this);
        }
    }

    private void OnApplicationQuit()
    {
        ES3.Save(UpgradesSavekey, this);
    }

    public void Init()
    {
        foreach (var saves in upgradeSaves)
        {
            saves.data.CurrentUpgrades = saves.currentUpgrade;
            saves.data.InitializeUpgrades();
            saves.behaviour.Initialize(saves.bought, saves.canBuy);
            
            UpgradesUI.Instance.AddNewUpgrade(saves.behaviour, saves.data);
        }
    }
    
    public void ActualizeUpgrades(WorkerBehaviour behaviour, CharacterData data, int currentUpgrades, bool bought, bool canBuy)
    {
        foreach (var saves in upgradeSaves)
        {
            if (saves.behaviour == behaviour)
            {
                saves.currentUpgrade = currentUpgrades;
                saves.canBuy = canBuy;
                saves.bought = bought;
            }
        }
    }
}
