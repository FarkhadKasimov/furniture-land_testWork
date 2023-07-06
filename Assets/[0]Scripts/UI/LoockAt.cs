using System;
using DG.Tweening;
using UnityEngine;

public class LoockAt : MonoBehaviour
{
    private Transform cameraTransform;

    private void Awake()
    {
        cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        if (cameraTransform == null)
        {
            return;
        }

        transform.DOLookAt(cameraTransform.position, Time.deltaTime);
    }
}
