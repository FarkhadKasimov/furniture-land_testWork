using System;
using System.Collections;
using DG.Tweening;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;

public class RespurcesGenerator : MonoBehaviour
{
    public event Action<RespurcesGenerator> CuttedTreeEvent;
    
    [SerializeField] private bool automaticRespawn = false;
    [ShowIf(nameof(automaticRespawn))] 
    [SerializeField] private float respawnDelay = 1f;
    public ZoneTrigger trigger = null;
    [SerializeField] private MMFeedbacks respawnFeedback;
    [SerializeField] private int resourcesCount;
    [SerializeField] private int givingResourcesAtHit = 1;
    [SerializeField] private int destroingPartsAtHit = 2;
    public CollectableObjectPart[] objectParts;
    [SerializeField] private float partDestroyingPower = 5f;
    [SerializeField] private float partDestroyingRadius = 5f;
    [SerializeField] private float partHideDelay = 3f;
    [SerializeField] private CollectableObject resourceToGenerate;
    [SerializeField] private Transform resourceGenerationTransform;
    [SerializeField] private MMWiggle wiggleEffect;
    //[SerializeField] private ParticleSystem collectingResourceParticle;

    private int _currentResourcesCount = 0;
    public Coroutine collectingRoutine = null;
    private PlayerController _controller;
    private WorkerLogBehaviour _worker;
    private MaterialPropertyBlock _materialPropertyBlock;
    private float playerSpeedWhenCollecting = 0f;
    private bool playerCollecting = false;
    private bool workerCollecting = false;

    private float timeToCollect;

    public virtual void Initialize(float time, int resources, Color color)
    {
        timeToCollect = time;
        resourcesCount = resources;
        var tmp = Mathf.Clamp(time - UpgradeSaves.Instance.playerChoppingSpeedUpgrades * 0.5f, 0f, 2f);
        playerSpeedWhenCollecting = Mathf.Clamp(2f - tmp, 0.3f, 2f);
        
        trigger.InstrumentEnterEvent += OnPlayerEnter;
        trigger.InstrumentExitEvent += OnPlayerExit;
        trigger.WorkerLogEnterEvent += OnWorkerEnter;
        trigger.WorkerLogExitEvent += OnWorkerExit;

        foreach (var part in objectParts)
        {
            var renderer = part.GetComponent<Renderer>();
            _materialPropertyBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(_materialPropertyBlock);
            
            _materialPropertyBlock.SetColor("_EmissionColor", color);
            renderer.SetPropertyBlock(_materialPropertyBlock);
        }

        AddResources();
    }

    public void AddResources()
    {
        _currentResourcesCount = resourcesCount;
    }
    
    private void OnDisable()
    {
        trigger.InstrumentEnterEvent -= OnPlayerEnter;
        trigger.InstrumentExitEvent -= OnPlayerExit;
        trigger.WorkerLogEnterEvent -= OnWorkerEnter;
        trigger.WorkerLogExitEvent -= OnWorkerExit;
        
        OnDisableEvent();
    }
    
    public virtual void OnPlayerEnter(PlayerController controller)
    {
        if (collectingRoutine != null)
        {
            return;
        }
        
        _controller = controller;
        playerCollecting = true;
    }

    public virtual void OnPlayerExit(PlayerController controller)
    {
        if (playerCollecting == false)
        {
            return;
        }
        if (collectingRoutine != null)
        {
            StopCollectingResource();
        }

        playerCollecting = false;
        _controller = null;
    }

    public virtual void OnWorkerEnter(WorkerLogBehaviour worker)
    {
        if (collectingRoutine != null)
        {
            return;
        }
        
        _worker = worker;
        workerCollecting = true;
    }

    public virtual void OnWorkerExit(WorkerLogBehaviour worker)
    {
        if (workerCollecting == false)
        {
            return;
        }
        
        if (collectingRoutine != null)
        {
            StopCollectingResource();
        }

        workerCollecting = false;
        _worker = null;
    }
    
    public virtual void OnDisableEvent()
    {
        
    }

    public virtual void SpawnResources()
    {
        if (HasAnyResources())
        {
            return;
        }

        foreach (var part in objectParts)
        {
            part.ResetPart();
        }

        AddResources();
    }

