using System;
using DG.Tweening;
using Project.Internal;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainInfoUI : Singleton<MainInfoUI>
{
    [SerializeField] private TargetOrderUI orderPrefab;
    [SerializeField] private float YOffset = 5;

    public TargetOrderUI GetOrderUI()
    {
        var ui = Instantiate(orderPrefab, transform);
        ui.offset = YOffset;
        return ui;
    }
}
