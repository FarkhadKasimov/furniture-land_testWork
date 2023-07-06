using System;
using UnityEngine;

public class ApplovinMaxInitializer : MonoBehaviour
{
    [SerializeField] private string key = "RIDnyxt0n58Jg7u5qBUldnnxBIMnh89Antc3Z5LemvvErLapxOsop-Db4hkIi1ZxLaocTGRCfCTxp9V6HiGhHR";
    [SerializeField] private bool showMediationDebugger = false;
    private void Start()
    {
        MaxSdk.SetSdkKey(key);
        MaxSdk.SetUserId(SystemInfo.deviceUniqueIdentifier);
        MaxSdk.SetVerboseLogging(true);
        MaxSdkCallbacks.OnSdkInitializedEvent += OnMaxInitialized;
        MaxSdk.InitializeSdk();
    }

    private void OnMaxInitialized(MaxSdkBase.SdkConfiguration sdkConfiguration)
    {
        if (MaxSdk.IsInitialized()) 
        {
            if (showMediationDebugger)
            {
                MaxSdk.ShowMediationDebugger();
            }
            Debug.Log("MaxSDK initialized");
        } 
        else 
        {
            Debug.Log("Failed to init MaxSDK");
        }
    }
}