    public virtual void GiveResource(int count = 1)
    {
        if (HasAnyResources() == false)
        {
            if(_controller != null)
                _controller.SetPlayerSpeed(5);
            return;
        }

        if (_controller != null)
        {
            if (_controller.PlayerInstruments.fuelUI.CanCollectTree() == false)
            {
                _controller.SetPlayerSpeed(5);
                return;
            }
        }
        
        var generateResourceCount = count;
        
        if (_currentResourcesCount < count)
        {
            generateResourceCount = _currentResourcesCount;
        }

        for (int i = 0; i < generateResourceCount; i++)
        {
            if (_controller != null)
            {
                if (_controller.CanTake() == false)
                {
                    break;
                }
            
                var resource = Instantiate(resourceToGenerate, resourceGenerationTransform.position, Quaternion.identity);
                _controller.TakeResource(resource, resource.transform.localScale, true);
            }
            else if (_worker != null)
            {
                var resource = Instantiate(resourceToGenerate, resourceGenerationTransform.position, Quaternion.identity);
                _worker.TakeResource(resource, resource.transform.localScale, true);
            }
        }
        
        _currentResourcesCount -= generateResourceCount;
        if (HasAnyResources() == false && automaticRespawn)
        {
            DOVirtual.DelayedCall(respawnDelay, SpawnResources);
        }

        if (HasAnyResources() == false)
        {
            if(_controller != null)
                _controller.SetPlayerSpeed(5);
        }
    }

    public virtual void HitResource()
    {
        var currentParts = 0;
        var index = 0;
        if(_controller != null)
            _controller.SetPlayerSpeed(playerSpeedWhenCollecting);

        while (currentParts < destroingPartsAtHit)
        {
            if (index > objectParts.Length - 1)
            {
                break;
            }
            
            if (objectParts[index].IsAvailable())
            {
                objectParts[index].EnablePart(transform.position, partDestroyingPower, partDestroyingRadius, partHideDelay);
                //collectingResourceParticle.Play();
                currentParts += 1;
            }

            index += 1;
        }
    }
    
    public bool HasAnyResources()
    {
        return _currentResourcesCount > 0;
    }

    public bool HasAnyPart()
    {
        var hasAnyPart = false;
        foreach (var objectPart in objectParts)
        {
            if (objectPart.IsAvailable())
            {
                hasAnyPart = true;
                break;
            }
        }

        return hasAnyPart;
    }

    public void DestroyTree()
    {
        foreach (var objectPart in objectParts)
        {
            objectPart.DestroyHard();
        }
    }
    
    public IEnumerator CollectingResource(float delayTime)
    {
        var delay = new WaitForSeconds(delayTime);
        
        while (HasAnyPart() && CanCollect())
        {
            wiggleEffect.WigglePosition(delayTime);
            yield return delay;
            wiggleEffect.WigglePosition(delayTime);
            yield return delay;
            HitResource();
            yield return null;
        }
        GiveResource(givingResourcesAtHit);
        Tutorial.Instance.MoveNext(Tutorial.TutorialPath.CollectResources);
    }

    private bool CanCollect()
    {
        if (_controller != null)
        {
            return _controller.PlayerInstruments.fuelUI.CanCollectTree();
        }

        if (_worker != null)
        {
            return true;
        }

        return false;
    }
    
    public IEnumerator CollectResource()
    {
        if (_controller != null)
            _controller.SetPlayerSpeed(playerSpeedWhenCollecting);

        var delayTime = timeToCollect / objectParts.Length;
        var time = Mathf.Clamp(delayTime - (UpgradeSaves.Instance.playerChoppingSpeedUpgrades * 0.5f), 0.05f, delayTime);
        var delay = new WaitForSeconds(time);

        while (HasAnyPart() && CanCollect())
        {
            wiggleEffect.WigglePosition(time);
            VibrationController.Instance.PlayVibration("CutTree");
            yield return delay;
            wiggleEffect.WigglePosition(time);
            yield return delay;
            HitResource();
            yield return null;
        }
        
        Tutorial.Instance.MoveNext(Tutorial.TutorialPath.CollectResources);
        GiveResource(resourcesCount);
        CuttedTreeEvent?.Invoke(this);
    } 

    private void StopCollectingResource()
    {
        StopCoroutine(collectingRoutine);
        collectingRoutine = null;
        if (_controller != null)
            _controller.SetPlayerSpeed(5);
    }
}
