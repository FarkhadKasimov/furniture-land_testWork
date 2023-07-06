using System;
using DG.Tweening;
using Project.Internal;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class MoneyManager : Singleton<MoneyManager>
{
    [SerializeField] private TextMeshProUGUI moneyLable;
    private float _money = 0;
    private const string MoneySavekey = "MONEY_SAVEKEY";
    public float Money => _money;

    [Button]
    [GUIColor(0, 1, 0)]
    private void Get1000()
    {
        AddMoney(1000);
    }

    public void Start()
    {
        _money = ES3.Load(MoneySavekey, 0f);
        moneyLable.text = "<sprite=0>" + Mathf.CeilToInt(_money);
    }

    public bool HasEnoughtMoney(float moneyForCheck)
    {
        return _money >= moneyForCheck;
    }

    public bool SpentMoney(float moneyToSpend, string item, string type)
    {
        if (HasEnoughtMoney(moneyToSpend))
        {
            _money -= moneyToSpend;
            moneyLable.text = "<sprite=0>" + Mathf.CeilToInt(_money);
            ES3.Save(MoneySavekey, _money);
            AnalyticsManager.Instance.OnMoneySpend(type, item, moneyToSpend);
            return true;
        }

        return false;
    }

    public void AddMoney(float moneyToAdd)
    {
        var currentMoney = _money;
        var moneyto = currentMoney + moneyToAdd;
        
        DOTween.To(()=> currentMoney, x=> currentMoney = x, moneyto, 0.5f).OnUpdate(() =>
        {
            moneyLable.text = "<sprite=0>" + Mathf.CeilToInt(currentMoney);
        });
        
        _money += moneyToAdd;
        ES3.Save(MoneySavekey, _money);
    }
}
