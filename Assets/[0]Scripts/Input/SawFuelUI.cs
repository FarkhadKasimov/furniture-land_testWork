using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class SawFuelUI : MonoBehaviour
{
    public event Action FuelEmptyEvent;

    [SerializeField] private Color emptyColor = Color.red;
    [SerializeField] private Color fullColor = Color.green;
    [SerializeField] private Slider slider;
    [SerializeField] private Image sliderFillImage;
    [SerializeField] private ScaleAnimation scaleAnimation;
    [SerializeField] private Image attentionSign;
    [SerializeField] private Image notFuelSign;

    private CanvasGroup group;
    private float fillSpeed = 1f;
    private bool startFilling = false;
    private bool fuelEmpty = false;
    private bool scaleAnimationRunned = false;
    private float fillAmount = 0f;
    private float triggerValue = 0f;

    public void Initialize(float sliderValue, float speed)
    {
        group = GetComponent<CanvasGroup>();
        fillSpeed = speed;
        fuelEmpty = false;
        StopScaleAnimation();
        notFuelSign.gameObject.SetActive(false);
        slider.maxValue = sliderValue;
        slider.value = sliderValue;
        sliderFillImage.color = fullColor;
        fillAmount = fillSpeed / slider.value * Time.fixedDeltaTime;
        triggerValue = (sliderValue * 45) / 100f;
        group.DOFade(1, 0.3f);
    }

    private void FixedUpdate()
    {
        if (startFilling == false)
        {
            return;
        }

        if (fuelEmpty)
        {
            return;
        }

        if (slider.value < triggerValue && scaleAnimationRunned == false)
        {
            RunScaleAnimation();
        }
        
        if (slider.value <= 0)
        {
            fuelEmpty = true;
            Tutorial.Instance.MoveNext(Tutorial.TutorialPath.waitFuel);
            notFuelSign.gameObject.SetActive(true);
            StopScaleAnimation();
            FuelEmptyEvent?.Invoke();
        }
        
        slider.value -= fillAmount;
        sliderFillImage.color = Color.Lerp(sliderFillImage.color, emptyColor, fillAmount/fillSpeed);
    }

    private void RunScaleAnimation()
    {
        scaleAnimationRunned = true;
        scaleAnimation.Animate = true;
        attentionSign.gameObject.SetActive(true);
    }

    private void StopScaleAnimation()
    {
        scaleAnimationRunned = false;
        scaleAnimation.Animate = false;
        attentionSign.gameObject.SetActive(false);
    }

    public void Hide()
    {
        group.DOFade(0, 0.3f);
        StopCollecting();
        StopScaleAnimation();
    }
    
    public void StartCollecting()
    {
        startFilling = true;
    }
    
    public void StopCollecting()
    {
        startFilling = false;
    }
    
    public bool CanCollectTree()
    {
        return slider.value > 0;
    }
}
