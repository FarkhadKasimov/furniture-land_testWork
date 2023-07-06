using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TargetOrderUI : MonoBehaviour
{
    internal float offset;
    
    [SerializeField] private TargetOrderRowText rowText;
    [SerializeField] private CanvasGroup mainCanvasGroup;
    [SerializeField] private GameObject cassaImage;
    [SerializeField] private GameObject wrappImage;

    private Transform _target;
    private TargetOrderRowText row;
    private Camera mainCamera;

    public void Initialize(WantedResource order, Transform target)
    {
        row = Instantiate(rowText, transform);

        mainCamera = Camera.main;
        _target = target;
        
        ActualizeRow(order);

        DOVirtual.DelayedCall(0.5f, () =>
        {
            mainCanvasGroup.DOFade(1, 0.2f);
        });
    }

    public void ActualizeRow(WantedResource order)
    {
        row.ShowText(order.count, order.currentCount, order.type.image);
    }

    public void EnableWrapp()
    {
        row.gameObject.SetActive(false);
        cassaImage.SetActive(false);
        wrappImage.SetActive(true);
    }

    public void EnableCassa()
    {
        row.gameObject.SetActive(false);
        cassaImage.SetActive(true);
        wrappImage.SetActive(false);
    }

    public void DisableAll()
    {
        mainCanvasGroup.DOFade(0, 0.3f);
    }

    private void Update()
    {
        var position = mainCamera.WorldToScreenPoint(_target.position);
        position.y += offset;
        transform.position = position;
    }
}
