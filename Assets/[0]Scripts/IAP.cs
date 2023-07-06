using UnityEngine;
using System;
using Project.Internal;
using TMPro;
using UnityEngine.Purchasing;

public class IAP : Singleton<IAP>
{
    private const string NO_ADS_ID = "com.wab.noads";
    private const string NO_ADS_SAVEKEY = "no_ads";
    
    [SerializeField] private IStoreListener listener;

    private static string kProductNameAppleSubscription = "com.unity3d.subscription.new";
    private static string kProductNameGooglePlaySubscription = "com.unity3d.subscription.original";

    [SerializeField] private GameObject noAdsPanel;
    [SerializeField] private GameObject noAdsButton;
    [SerializeField] private float moneyForPurchase;
    [SerializeField] private GameObject playerCap;

    private bool noAdsPurchased = false; 

    void Start()
    {
        var controller = CodelessIAPStoreListener.Instance.StoreController;

        CheckRefund(controller);
        
        noAdsPurchased = ES3.Load(NO_ADS_SAVEKEY, false);
        ADSManager.Instance.CheckNoADs(noAdsPurchased);
        if (noAdsPurchased)
        {
            CheckPlayerCap();
            noAdsButton.SetActive(false);
        }

    }

    private void CheckRefund(IStoreController controller)
    {
        if (controller == null)
        {
            print("NO CONTROLLER!!!");
            return;
        }
        
        var product = controller.products.WithID(NO_ADS_ID);
        if (product.hasReceipt == false)
        {
            ES3.Save(NO_ADS_SAVEKEY, false);
        }
    }

    private void GetRewardsForPurchase()
    {
        MoneyManager.Instance.AddMoney(moneyForPurchase);
        CheckPlayerCap();
    }

    private void CheckPlayerCap()
    {
        if (playerCap.activeInHierarchy == false)
        {
            playerCap.SetActive(true);
        }
    }
    
    public void OnPurchaseCompleted(Product product)
    {
        if (string.Equals(product.definition.id, NO_ADS_ID, StringComparison.Ordinal))
        {
            ES3.Save(NO_ADS_SAVEKEY, true);
            ADSManager.Instance.CheckNoADs(true);
            AnalyticsManager.Instance.NoAdsPurchased(product);
            GetRewardsForPurchase();
            CloseNOADSPopUp();
            noAdsButton.SetActive(false);
        }
    }

    public void CloseNOADSPopUp()
    {
        noAdsPanel.SetActive(false);
    }

    public void ShowNOADSPopUp()
    {
        if (noAdsPurchased)
        {
            return;
        }
        
        noAdsPanel.SetActive(true);
    }
}
