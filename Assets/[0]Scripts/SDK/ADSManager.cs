using System;
using System.Collections;
using Project.Internal;
using Sirenix.Utilities;
using UnityEngine;

public class ADSManager : Singleton<ADSManager>
{
    public const string BOOST_PLACEMENT = "boost_collection";
    public const string UPGRADES_PLACEMENT = "upgrade_entity";
    public const string BUFF_PLACEMENT = "player_buff";
    public const string INTER_PLACEMENT = "interstitial_show";
    public const string BANNER_PLACEMENT = "banner";

    public event Action RewardedAdViwedEvent;
    public event Action RewardedAdFailEvent;

    [SerializeField] private string InterstitialAdUnitId = "ENTER_INTERSTITIAL_AD_UNIT_ID_HERE";
    [SerializeField] private string RewardedAdUnitId = "ENTER_REWARD_AD_UNIT_ID_HERE";
    [SerializeField] private string BannerAdUnitId = "YOUR_BANNER_AD_UNIT_ID";
    
    [SerializeField] private string SDK_KEY;
    [SerializeField] private int interstitialDelay = 40;
    [SerializeField] private bool showMediationDebugger = false;

    private int _interstitialRetryAttempt;
    private int _rewardedRetryAttempt;
    private int _initialDelay = 0;
    private int _maxDelay = 0;

    private float _lastInterstitialTime = 0f;
    private string _rewardedPlacement;
    private bool addShowing = false;
    private string _lastAddNetwork = "applovin";
    private string _lastRewardedNetwork = "applovin";

    private bool bannerShown = false;
    private Coroutine bannerRoutine = null;
    private bool NoAds = false;

    private void Start()
    {
        MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
        {
            InitializeInterstitialAds();
            InitializeRewardedAds();
            InitializeBannerAds();
            ShowMediationDebuggerWindow();
            StartBannerRoutine();
        };

        MaxSdk.SetSdkKey(SDK_KEY);
        MaxSdk.InitializeSdk();

        _lastInterstitialTime = interstitialDelay;
    }
    
    private void Update()
    {
        if (addShowing)
        {
            return;
        }
        
        if (_lastInterstitialTime < 0)
        {
            _lastInterstitialTime = 0;
            if (addShowing == false)
            {
                ShowInterstitial();
                ResetInterstitalTimer();
            }
        }
        else
        {
            _lastInterstitialTime -= Time.deltaTime;
        }
    }

    public void CheckNoADs(bool noads)
    {
        NoAds = noads;
        if (noads)
        {
            HideBanner();
        }
    }
    
    public void ShowBanner()
    {
        if (bannerShown)
        {
            return;
        }

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            return;
        }

        if (NoAds)
        {
            return;
        }
        
