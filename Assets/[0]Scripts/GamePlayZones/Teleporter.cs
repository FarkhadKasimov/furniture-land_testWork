using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;

public class Teleporter : MonoBehaviour
{
    public UnityEvent teleportEvent;
    public bool withTime = false;
    public bool enableRocks = false;

    [ShowIf(nameof(withTime))][SerializeField] private Image fillImage;
    [ShowIf(nameof(withTime))][SerializeField] private float time;
    
    private Coroutine coroutine;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            if (withTime == false)
            {
                teleportEvent.Invoke();
                ADSManager.Instance.ShowInterstitial();
            }
            else
            {
                coroutine = StartCoroutine(InvokeWithTime());
            }    
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (withTime == false)
        {
            return;
        }

        if (other.GetComponent<PlayerController>())
        {
            StopRoutine();
        }
    }

    public void TeleportToResources(int zoneIndex)
    {
        GamePlayZoneList.Instance.EnableZoneByType((GamePlayZoneType)zoneIndex, enableRocks);
    }
    
    private IEnumerator InvokeWithTime()
    {
        var fillAmount = 1 / time * Time.deltaTime;

        while (fillImage.fillAmount < 0.99f)
        {
            fillImage.fillAmount += fillAmount;
            yield return null;
        }

        teleportEvent.Invoke();
        ADSManager.Instance.ShowInterstitial();
        StopRoutine();
        
        yield return null;
    }

    private void StopRoutine()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
            fillImage.fillAmount = 0f;
        }
    }
}
