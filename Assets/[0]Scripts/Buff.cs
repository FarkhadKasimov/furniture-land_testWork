using System;
using UnityEngine;
using UnityEngine.UI;

public class Buff : MonoBehaviour
{
    [SerializeField] private GameObject buffButton;

    private PlayerController controller;

    public void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            controller = other.GetComponent<PlayerController>();
            ShowButton();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            HideButton();
        }
    }

    public void OnMouseDown()
    {
        if (buffButton.activeInHierarchy)
        {
            ShowAdForBuff();
        }
    }

    private void HideButton()
    {
        buffButton.SetActive(false);
    }

    private void ShowButton()
    {
        buffButton.SetActive(true);
    }

    public void ShowAdForBuff()
    {
        ADSManager.Instance.RewardedAdFailEvent += ClearEvents;
        ADSManager.Instance.RewardedAdViwedEvent += BuffPlayer;
            
        ADSManager.Instance.ShowRewardedAd(ADSManager.BUFF_PLACEMENT);
    }
    
    private void ClearEvents()
    {
        ADSManager.Instance.RewardedAdFailEvent -= ClearEvents;
        ADSManager.Instance.RewardedAdViwedEvent -= BuffPlayer;
    }

    private void BuffPlayer()
    {
        controller.BuffPlayer();
        Destroy(gameObject);
    }
}