        MaxSdk.ShowBanner(BannerAdUnitId);
        bannerShown = true;
        StopShowingBannerRoutine();
    }

    public void HideBanner()
    {
        MaxSdk.HideBanner(BannerAdUnitId);
    }

    private void StartBannerRoutine()
    {
        if (bannerRoutine != null)
        {
            StopShowingBannerRoutine();
        }

        bannerRoutine = StartCoroutine(ShownigBannerRoutine());
    }
    
    private IEnumerator ShownigBannerRoutine()
    {
        while (bannerShown == false)
        {
            yield return new WaitForSeconds(1f);
            ShowBanner();
        }

        yield return null;
    }

    private void StopShowingBannerRoutine()
    {
        if (bannerRoutine != null)
        {
            StopCoroutine(bannerRoutine);
        }

        bannerRoutine = null;
    }
    
    
    
    private bool IntirstitialReady()
    {
        return _lastInterstitialTime <= 0;
    }

    private void ResetInterstitalTimer()
    {
        _lastInterstitialTime = interstitialDelay;
        addShowing = false;
    }
    
    public void ShowInterstitial()
    {
        if (IntirstitialReady() == false)
        {
            return;
        }

        if (NoAds)
        {
            return;
        }

        var interIsReady = MaxSdk.IsInterstitialReady(InterstitialAdUnitId);
        var result = interIsReady ? "success" : "fail";
        var internetConnection = Application.internetReachability == NetworkReachability.NotReachable ? 0 : 1;

        AnalyticsManager.Instance.VideoAdsAvailable("interstitial", INTER_PLACEMENT, result, internetConnection, _lastAddNetwork);
        
        if (interIsReady)
        {
            addShowing = true;
            MaxSdk.ShowInterstitial(InterstitialAdUnitId);
        }
    }
    
    public void ShowRewardedAd(String placement)
    {
        _rewardedPlacement = placement;

        var rewardedIsReady = MaxSdk.IsRewardedAdReady(RewardedAdUnitId);
        var result = rewardedIsReady ? "success" : "fail";
        var internetConnection = Application.internetReachability == NetworkReachability.NotReachable ? 0 : 1;

        AnalyticsManager.Instance.VideoAdsAvailable("rewarded", placement, result, internetConnection, _lastRewardedNetwork);
        
        if (rewardedIsReady)
        {
            addShowing = true;
            MaxSdk.ShowRewardedAd(RewardedAdUnitId);
        }
        else 
        { 
            RewardedAdFailEvent?.Invoke();
        }
    }
    
    private void ShowMediationDebuggerWindow()
    {
        if (showMediationDebugger == false)
        {
            return;
        }
        
        MaxSdk.ShowMediationDebugger();
    }
    
    #region Interstitial Ad Methods

    private void InitializeInterstitialAds()
    {
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += OnInterstitialFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += InterstitialFailedToDisplayEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += OnInterstitialDismissedEvent;
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += OnInterstitialRevenuePaidEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += OnInterstitialDisplayedEvent;
        MaxSdkCallbacks.Interstitial.OnAdClickedEvent += OnInterstitialClickedEvent;
        
        LoadInterstitial();
    }
    
    private void LoadInterstitial()
    {
        MaxSdk.LoadInterstitial(InterstitialAdUnitId);
    }

    private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Interstitial loaded");

        _lastAddNetwork = adInfo.NetworkName;
        //AnalyticsManager.Instance.VideoAdsAvailable("interstitial", INTER_PLACEMENT);
        
        _interstitialRetryAttempt = 0;
    }

    private void OnInterstitialClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Interstitial clicked!");
        var internetConnection = Application.internetReachability == NetworkReachability.NotReachable ? 0 : 1;
        AnalyticsManager.Instance.VideoAdsWatch("interstitial", INTER_PLACEMENT, "clicked", internetConnection, adInfo.NetworkName);
    }
    
    private void OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        _interstitialRetryAttempt++;
        
        double retryDelay = Mathf.Pow(2, Mathf.Min(6, _interstitialRetryAttempt));
        Debug.Log("Interstitial failed to load with error code: " + errorInfo.Code);

        Invoke(nameof(LoadInterstitial), (float)retryDelay);
        ResetInterstitalTimer();
    }

    private void InterstitialFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Interstitial failed to display with error code: " + errorInfo.Code);
        LoadInterstitial();
        ResetInterstitalTimer();
    }

    private void OnInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Interstitial dismissed");
        ResetInterstitalTimer();
        
        var internetConnection = Application.internetReachability == NetworkReachability.NotReachable ? 0 : 1;
        AnalyticsManager.Instance.VideoAdsWatch("interstitial", INTER_PLACEMENT, "watched", internetConnection, adInfo.NetworkName);

        LoadInterstitial();
    }

    private void OnInterstitialRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Interstitial revenue paid");

        double revenue = adInfo.Revenue;

        string countryCode = MaxSdk.GetSdkConfiguration().CountryCode; 
        string networkName = adInfo.NetworkName;
        string adUnitIdentifier = adInfo.AdUnitIdentifier;
        string placement = adInfo.Placement;
    }

    private void OnInterstitialDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Interstitial revenue displayed");
        
        var internetConnection = Application.internetReachability == NetworkReachability.NotReachable ? 0 : 1;
        AnalyticsManager.Instance.VideoAdsStarted("interstitial", INTER_PLACEMENT, internetConnection, adInfo.NetworkName);
        
        ResetInterstitalTimer();
    }

    #endregion
    
    #region Rewarded Ad Methods

    private void InitializeRewardedAds()
    {
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
        MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdDismissedEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;

        LoadRewardedAd();
    }

    private void LoadRewardedAd()
    {
        MaxSdk.LoadRewardedAd(RewardedAdUnitId);
    }

    private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Rewarded ad loaded");

        _lastRewardedNetwork = adInfo.NetworkName;
        //AnalyticsManager.Instance.VideoAdsAvailable("rewarded", 
            //_rewardedPlacement.IsNullOrWhitespace() ? "loaded" : _rewardedPlacement);
        
        _rewardedRetryAttempt = 0;
    }

    private void OnRewardedAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        _rewardedRetryAttempt++;
        double retryDelay = Mathf.Pow(2, Mathf.Min(6, _rewardedRetryAttempt));

        Debug.Log("Rewarded ad failed to load with error code: " + errorInfo.Code);
        RewardedAdFailEvent?.Invoke();

        Invoke(nameof(LoadRewardedAd), (float)retryDelay);
        ResetInterstitalTimer();
    }

    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Rewarded ad failed to display with error code: " + errorInfo.Code);
        RewardedAdFailEvent?.Invoke();
        LoadRewardedAd();
        ResetInterstitalTimer();
    }

    private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Rewarded ad displayed");
        
        var internetConnection = Application.internetReachability == NetworkReachability.NotReachable ? 0 : 1;
        AnalyticsManager.Instance.VideoAdsStarted("rewarded",_rewardedPlacement, internetConnection, adInfo.NetworkName);
    }

    private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Rewarded ad clicked");
        var internetConnection = Application.internetReachability == NetworkReachability.NotReachable ? 0 : 1;
        AnalyticsManager.Instance.VideoAdsWatch("rewarded",_rewardedPlacement, "clicked" , internetConnection, adInfo.NetworkName);
    }

    private void OnRewardedAdDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Rewarded ad dismissed");
        RewardedAdFailEvent?.Invoke();
        LoadRewardedAd();
        ResetInterstitalTimer();
    }

    private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Rewarded ad received reward");
        
        var internetConnection = Application.internetReachability == NetworkReachability.NotReachable ? 0 : 1;
        AnalyticsManager.Instance.VideoAdsWatch("rewarded",_rewardedPlacement, "watched" , internetConnection, adInfo.NetworkName);
        
        RewardedAdViwedEvent?.Invoke();
        ResetInterstitalTimer();
    }

    private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("Rewarded ad revenue paid");

        double revenue = adInfo.Revenue;

        string countryCode = MaxSdk.GetSdkConfiguration().CountryCode;
        string networkName = adInfo.NetworkName;
        string adUnitIdentifier = adInfo.AdUnitIdentifier;
        string placement = adInfo.Placement;
    }

    #endregion
    
    #region Banner Ad Methods
    
    public void InitializeBannerAds()
    {
        MaxSdk.CreateBanner(BannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);
        MaxSdk.SetBannerBackgroundColor(BannerAdUnitId, Color.white);

        MaxSdkCallbacks.Banner.OnAdLoadedEvent      += OnBannerAdLoadedEvent;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent  += OnBannerAdLoadFailedEvent;
        MaxSdkCallbacks.Banner.OnAdClickedEvent     += OnBannerAdClickedEvent;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
        MaxSdkCallbacks.Banner.OnAdExpandedEvent    += OnBannerAdExpandedEvent;
        MaxSdkCallbacks.Banner.OnAdCollapsedEvent   += OnBannerAdCollapsedEvent;
    }

    private void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        
    }

    private void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        
    }

    private void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        
    }

    private void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        
    }

    private void OnBannerAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        
    }

    private void OnBannerAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        
    }
    
    #endregion
}
