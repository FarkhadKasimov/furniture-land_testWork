using com.adjust.sdk;
using UnityEngine;

public class AdjustInitializer : MonoBehaviour
{
    [SerializeField] private string IOS_Token = "YOUR_IOS_APP_TOKEN_HERE";
    [SerializeField] private string ANDROID_Token = "YOUR_ANDROID_APP_TOKEN_HERE";
    private void Start()
    {
#if UNITY_IOS
            /* Mandatory - set your iOS app token here */
            InitAdjust(IOS_Token);
#elif UNITY_ANDROID
        /* Mandatory - set your Android app token here */
        InitAdjust(ANDROID_Token);
#endif
    }

    private void InitAdjust(string adjustAppToken)
    {
        var adjustConfig = new AdjustConfig(adjustAppToken, AdjustEnvironment.Production, true);

        adjustConfig.setLogLevel(AdjustLogLevel.Info);
        adjustConfig.setSendInBackground(true);
        new GameObject("Adjust").AddComponent<Adjust>();

        adjustConfig.setAttributionChangedDelegate((adjustAttribution) => 
        {
            // Debug.LogFormat("Adjust Attribution Callback: ", adjustAttribution.trackerName);
        });

        Adjust.start(adjustConfig);
    }
}
