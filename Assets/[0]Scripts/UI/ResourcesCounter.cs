using System;
using DG.Tweening;
using Project.Internal;
using TMPro;
using UnityEngine;

public class ResourcesCounter : Singleton<ResourcesCounter>
{
    [System.Serializable]
    public class CountersData
    {
        public TextMeshProUGUI counter;
        public int currentCount;
        public ObjectSettings type;
        public GameObject counterObject;

        public void UpdateCounter(bool add)
        {
            currentCount += add ? 1 : -1;

            if (currentCount < 0)
            {
                currentCount = 0;
            }
            
            counter.text = currentCount.ToString();
            counterObject.SetActive(currentCount > 0);
        }

        public bool CheckType(ObjectSettings resource)
        {
            return resource == type;
        }

        public void ShowMax()
        {
            counter.text = "MAX";
            counterObject.transform.DOShakePosition(.5f, 30f, 30);
        }

        public void HideMax()
        {
            counter.text = currentCount.ToString();
            counterObject.SetActive(currentCount > 0);
        }
            
    }
    
    [SerializeField] private CanvasGroup mainGroup;
    [SerializeField] private CountersData[] countersData;

    public void Start()
    {
        Show();
    }

    public void Hide()
    {
        mainGroup.DOFade(0, 0.5f);
    }

    public void Show()
    {
        mainGroup.DOFade(1, 0.5f);
    }

    public void AddResource(ObjectSettings resource)
    {
        foreach (var data in countersData)
        {
            if (data.CheckType(resource))
            {
                data.UpdateCounter(true);
            }
        }
    }

    public void RemoveResource(ObjectSettings resource)
    {
        foreach (var data in countersData)
        {
            if (data.CheckType(resource))
            {
                data.UpdateCounter(false);
            }
        }
    }

    public void ShuffleText()
    {
        foreach (var data in countersData)
        {
            data.ShowMax();
        }

        DOVirtual.DelayedCall(1.5f, () =>
        {
            foreach (var data in countersData)
            {
                data.HideMax();
            }
        });
    }
}
