using System;
using DG.Tweening;
using Project.Internal;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    public NoMoneyPopUp noMoneyPopUp;
    public BottomTeleportMenu bottomMenu;
    public TeleportUI teleportWindow;
    
    [SerializeField] private TextMeshProUGUI timerLable;
    [SerializeField] private CanvasGroup timer;
    [SerializeField] private Slider sliderTimer;
    [SerializeField] private CanvasGroup fadePanel;
    
    private TimeSpan _duration = new TimeSpan(0 ,0 ,0);
    
    public void ShowTimer(int timerValue)
    {
        timer.DOFade(1, 0.5f);
        sliderTimer.wholeNumbers = true;
        sliderTimer.maxValue = timerValue;
        sliderTimer.value = timerValue;
    }

    public void CloseTimer()
    {
        timer.DOFade(0, 0.5f);
    }

    public void UpdateTimer(int currentTime, int spriteType)
    {
        _duration = new TimeSpan(0, 0, currentTime);
        timerLable.text = "<sprite=" + spriteType + "> " + _duration.ToString(@"mm\:ss");
        sliderTimer.value = currentTime;
    }

    public void ShowFadePanel(float duration)
    {
        fadePanel.DOFade(1f, duration);
    }

    public void HideFadePanel(float duration)
    {
        fadePanel.DOFade(0f, duration);
    }
}
