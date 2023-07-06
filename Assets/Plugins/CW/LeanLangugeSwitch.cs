using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Localization;
using System;
using System.Linq;

public class LeanLangugeSwitch : MonoBehaviour
{
    string prevLang;
    // Start is called before the first frame update
    void Update()
    {
        var ll = GetComponent<LeanLocalization>();
        //for webgl -> choose yandex language if possible, or choose system language
        //for others -> choose system language

#if UNITY_WEBGL
        try
        {
            string lang = YG.YandexGame.EnvironmentData.language;
            if (prevLang == lang)
                return;
            prevLang = lang;
            Debug.Log("Yandex lang = " + lang);
            ll.DetectLanguage = LeanLocalization.DetectType.None;
            //try to find the existing language
            var leanLanguages = LeanLocalization.CurrentLanguages.Values.ToList();
            bool found = false;
            for (int i = 0; i < leanLanguages.Count; i++)
            {
                if (leanLanguages[i].Cultures.Find(c => c.Equals(lang)) != null)
                {
                    ll.CurrentLanguage = (leanLanguages[i].name);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                ll.DetectLanguage = LeanLocalization.DetectType.SystemLanguage;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.StackTrace);
            ll.DetectLanguage = LeanLocalization.DetectType.SystemLanguage;
        }
#else
        ll.DetectLanguage = LeanLocalization.DetectType.SystemLanguage;
#endif
    }
}
