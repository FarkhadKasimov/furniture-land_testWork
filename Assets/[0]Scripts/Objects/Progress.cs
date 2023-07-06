using System;
using DG.Tweening;
using Project.Internal;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class Progress : Singleton<Progress>
{
    [System.Serializable]
    public class ProgressStep
    {
        [FoldoutGroup("-Objects List-")] public FabriqueMachine[] objectsList;
    }

    [SerializeField] private float checkDelay = 10f;
    
    private int currentProgressIndex = 0;
    public ProgressStep[] Steps;
    private float currentTime = 0;

    public UnityEvent FirstShopWasBoughtEvent;

    public void Initialize()
    {
        currentProgressIndex = 0;
        foreach (var step in Steps)
        {
            bool completed = true;
            foreach (var magazine in step.objectsList)
            {
                if (magazine.IsBought == false)
                {
                    completed = false;
                    break;
                }
            }

            if (completed)
            {
                var index = Mathf.Clamp(currentProgressIndex + 1, 0, Steps.Length - 1);
                currentProgressIndex = index;
            }
            else
            {
                break;
            }
        }

        if (currentProgressIndex == 0)
        {
            currentProgressIndex = -1;
        }
        
        CheckProgress();
    }

    private void Start()
    {
        //InvokeRepeating(nameof(CheckAnimationFingerState), checkDelay, checkDelay);
    }

    public void CheckProgress(bool fromWareHouse = false)
    {
        if (currentProgressIndex >= Steps.Length)
        {
            return;
        }
        
        var currentStep = Steps[Mathf.Clamp(currentProgressIndex, 0, Steps.Length - 1)];
        bool completed = true;
        
        foreach (var magazine in currentStep.objectsList)
        {
            if (magazine.IsBought == false)
            {
                completed = false;
                break;
            }
        }

        if (fromWareHouse)
        {
            completed = true;
        }
        
        if (completed)
        {
            currentProgressIndex++;

            var SendEvent = currentProgressIndex % 2 == 0;
            if (SendEvent)
            {
                AnalyticsManager.Instance.OnNewFactureBuy();
            }
            
            EnableOtherMagazines();
        }
    }

    public void EnableOtherMagazines()
    {
        var step = Steps[Mathf.Clamp(currentProgressIndex, 0, Steps.Length - 1)];
        foreach (var magazine in step.objectsList)
        {
            FactorySaves.Instance.EnableMagazineBuy(magazine);
        }

        if (currentProgressIndex >= 9)
        {
            FirstShopWasBoughtEvent?.Invoke();
        }
    }

    private void CheckAnimationFingerState()
    {
        var step = Steps[Mathf.Clamp(currentProgressIndex, 0, Steps.Length - 1)];
        foreach (var magazine in step.objectsList)
        {
            //if (magazine.IsWorker && magazine.CanBuy() && !magazine.IsBought())
            //{
                //UpgradesUI.Instance.CheckAnimation(true);
            //}
        }
    }
}
