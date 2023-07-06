using System;
using UnityEngine;
using Utils;

public class Tree : RespurcesGenerator
{
    [SerializeField] private float collectigDelay = 0.2f;
    [SerializeField] private ParticleSystem collectingParticle;
    [SerializeField] private ParticleSystem collectingParticle_2;

    private MaterialPropertyBlock _materialPropertyBlock;
    
    public override void Initialize(float time, int resources, Color color)
    {
        base.Initialize(time, resources, color);
        _materialPropertyBlock = new MaterialPropertyBlock();
        var renderer = collectingParticle_2.GetComponent<ParticleSystemRenderer>();
        renderer.GetPropertyBlock(_materialPropertyBlock);
        _materialPropertyBlock.SetColor("_EmissionColor", color);
        renderer.SetPropertyBlock(_materialPropertyBlock);
    }

    public override void OnPlayerEnter(PlayerController controller)
    {
        if (controller.PlayerInstruments.fuelUI.CanCollectTree() == false)
        {
            controller.SetPlayerSpeed(5);
            return;
        }
        
        base.OnPlayerEnter(controller);

        if (HasAnyPart() == false)
        {
            return;
        }

        if (collectingRoutine == null)
        {
            collectingRoutine = 
                StartCoroutine(CollectResource());
            collectingParticle.Play();
            collectingParticle_2.Play();
        }
    }
    
    public override void OnPlayerExit(PlayerController controller)
    {
        base.OnPlayerExit(controller);
        collectingParticle.Stop();
        collectingParticle_2.Stop();
    }

    public override void OnWorkerEnter(WorkerLogBehaviour worker)
    {
        base.OnWorkerEnter(worker);
        if (HasAnyPart() == false)
        {
            return;
        }
        if (collectingRoutine == null)
        {
            collectingRoutine = 
                StartCoroutine(CollectResource());
            collectingParticle.Play();
            collectingParticle_2.Play();
        }
    }

    public override void OnWorkerExit(WorkerLogBehaviour worker)
    {
        base.OnWorkerExit(worker);
        collectingParticle.Stop();
        collectingParticle_2.Stop();
    }

    public override void GiveResource(int count = 1)
    {
        base.GiveResource(count);
        collectingParticle.Stop();
        collectingParticle_2.Stop();
    }
}
