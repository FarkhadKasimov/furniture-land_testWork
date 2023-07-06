using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class RagDollController : MonoBehaviour
{
    [SerializeField] private Transform hipsRoot;
    [SerializeField] private Animator anim = null;
    [SerializeField] private bool needDisableMainColliderAndRigidbody = true;
    
    private Collider mainCollider;
    private Rigidbody mainRigidBody;
    private Rigidbody[] rigidBodies;
    private Collider[] colliders;

    [Button]
    [GUIColor(0, 1, 0)]
    private void EnableRagDoll()
    {
        ChangeRagDollState(true);
    }

    private void Awake()
    {
        anim ??= GetComponent<Animator>();
        mainCollider = GetComponent<Collider>();
        mainRigidBody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        rigidBodies = hipsRoot.GetComponentsInChildren<Rigidbody>();
        colliders = hipsRoot.GetComponentsInChildren<Collider>();
    }

    public void ChangeRagDollState(bool state)
    {
        anim.enabled = !state;

        if (needDisableMainColliderAndRigidbody)
        {
            mainCollider.enabled = !state;
            mainRigidBody.isKinematic = state;
        }

        foreach (Rigidbody rb in rigidBodies)
        {
            rb.isKinematic = !state;
        }

        foreach (Collider col in colliders)
        {
            col.enabled = state;
        }
    }

    public void ApplyForceToRagDoll()
    {
        foreach (Rigidbody rb in rigidBodies)
        {
            mainRigidBody.AddForce(Vector3.up * 10f, ForceMode.Impulse);
            rb.AddForce(Vector3.up * 10f, ForceMode.Impulse);
        }
    }
}
