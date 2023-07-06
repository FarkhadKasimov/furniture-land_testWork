using System;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class CollectableObjectPart : MonoBehaviour
{
    private Vector3 _position;
    private Quaternion _rotation;

    private Rigidbody _rigidbody;
    private Collider _collider;

    private bool _enabled = false;

    private void Start()
    {
        _position = transform.localPosition;
        _rotation = transform.localRotation;

        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();

        _enabled = false;
    }

    public void EnablePart(Vector3 position, float power, float radius, float delay)
    {
        _rigidbody.isKinematic = false;
        _collider.enabled = true;
        _rigidbody.AddExplosionForce(power, position, radius, 5f);
        _enabled = true;
        DOVirtual.DelayedCall(delay, Hide);
    }

    public void DestroyHard()
    {
        Hide();
        _enabled = true;
    }

    public void ResetPart()
    {
        _rigidbody.isKinematic = true;
        _collider.enabled = false;

        transform.localPosition = _position;
        transform.localRotation = _rotation;

        _enabled = false;

        Show();
    }

    public bool IsAvailable()
    {
        return _enabled == false;
    }
    
    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
