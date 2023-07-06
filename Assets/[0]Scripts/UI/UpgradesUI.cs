using System;
using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Utils;
using Image = UnityEngine.UI.Image;

public class UpgradesUI : UIPanel
{
    [System.Serializable]
    public class UpgradesData
    {
        public WorkerBehaviour workerBehaviour;
        public CharacterData workerData;

        public UpgradesData(WorkerBehaviour worker, CharacterData data)
        {
            workerBehaviour = worker;
            workerData = data;
        }
    }

    public static UpgradesUI Instance = null;

    [SerializeField] private CanvasGroup workersPanel;
    [SerializeField] private CanvasGroup upgradesPanel;
    [SerializeField] private Image workerButton;
    [SerializeField] private Image upgradesButton;
    [SerializeField] private Color unselectedButtonColor;
    [SerializeField] private UpgradeRow workerUpgradeRow;
    [SerializeField] private Transform content;
    [SerializeField] private Floater finger;
    [SerializeField] private Floater upgradeFinger;
    [SerializeField] private ColorAnimation mainButtonAnimation;
    [SerializeField] private ScrollRect upgradesScroll;
    [SerializeField] private PlayerUpgrades[] playerUpgrades;
    [SerializeField] private GameObject upgradeSign;

    private List<UpgradeRow> workerUpgradesRows = new List<UpgradeRow>();
    private bool _workersWindowOpened = false;
    private bool _windowOpened = false;
    private Color _originalButtonColor = Color.white;
    private bool _upgradeTutorialCompleted = false;
    private List<UpgradesData> _upgrades = new List<UpgradesData>();
    private int activeUpgradesCount = 0;
    private int lastPompIndex = 0;

    private float timer = 0f;
    
    private const string upgradeTutorialFinger = "UPGRADETUTORIALFINGER_SAVEKEY";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance == this)
        {
            Destroy(Instance.gameObject);
        }
        
        _upgrades.Clear();
        _originalButtonColor = upgradesButton.color;
    }

    private void Start()
    {
        GetCanvasGroup();
        UpdateUpgradePrices();
    }

    private void Update()
    {
        if (Tutorial.Instance.TutorialCompleted == false)
        {
            return;
        }

        if (_windowOpened)
        {
            return;
        }
        
        timer += Time.deltaTime;
        if (timer >= 3f)
        {
            timer = 0f;
            upgradeSign.SetActive(HasMoneyForUpgrade());
        }
    }

    public void ShowUpgradesPanel()
    {
        _windowOpened = true;
        upgradeSign.SetActive(false);
        ConstructUpgrades();
        UpdateUpgradePrices();
        SwitchPanel();
        EnableUpgradesPanel();
    }

    private bool HasMoneyForUpgrade()
    {
        bool hasMoney = false;
        foreach (var workerUpgrade in workerUpgradesRows)
        {
            if (workerUpgrade.EnoughtMoney())
            {
                return true;
            }
        }

        foreach (var upgrade in playerUpgrades)
        {
            if (upgrade.EnougthMoney())
            {
                return true;
            }
        }

        return false;
    }
    
    public void EnableWorkersPanel()
    {
        if (_workersWindowOpened)
        {
            return;
        }
        
        _workersWindowOpened = true;

        SwitchPanels();
    }
    
    public void EnableUpgradesPanel()
    {
        if (_workersWindowOpened == false)
        {
            return;
        }
        
        _workersWindowOpened = false;
        UpdateUpgradePrices();

        SwitchPanels();
    }

    public void CloseWindow()
    {
        _windowOpened = false;
        SwitchPanel();
        DestroyConstructedUpgrades();
        activeUpgradesCount = 0;
    }

    public void AddNewUpgrade(WorkerBehaviour worker, CharacterData data)
    {
        _upgrades.Add(new UpgradesData(worker, data));
    }

    public void OnAdWatched()
    {
        
    }

    public void RemoveAdEvents()
    {
        
    }
    
    public void AnimateFinger(bool state)
    {
        finger.gameObject.SetActive(state);
        finger.Animate = state;
        mainButtonAnimation.Animate = state;
    }

    public void ShowUpgradeFinger()
    {
        if (_upgradeTutorialCompleted)
        {
            return;
        }
        
        upgradeFinger.gameObject.SetActive(true);
        upgradeFinger.Animate = true;
    }
    
    public void HideFinger()
    {
        if (_upgradeTutorialCompleted)
        {
            return;
        }
        
        upgradeFinger.gameObject.SetActive(false);
        _upgradeTutorialCompleted = true;
        ES3.Save(upgradeTutorialFinger, _upgradeTutorialCompleted);
    }
    
    public void CheckAnimation(bool state)
    {
        if (_windowOpened)
        {
            return;
        }

        //AnimateFinger(state);
    }
    
    private void SwitchPanels()
    {
        workersPanel.DOFade(_workersWindowOpened ? 1 : 0, 0.5f);
        workersPanel.interactable = _workersWindowOpened;
        workersPanel.blocksRaycasts = _workersWindowOpened;

        upgradesPanel.DOFade(!_workersWindowOpened ? 1 : 0, 0.5f);
        upgradesPanel.interactable = !_workersWindowOpened;
        upgradesPanel.blocksRaycasts = !_workersWindowOpened;

        workerButton.color = _workersWindowOpened ? _originalButtonColor : unselectedButtonColor;
        upgradesButton.color = !_workersWindowOpened ? _originalButtonColor : unselectedButtonColor;
    }
    
    private void ConstructUpgrades()
    {
        UpgradeRow lastUpgrade = null;
        workerUpgradesRows.Clear();

        foreach (var upgradeData in _upgrades)
        {
            if (upgradeData.workerBehaviour.CanBuy() == false && upgradeData.workerBehaviour.IsBought() == false)
            {
                continue;
            }

            var currectShop = UIManager.Instance.teleportWindow.CurrentMagazine();
            if (upgradeData.workerBehaviour.CheckShop(currectShop) == false)
            {
                continue;
            }
            
            var row = Instantiate(workerUpgradeRow, content);
            row.Initialize(upgradeData);
            workerUpgradesRows.Add(row);
            
            if (upgradeData.workerBehaviour.CanBuy())
            {
                lastUpgrade = row;
            }
            
            activeUpgradesCount++;
        }
        
        if (lastUpgrade != null)
        {
            upgradesScroll.DONormalizedPos(Vector2.zero, 0.1f);
        }
    }

    private void DestroyConstructedUpgrades()
    {
        content.MMDestroyAllChildren();
    }

    public void UpdateUpgradePrices()
    {
        for (int i = 0; i < playerUpgrades.Length; i++)
        {
            playerUpgrades[i].UpdatePrice(i);
        }

        foreach (var row in workerUpgradesRows)
        {
            row.CheckButtonIteractebility();
        }
    }
}
