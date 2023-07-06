using System;
using System.Collections.Generic;
using AppsFlyerSDK;
//using GameAnalyticsSDK;
using Project.Internal;
using UnityEngine;
using UnityEngine.Purchasing;

public class AnalyticsManager : Singleton<AnalyticsManager>
{
    private const string googleKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAg5l9QxXrjSnAHGBhDvdfS3QT21dOKTTI0LfOh7pkjXtFHlGFLkzTojKRhZhazzipn/cumwolSTNZyeb891vn7mcETo+Gz/tboFE3UbUnb4ObNpsHEjlZftoQUni4xf21isMHdWcHBQ1JGpZuDz36nZqWJBqdiFoiErNIzrLdUR4Q0uRm9d+RIr5eSptUrjUTIf8z5/4ZFkM/H/8NAqITnjvPg8m5h0NsAG9iK9dC9xqVy8h2qUQkq4nNmBwi2CvIisJHWv7CLMtB8k01FXzzamtywrZEuDYMMxv7MOFeHbhBkHhiaEQBvsPYz+qgRXhoC21AXfBtQSZGLEsaYtDr+QIDAQAB";
    
    public int adsViewCountsTrigger = 5;

    private int _adsViewedCount = 0;
    private float _currentPlayTime = 0;
    private float _totalPlayTime = 0;
    private DateTime _firstEnterDate;
    private int popUpSownCount = 0;

    private void Update()
    {
        if (_currentPlayTime >= 60f)
        {
            ReportTotalPlayTime();
            _currentPlayTime = 0f;
        }

        _currentPlayTime += Time.deltaTime;
        _totalPlayTime += Time.deltaTime;
    }

    public void OnGameStarted()
    {
        _adsViewedCount = 0;
        popUpSownCount = 0;
        _totalPlayTime = ES3.Load("total_playTime", 0f);

        ReportRetention1Day();
        ReportSessionsCount();
        //ReportEvent("game_start");
    }

    private void ReportTotalPlayTime()
    {
        var parameters = new Dictionary<string, object>();
        parameters.Add("minutes_total", _totalPlayTime / 60f);
        
        ES3.Save("total_playTime", _totalPlayTime / 60f);
       
        //ReportEvent("total_playtime", parameters);
    }

    private void ReportSessionsCount()
    {
        var sessionsCount = ES3.Load("total_session_count", 1);

        var parameters = new Dictionary<string, object>();
        parameters.Add("count", sessionsCount);

        sessionsCount += 1;
        ES3.Save("total_session_count", sessionsCount);
       
        //ReportEvent("game_start", parameters, true);
    }

    public void OnGameFinished()
    {
        //ReportEvent("game_finish");
    }

    public void OnNewFactureBuy()
    {
        //ReportEvent("buy_new_conveyor");
    }

    public void OnUpgradeBought(string type, int level)
    {
        //var parameters = new Dictionary<string, object>();
        //parameters.Add(type, level);
        
        //ReportEvent("buy_upgrade", parameters);
    }

    public void OnMoneySpend(string type, string item, float price)
    {
        var count = ES3.Load("total_money_spend_count", 1);
        count += 1;
        ES3.Save("total_money_spend_count", count);
        
        var parameters = new Dictionary<string, object>();
        parameters.Add("currency", "dollars");
        parameters.Add("type", type);
        parameters.Add("item", item);
        parameters.Add("price", price);
        parameters.Add("count", count);
        
        //ReportEvent("currency_spend", parameters);
    }

    public void OnMoneyGained(float amount)
    {
        var count = ES3.Load("total_money_gained_count", 1);
        count += 1;
        ES3.Save("total_money_gained_count", count);
        
        var parameters = new Dictionary<string, object>();
        parameters.Add("currency", "dollars");
        parameters.Add("source", "from_customer");
        parameters.Add("amount", amount);
        parameters.Add("count", count);
        
        //ReportEvent("currency_gained", parameters);
    }
    
    public void OnFirstMoneyCollected()
    {
        //ReportEvent("first_money_collected");
    }

    private void ReportAddsView()
    {
        _adsViewedCount += 1;

        var send = _adsViewedCount != 0 && _adsViewedCount % adsViewCountsTrigger == 0 && _adsViewedCount <= 30;
        if (send == false)
        {
            return;
        }
        
        AppsFlyer.sendEvent("ads_viewed_" + _adsViewedCount, null);
        //ReportEvent("ads_viewed_" + _adsViewedCount);
    }

