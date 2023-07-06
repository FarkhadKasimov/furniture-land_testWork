using System;
using System.Collections;
//using Google.Play.Review;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

public class RateUs : MonoBehaviour
{
    public GameObject rateUsUI;
    public float timeInMin = 10;
    public float timeInMinOffset = 60;
        
    //private ReviewManager _reviewManager;
    //private PlayReviewInfo _playReviewInfo;
    
    
    private float _currentTime = 0f;
    private bool _canShowRateUs = true;
    private bool _rateUsShown = false;
    private int _times = 0;

    private void Start()
    {
        //_reviewManager = new ReviewManager();
        //_playReviewInfo = null;
        _currentTime = timeInMin;
        _canShowRateUs = ES3.Load("can_Show_Rate_Us", true);
        _rateUsShown = false;
        _times = 0;
    }

    private void Update()
    {
        if (_canShowRateUs == false)
        {
            return;
        }

        if (_rateUsShown == true)
        {
            return;
        }

        var timeToTrigger = timeInMin * 60 + (_times * timeInMinOffset * 60f);

        _currentTime += Time.deltaTime;

        if (_currentTime >= timeToTrigger)
        {
            ShowRateUsUI();
        }
    }

    public void ShowRateUs()
    {
        _rateUsShown = true;
        //StartCoroutine(ShowRateUsRoutine());
    }

    [Button]
    public void ShowRateUsUI()
    {
        rateUsUI.SetActive(true);
    }

    public void HideRateUsUI()
    {
        rateUsUI.SetActive(false);
        _rateUsShown = false;
        _times += 1;
    }
    
    //private IEnumerator ShowRateUsRoutine()
    //{
    //    _canShowRateUs = false;
    //    ES3.Save("can_Show_Rate_Us", false);
        
    //    var requestFlowOperation = _reviewManager.RequestReviewFlow();
    //    yield return requestFlowOperation;
    //    if (requestFlowOperation.Error != ReviewErrorCode.NoError)
    //    {
    //        Debug.Log("<color=red>" + requestFlowOperation.Error + "</color>");
    //        yield break;
    //    }
    //    _playReviewInfo = requestFlowOperation.GetResult();
        
    //    var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
    //    yield return launchFlowOperation;
        
    //    _playReviewInfo = null;
    //    if (launchFlowOperation.Error != ReviewErrorCode.NoError)
    //    {
    //        Debug.Log("<color=red>" + requestFlowOperation.Error + "</color>");
    //    }
        
    //    Debug.Log("Rate us completed!");
        
    //    HideRateUsUI();
    //}
}
