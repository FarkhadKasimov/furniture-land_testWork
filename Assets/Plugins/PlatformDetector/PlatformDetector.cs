#if UNITY_WEBGL
using System.Runtime.InteropServices;
#endif
using UnityEngine;

public class PlatformDetector : MonoBehaviour
{
#if UNITY_WEBGL
    [DllImport("__Internal")] static extern bool IsMobilePlatform();
    [DllImport("__Internal")] static extern bool IsIOSPlatform();
    [DllImport("__Internal")] static extern bool IsAndroidPlatform();
#endif

    private static bool initialized = false;
    private static bool isMobileDevice = false;
    private static bool isIOSDevice = false;
    private static bool isAndroidDevice = false;

    private static void Initialize()
    {
        if (initialized)
            return;
        isMobileDevice = IsMobileInit();
        isIOSDevice = IsIOSMobileInit();
        isAndroidDevice = IsAndroidMobileInit();
        initialized = true;
    }
    public static bool IsMobile()
    {
        if (!initialized)
            Initialize();
        return isMobileDevice;
    }
    public static bool IsMobileInit()
    {
        bool returnValue = false;
#if UNITY_EDITOR
        returnValue = false;
#elif UNITY_WEBGL
        returnValue = IsMobilePlatform(); // value based on the current browser
#elif UNITY_IOS || UNITY_ANDROID
        returnValue = true;
#endif
        return returnValue;
    }

    public static bool IsIOSMobile()
    {
        if (!initialized)
            Initialize();
        return isIOSDevice;
    }
    private static bool IsIOSMobileInit()
    {
        if (initialized)
            return isIOSDevice;
        bool returnValue = false;
#if UNITY_EDITOR
        returnValue = true; //FOR TESTING PURPOSE
        returnValue = false; // value to return in Play Mode (in the editor)
#elif UNITY_WEBGL
        returnValue = IsIOSPlatform(); // value based on the current browser
#elif UNITY_IOS
        returnValue = true;
#else
        returnValue = false;
#endif
        return returnValue;
    }

    public static bool IsAndroidMobile()
    {
        if (!initialized)
            Initialize();
        return isAndroidDevice;
    }
    public static bool IsAndroidMobileInit()
    {
        bool returnValue = false;

#if UNITY_EDITOR
        returnValue = true; //FOR TESTING PURPOSE
        returnValue = false; // value to return in Play Mode (in the editor)
#elif UNITY_WEBGL
        returnValue = IsAndroidPlatform(); // value based on the current browser
#elif UNITY_ANDROID
        returnValue = true;
#else
        returnValue = false;
#endif
        return returnValue;
    }
}