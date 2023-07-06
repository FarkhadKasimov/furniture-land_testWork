using System.Collections.Generic;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeRow : MonoBehaviour
{
    [SerializeField] private Image uiUpgradeImage;
    [SerializeField] private GameObject upgradeGameObject;
    [SerializeField] private Image upgradePart;
    [SerializeField] private Color boughtUpgradeColor;
    [SerializeField] private Transform upgradesContent;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button hireButton;
    [SerializeField] private Button rewardedButton;
    [SerializeField] private TextMeshProUGUI upgradePriceText;
    [SerializeField] private TextMeshProUGUI hirePriceText;
    [SerializeField] private TextMeshProUGUI workerLable;
    [SerializeField] private TextMeshProUGUI upgradesLable;
    //[SerializeField] private ScaleAnimation hireButtonAnimation;

    private UpgradesUI.UpgradesData _data;
    private int upgradesCount = 5;
    private List<Image> _upgradeParts = new List<Image>();
    private MoneyManager _moneyManager => MoneyManager.Instance;

    public void Initialize(UpgradesUI.UpgradesData data)
    {
        _data = data;
        RefreshUIData();
        SetupListeners();
    }

    private void SetupListeners()
    {
        upgradeButton.onClick.RemoveAllListeners();
        hireButton.onClick.RemoveAllListeners();
        rewardedButton.onClick.RemoveAllListeners();
        
        upgradeButton.onClick.AddListener(() => BuyUpgrade());
        hireButton.onClick.AddListener(() => HireWorker());
        rewardedButton.onClick.AddListener(BuySomethingFromAdd);
    }
    
    private void RefreshUIData()
    {
        var isBought = _data.workerBehaviour.IsBought();

        uiUpgradeImage.sprite = _data.workerData.uiUpgradeImage;
        upgradeGameObject.SetActive(isBought);
        //workerLable.text = _data.workerData.name;

        //Translate and set workerLable text
        workerLable.text = Lean.Localization.LeanLocalization.GetTranslationText(_data.workerData.name);

        if (isBought)
        {
            GenerateUpgrades();
            UpdateUpgradesCount();
        }

        upgradeButton.gameObject.SetActive(isBought);
        hireButton.gameObject.SetActive(!isBought);
        //hireButtonAnimation.Animate = !isBought;

        var data = _data.workerData;
        var upgradePrice = data.UpgradePrice + (data.NextUpgradeCostOffset * data.CurrentUpgrades);

        ShowAddButton(isBought ? upgradePrice : data.HirePrice);
        
        upgradePriceText.text = data.CurrentUpgrades >= data.MaximumUpgrades ? Lean.Localization.LeanLocalization.GetTranslationText("MAX") : upgradePrice + "\n<sprite=0>";
        hirePriceText.text = "<sprite=0>" + data.HirePrice;

        UpgradesUI.Instance.UpdateUpgradePrices();
    }

    private void ShowAddButton(float price)
    {
        if (_moneyManager.HasEnoughtMoney(price))
        {
            return;
        }
        
        upgradeButton.gameObject.SetActive(false);
        hireButton.gameObject.SetActive(false);
        rewardedButton.gameObject.SetActive(true);
    }

    public bool EnoughtMoney()
    {
        var data = _data.workerData;

        if (_data.workerBehaviour.IsBought())
        {
            var upgradePrice = data.UpgradePrice + (data.NextUpgradeCostOffset * data.CurrentUpgrades);
            return MoneyManager.Instance.HasEnoughtMoney(upgradePrice);
        }
        
        return MoneyManager.Instance.HasEnoughtMoney(data.HirePrice);
    }
    
    private void GenerateUpgrades()
    {
        _upgradeParts.Clear();
        upgradesContent.MMDestroyAllChildren();
        
        var data = _data.workerData;

        for (int i = 0; i < data.MaximumUpgrades; i++)
        {
            var upgradePartImage = Instantiate(upgradePart, upgradesContent);
            _upgradeParts.Add(upgradePartImage);
        }
    }

    private void UpdateUpgradesCount()
    {
        var data = _data.workerData;
        
        for (int i = 0; i < data.CurrentUpgrades; i++)
        {
            _upgradeParts[i].color = boughtUpgradeColor;
        }

        //upgradesLable.text = "Upgrades: "+(upgradesCount + (data.CurrentUpgrades * 4))+"/min.";

        //Translate and set upgradesLable text
        upgradesLable.text = Lean.Localization.LeanLocalization.GetTranslationText("Upgrades") + ": " + (upgradesCount + (data.CurrentUpgrades * 4)) + "/" + Lean.Localization.LeanLocalization.GetTranslationText("min") + ".";
    }

    private void BuyUpgrade(bool fromAd = false)
    {
        var data = _data.workerData;
        var upgradePrice = data.UpgradePrice + (data.NextUpgradeCostOffset * data.CurrentUpgrades);
        var itemName = "upgrade_" + data.name + "_" + (data.CurrentUpgrades + 1);

        if (data.CurrentUpgrades >= data.MaximumUpgrades)
        {
            return;
        }
        
        if (_moneyManager.SpentMoney(upgradePrice, itemName, "upgrade_workers") == false)
        {
            //UIManager.Instance.noMoneyPopUp.ShowPopUP();
            //return;
            if(fromAd == false)
                return;
        }
        
        data.Upgrade();
        UpdateUpgradesCount();
        UpgradeSaves.Instance.ActualizeUpgrades(_data.workerBehaviour, data, data.CurrentUpgrades, 
            _data.workerBehaviour.IsBought(),
            _data.workerBehaviour.CanBuy());
        
        upgradePrice += data.NextUpgradeCostOffset;
        upgradePriceText.text = data.CurrentUpgrades >= data.MaximumUpgrades ? "MAX" : upgradePrice + "\n<sprite=0>";
        
        AnalyticsManager.Instance.OnUpgradeBought("worker_" + _data.workerData.name, data.CurrentUpgrades);
        
        ShowAddButton(upgradePrice);
        
        VibrationController.Instance.PlayVibration("BuyNewItem_Vibration");
    }

    private void HireWorker(bool fromAd = false)
    {
        var itemName = "hire_" + _data.workerData.name;
        if (_moneyManager.SpentMoney(_data.workerData.HirePrice, itemName, "hire_workers") == false)
        {
            //UIManager.Instance.noMoneyPopUp.ShowPopUP();
            //return;
            
            if(fromAd == false)
                return;
        }
        
        _data.workerBehaviour.Buy();
        AnalyticsManager.Instance.OnUpgradeBought("worker_" + _data.workerData.name, 0);

        RefreshUIData();

        UpgradesUI.Instance.HideFinger();
        
        VibrationController.Instance.PlayVibration("BuyNewItem_Vibration");
    }

    private void BuySomethingFromAdd()
    {
        ADSManager.Instance.RewardedAdViwedEvent += OnAdWatched;
        ADSManager.Instance.RewardedAdFailEvent += RemoveAdEvents;
            
        ADSManager.Instance.ShowRewardedAd(ADSManager.UPGRADES_PLACEMENT);
    }

    private void OnAdWatched()
    {
        if (_data.workerBehaviour.IsBought())
        {
            BuyUpgrade(true);
        }
        else
        {
            HireWorker(true);
        }
        
        RefreshUIData();
        RemoveAdEvents();
    }

    private void RemoveAdEvents()
    {
        ADSManager.Instance.RewardedAdViwedEvent -= OnAdWatched;
        ADSManager.Instance.RewardedAdFailEvent -= RemoveAdEvents;
    }

    public void CheckButtonIteractebility()
    {
        var data = _data.workerData;
        var upgradePrice = data.UpgradePrice + (data.NextUpgradeCostOffset * data.CurrentUpgrades);

        if (hireButton.gameObject.activeInHierarchy)
        {
            hireButton.interactable = MoneyManager.Instance.HasEnoughtMoney(data.HirePrice);
        }
        else
        {
            upgradeButton.interactable = MoneyManager.Instance.HasEnoughtMoney(upgradePrice) && data.CurrentUpgrades >= data.MaximumUpgrades;
        }
    }
}
