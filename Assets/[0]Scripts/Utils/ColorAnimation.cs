using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ColorAnimation : MonoBehaviour
{
    [SerializeField] private bool animateOnStart = false;
    [SerializeField] private float AnimationSpeed = 1f;

    [SerializeField] private Color startColor = Color.white;
    [SerializeField] private Color endColor = Color.white;

    private bool animate = true;
    private Image _image;

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

    private void Start()
    {
        _image = GetComponent<Image>();

        if (animateOnStart)
        {
            animate = true;
            StartCoroutine(PlayAnimation());
        }
    }

    public void PlayOnce()
    {
        StartCoroutine(PlayAnimationOnce());
    }

    private IEnumerator PlayAnimationOnce()
    {
        _image.DOColor(endColor, AnimationSpeed);
        yield return new WaitForSecondsRealtime(AnimationSpeed);
        _image.DOColor(startColor, AnimationSpeed);
        yield return new WaitForSecondsRealtime(AnimationSpeed);
    }

    private IEnumerator PlayAnimation()
    {
        while (animate)
        {
            _image.DOColor(endColor, AnimationSpeed);
            yield return new WaitForSecondsRealtime(AnimationSpeed);
            _image.DOColor(startColor, AnimationSpeed);
            yield return new WaitForSecondsRealtime(AnimationSpeed);
        }
    }
}