    private void ReportRetention1Day()
    {
        if (ES3.KeyExists("first_entry") == false)
        {
            _firstEnterDate = DateTime.Now;
            Debug.Log("<color=green>" + _firstEnterDate + "</color>");
            ES3.Save("first_entry", _firstEnterDate);
        }
        else
        {
            _firstEnterDate = ES3.Load<DateTime>("first_entry");
            var now = DateTime.Now;

            var hoursDifference = (int)now.Subtract(_firstEnterDate).TotalHours;
            Debug.Log("<color=green> HOURS : " + hoursDifference + "</color>");
            if (hoursDifference is < 24 or > 48) return;
            
            AppsFlyer.sendEvent("ret_1", null);
            //ReportEvent("ret_1");
        }
    }
    
    public void VideoAdsAvailable(string adsType, string placement, string result, int internet, string ad_network)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>();
        parameters.Add("ads_type", adsType);
        parameters.Add("placement", placement);
        parameters.Add("result", result);
        parameters.Add("internet", internet);
        parameters.Add("ad_network", ad_network);

        //ReportEvent("video_ads_triggered", parameters);
    }
    
    public void VideoAdsStarted(string adsType, string placement, int internet, string ad_network)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>();
        parameters.Add("ads_type", adsType);
        parameters.Add("placement", placement);
        parameters.Add("result", "start");
        parameters.Add("internet", internet);
        parameters.Add("ad_network", ad_network);
        
        //ReportEvent("video_ads_started", parameters);
    }

    public void VideoAdsWatch(string adsType, string placement, string result, int internet, string ad_network)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>();
        parameters.Add("ads_type", adsType);
        parameters.Add("placement", placement);
        parameters.Add("result", result);
        parameters.Add("internet", internet);
        parameters.Add("ad_network", ad_network);
        
        //ReportEvent("video_ads_completed", parameters);
        
        if(result == "watched")
            ReportAddsView();
        if (adsType == "interstitial")
            CheckNoADsPopUp();
    }

    private void CheckNoADsPopUp()
    {
        popUpSownCount += 1;
        if (popUpSownCount != 0 && popUpSownCount % 3 == 0)
        {
            IAP.Instance.ShowNOADSPopUp();
        }
    }

    public void NoAdsPurchased(Product product)
    {
        //ReportEvent("no_ads_purchased");
        
        /*Dictionary<string, string> purchaseEvent = new Dictionary<string, string>
        {
            {AFInAppEvents.CURRENCY, product.metadata.isoCurrencyCode},
            {AFInAppEvents.REVENUE, product.metadata.localizedPriceString},
            {AFInAppEvents.QUANTITY, "1"},
            {AFInAppEvents.CONTENT_TYPE, "no_ads"}
        };
        
        AppsFlyer.sendEvent ("af_purchase", purchaseEvent);
        */
        
        string receipt = product.receipt;
        var wrapper = (Dictionary<string, object>) MiniJson.JsonDecode (receipt);

        if (wrapper == null)
        {
            return;
        }
        
        var payload = (string)wrapper ["Payload"];
        var details = (Dictionary<string, object>)MiniJson.JsonDecode (payload);

        var signature = "";
        var data = "";
        
        if (details != null)
        {
            signature = (string) details["signature"];
            data = (string) details["json"];
        }

        AppsFlyer.validateAndSendInAppPurchase( googleKey, signature,  data,  product.metadata.localizedPriceString, product.metadata.isoCurrencyCode ,  null,  this);
    }

    //private void ReportEvent(string eventName, Dictionary<string, object> parameters, bool reportFromBuffer = false)
    //{
    //    AppMetrica.Instance.ReportEvent(eventName, parameters);
    //    if (reportFromBuffer)
    //    {
    //        AppMetrica.Instance.SendEventsBuffer();
    //    }

    //    GameAnalytics.NewDesignEvent(eventName, parameters);
        
    //    ShowEventDebug(eventName, parameters);
    //}

    //private void ReportEvent(string eventName, bool reportFromBuffer = false)
    //{
    //    AppMetrica.Instance.ReportEvent(eventName);
    //    if (reportFromBuffer)
    //    {
    //        AppMetrica.Instance.SendEventsBuffer();
    //    }

    //    GameAnalytics.NewDesignEvent(eventName);
        
    //    ShowEventDebug(eventName);
    //}

    private void ShowEventDebug(string eventName, Dictionary<string, object> parameters = null)
    {
        Debug.Log("<color=yellow>Event " + eventName + " raported!</color>");
        if (parameters != null)
        {
            Debug.Log("<color=yellow>With parameters : </color>");

            foreach (var parameter in parameters)
            {
                Debug.Log("<color=yellow>" + parameter.Key + " -- " + parameter.Value + "</color>");
            }
        }
    }
}
