using System.Collections;
using System.ComponentModel;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine.Events;

public class ScaleAnimation : MonoBehaviour
{
    public UnityEvent beginEvent;
    public UnityEvent endEvent;
    private bool animate = true;
    [SerializeField] private bool animateOnStart = false;
    [SerializeField] private float withDelay = 0;
    public bool Animate
    {
        get
        {
            return animate;
        }
        set
        {
            animate = value;
            if (animate)
            {
                StartCoroutine(PlayAnimation());
            }
            else
            {
                StopCoroutine(PlayAnimation());
            }
        }
    }

    [SerializeField] private float AnimationSpeed;
    [SerializeField] private Vector3 endValue;
    [SerializeField] private bool reverceScale = true;
    [SerializeField] private Vector3 initialScaleToSet = Vector3.zero;
    private Vector3 origiginalValue;

    [ButtonGroup]
    [GUIColor(0,1,0)]
    public void TestAnimation()
    {
        PlayOnce();
    }

    
    
    private void Start()
    {
        origiginalValue = transform.localScale;
        if(animateOnStart)
        {
            animate = true;
            StartCoroutine(PlayAnimation());
        }
    }

    public void PlayOnce(bool reverce = false)
    {
        StartCoroutine(PlayAnimationOnce(reverce));
    }

    public void PlayOnceWithSet()
    {
        transform.localScale = initialScaleToSet;

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(PlayAnimationOnce(false));
        }
    }
    
    private IEnumerator PlayAnimationOnce(bool reverce)
    {
        transform.DOScale(reverce ? origiginalValue : endValue, AnimationSpeed);
        yield return new WaitForSecondsRealtime(AnimationSpeed);
        yield return new WaitForSeconds(withDelay);
        if(reverceScale)
        {
            transform.DOScale(origiginalValue, AnimationSpeed);
            yield return new WaitForSecondsRealtime(AnimationSpeed);
        }
        else
        {
            yield return null;
        }
    }

    private IEnumerator PlayAnimation()
    {
        while (animate)
        {
            transform.DOScale(endValue, AnimationSpeed).OnComplete(BeginEvent);
            BeginEvent();
            yield return new WaitForSecondsRealtime(AnimationSpeed);
            yield return new WaitForSeconds(withDelay);
            EndEvent();
            transform.DOScale(origiginalValue, AnimationSpeed);
            yield return new WaitForSecondsRealtime(AnimationSpeed);
            yield return null;
        }
    }

    public void BeginEvent()
    {
        beginEvent.Invoke();
    }

    private void EndEvent()
    {
        endEvent.Invoke();
    }
}
