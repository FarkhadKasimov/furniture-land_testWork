using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlatformObjectEnabler : MonoBehaviour
{
    public List<PlatformType> platformType = new List<PlatformType>();
    public UnityEvent onSupport;
    public UnityEvent onDontSupport;
    public bool disableIfDontSupport = true;
    // Start is called before the first frame update
    void Start()
    {
        PlatformType currentPlatform = PlatformType.NONE;

#if UNITY_ANDROID || UNITY_IOS
        currentPlatform = PlatformType.MOBILE;
#elif UNITY_WEBGL
        if (PlatformDetector.IsMobile()) {
            currentPlatform = PlatformType.MOBILE_WEBGL;
        }
        else {
            currentPlatform = PlatformType.DESKTOP_WEBGL;
        }
#elif UNITY_STANDALONE
        currentPlatform = PlatformType.DESKTOP;
#endif

        for (int i = 0; i < platformType.Count; i++)
        {
            if (platformType[i] == currentPlatform) {
                onSupport.Invoke();
                return;
            }
        }
        onDontSupport.Invoke();
        if(disableIfDontSupport)
            gameObject.SetActive(false);
    }

    public enum PlatformType {
        NONE,
        DESKTOP_WEBGL,
        MOBILE_WEBGL,
        DESKTOP,
        MOBILE
    }
}
