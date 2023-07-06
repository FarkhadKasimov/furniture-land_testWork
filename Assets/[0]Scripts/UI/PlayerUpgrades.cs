using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUpgrades : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private int maximumUpgrades = 5;
    [SerializeField] private int updatePrice = 30;
    [SerializeField] private int nextUpgradePrice = 30;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button addsButton;
    [SerializeField] private bool upgradeable = false;

    private int _level = 0;
    private int _index = 0;
    private int _money = 0;

    private UpgradeSaves _upgradeSaves => UpgradeSaves.Instance;

    public void UpdatePrice(int upgradeIndex)
    {
        _index = upgradeIndex;
        
        switch (upgradeIndex)
        {
            case 0:
                UpdatePriceText(_upgradeSaves.processingSpeedUpgrades);
                UpdateLevelText(_upgradeSaves.processingSpeedUpgrades);
                _level = _upgradeSaves.processingSpeedUpgrades;
                break;
            case 1:
                UpdatePriceText(_upgradeSaves.playerChoppingSpeedUpgrades);
                UpdateLevelText(_upgradeSaves.playerChoppingSpeedUpgrades);
                _level = _upgradeSaves.playerChoppingSpeedUpgrades;
                break;
            case 2:
                UpdatePriceText(_upgradeSaves.playerCapacityUpgrades);
                UpdateLevelText(_upgradeSaves.playerCapacityUpgrades);
                _level = _upgradeSaves.playerCapacityUpgrades;
                break;
            case 3:
                UpdatePriceText(_upgradeSaves.playerSawScaleUpgrade);
                UpdateLevelText(_upgradeSaves.playerSawScaleUpgrade);
                _level = _upgradeSaves.playerSawScaleUpgrade;
                break;
            case 4:
                UpdatePriceText(_upgradeSaves.playerSawFuel);
                UpdateLevelText(_upgradeSaves.playerSawFuel);
                _level = _upgradeSaves.playerSawFuel;
                break;
            default:
                Debug.Log("We dont have that type of player upgrades!");
                return;
        }
    }
    
    public void BuyUpgrade(int upgradeIndex)
    {
        _index = upgradeIndex;
        var currentLevel = 0;
        var itemName = "";
        switch (upgradeIndex)
        {
            case 0:
                currentLevel = _upgradeSaves.processingSpeedUpgrades;
                itemName = "processing_speed_upgrade";
                break;
            case 1:
                currentLevel = _upgradeSaves.playerChoppingSpeedUpgrades;
                itemName = "saw_power_upgrade";
                break;
            case 2:
                currentLevel = _upgradeSaves.playerCapacityUpgrades;
                itemName = "inventory_capacity_upgrade";
                break;
            case 3:
                currentLevel = _upgradeSaves.playerSawScaleUpgrade;
                itemName = "saw_scale_upgrade";
                break;
            case 4:
                currentLevel = _upgradeSaves.playerSawFuel;
                itemName = "saw_fuel_upgrade";
                break;
            default:
                Debug.Log("We dont have that type of player upgrades!");
                return;
        }
        
        if (CanBuyUpgrade(currentLevel) == false)
        {
            return;
        }

        itemName += "_" + currentLevel;
        
        if (MoneyManager.Instance.SpentMoney(GetActualPrice(currentLevel), itemName, "player_upgrades") == false)
        {
            //UIManager.Instance.noMoneyPopUp.ShowPopUP();
            _money = GetActualPrice(currentLevel);

            ADSManager.Instance.RewardedAdFailEvent += ClearUnitEvents;
            ADSManager.Instance.RewardedAdViwedEvent += AddMoneyForUpgrade;
            
            ADSManager.Instance.ShowRewardedAd(ADSManager.UPGRADES_PLACEMENT);
            
            return;
        }

        var upgradeName = "";
        switch (upgradeIndex)
        {
            case 0:
                _upgradeSaves.processingSpeedUpgrades += 1;
                currentLevel = _upgradeSaves.processingSpeedUpgrades;
                upgradeName = "processing_speed";
                break;
            case 1:
                _upgradeSaves.playerChoppingSpeedUpgrades += 1;
                currentLevel = _upgradeSaves.playerChoppingSpeedUpgrades;
                upgradeName = "collecting_speed";
                break;
            case 2:
                _upgradeSaves.playerCapacityUpgrades += 1;
                currentLevel = _upgradeSaves.playerCapacityUpgrades;
                upgradeName = "inventory_capacity";
                break;
            case 3:
                _upgradeSaves.playerSawScaleUpgrade += 1;
                currentLevel = _upgradeSaves.playerSawScaleUpgrade;
                upgradeName = "saw_scale";
                break;
            case 4:
                _upgradeSaves.playerSawFuel += 1;
                currentLevel = _upgradeSaves.playerSawFuel;
                upgradeName = "saw_fuel";
                break;
            default:
                Debug.Log("We dont have that type of player upgrades!");
                upgradeName = "unnown";
                return;
        }

        AnalyticsManager.Instance.OnUpgradeBought(upgradeName, currentLevel);
        
        UpdateLevelText(currentLevel);
        UpdatePriceText(currentLevel);
        UpgradesUI.Instance.UpdateUpgradePrices();
    }

    private void AddMoneyForUpgrade()
    {
        MoneyManager.Instance.AddMoney(_money);
        DOVirtual.DelayedCall(0.1f, () =>
        {
            BuyUpgrade(_index);
        });
        
        ClearUnitEvents();
    }

    private void ClearUnitEvents()
    {
        ADSManager.Instance.RewardedAdFailEvent -= ClearUnitEvents;
        ADSManager.Instance.RewardedAdViwedEvent -= AddMoneyForUpgrade;
    }
    
    private void UpdateLevelText(int level)
    {
        levelText.text = (level * 10 + 100) + "%";
    }

    private void UpdatePriceText(int level)
    {
        bool getWithCash = CanBuyUpgrade(level) && MoneyManager.Instance.HasEnoughtMoney(GetActualPrice(level));
        buyButton.gameObject.SetActive(getWithCash);
        addsButton.gameObject.SetActive(getWithCash == false);
        _money = GetActualPrice(level);
        // buyButton.interactable = getWithCash;
        if (CanBuyUpgrade(level) == false)
        {
            priceText.text = "MAX";
            return;
        }
        
        priceText.text = GetActualPrice(level) + " <sprite=0>";
    }

    public bool EnougthMoney()
    {
        return MoneyManager.Instance.HasEnoughtMoney(GetActualPrice(_level)) && upgradeable;
    }
    
    private int GetActualPrice(int level)
    {
        return updatePrice + (nextUpgradePrice * level);
    }

    private bool CanBuyUpgrade(int currentLevel)
    {
        return currentLevel < maximumUpgrades;
    }
}
