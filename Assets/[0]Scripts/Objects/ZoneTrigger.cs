using System;
using DG.Tweening;
using UnityEngine;
using Utils;

public class ZoneTrigger : MonoBehaviour
{
    public event Action<PlayerController> PlayerEnterEvent;
    public event Action<PlayerController> PlayerExitEvent;
    public event Action<WorkerBehaviour> WorkerEnterEvent;
    public event Action<WorkerBehaviour> WorkerExitEvent;
    public event Action<PlayerController> InstrumentEnterEvent;
    public event Action<PlayerController> InstrumentExitEvent;
    
    public event Action<WorkerLogBehaviour> WorkerLogEnterEvent;
    public event Action<WorkerLogBehaviour> WorkerLogExitEvent;

    [SerializeField] private WorkerType workerType;
    [SerializeField] private bool scaleZone = false;
    
    private const float scaleSize = 0.3f;
    private Vector3 _initialScale;

    private void OnEnable()
    {
        PlayerEnterEvent += OnPlayerEnter;
        PlayerExitEvent += OnPlayerExit;
    }

    private void OnDisable()
    {
        PlayerEnterEvent -= OnPlayerEnter;
        PlayerExitEvent -= OnPlayerExit;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController playerController))
        {
            PlayerEnterEvent?.Invoke(playerController);
        }

        else if (other.TryGetComponent(out WorkerBehaviour workerBehaviour))
        {
            if (workerBehaviour.MatchType(workerType))
            {
                WorkerEnterEvent?.Invoke(workerBehaviour);
            }
        }

        if (other.TryGetComponent(out Instrument instrument))
        {
            InstrumentEnterEvent?.Invoke(instrument.Player);
        }

        if (other.TryGetComponent(out WorkerLogBehaviour worker))
        {
            WorkerLogEnterEvent?.Invoke(worker);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerController playerController))
        {
            PlayerExitEvent?.Invoke(playerController);
        }
        
        else if (other.TryGetComponent(out WorkerBehaviour workerBehaviour))
        {
            if (workerBehaviour.MatchType(workerType))
            {
                WorkerExitEvent?.Invoke(workerBehaviour);
            }
        }
        
        if (other.TryGetComponent(out Instrument instrument))
        {
            InstrumentExitEvent?.Invoke(instrument.Player);
        }
        
        if (other.TryGetComponent(out WorkerLogBehaviour worker))
        {
            WorkerLogExitEvent?.Invoke(worker);
        }
    }

    private void OnPlayerEnter(PlayerController playerController)
    {
        _initialScale = transform.localScale;
        ScaleZone(true);
    }

    private void OnPlayerExit(PlayerController playerController)
    {
        ScaleZone(false);
    }

    private void ScaleZone(bool scaleZoneIn)
    {
        if (scaleZone == false)
        {
            return;
        }
        
        var scaleTarget = scaleZoneIn ? _initialScale + Vector3.one * scaleSize : _initialScale;
        transform.DOScaleX(scaleTarget.x, 0.2f);
        transform.DOScaleZ(scaleTarget.z, 0.2f);
    }
}
