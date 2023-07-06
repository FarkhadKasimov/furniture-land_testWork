using System;
using System.Collections;
using DG.Tweening;
using Pathfinding;
using TMPro;
using UnityEngine;
using Utils;

public class Manager : MonoBehaviour
{
    public event Action DespawnEvent;
    public event Action PressedEvent;
    
    [SerializeField] private Animator animator;
    [SerializeField] private Vector3 makeBoostRotation;
    [SerializeField] private ParticleSystem managerParticle;
    [SerializeField] private TextMeshProUGUI infoLable;
    
    public ManagerType Type => _type;
    private ManagerType _type;
    
    private Transform _currentTarget = null;
    private AIPath _aiPath;
    private ManagerState _currentState;

    private Transform _baseTarget;
    private Transform _boostTarget;
    private Transform _despawnTarget;

    private readonly int Movement = Animator.StringToHash("Movement");
    private readonly int MakeBoost = Animator.StringToHash("MakeBoost");

    public void Initialize(ManagerType type, Transform baseTarget, Transform boost, Transform despawn)
    {
        _aiPath = GetComponent<AIPath>();
        _type = type;
        _currentState = ManagerState.GoToMagazine;

        _baseTarget = baseTarget;
        _boostTarget = boost;
        _despawnTarget = despawn;

        infoLable.text = "<sprite=" + (int) _type + "> x2"; 
        
        FindTargetAndSendDestination();
    }

    public void GoToMakeBoostState()
    {
        _currentState = ManagerState.MakeBoost;
        managerParticle.gameObject.SetActive(false);
        FindTargetAndSendDestination();
    }

    public void GoToDespawn()
    {
        _currentState = ManagerState.Despawn;
        FindTargetAndSendDestination();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            print("ENTER!");
            PressedEvent?.Invoke();
        } 
    }

    private void FindTargetAndSendDestination()
    {
        switch (_currentState)
        {
            case ManagerState.GoToMagazine:
                _currentTarget = _baseTarget;
                break;
            case ManagerState.MakeBoost:
                _currentTarget = _boostTarget;
                break;
            case ManagerState.Despawn:
                _currentTarget = _despawnTarget;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        SetTargetDestination(_currentTarget.position);
    }
    
    private void SetTargetDestination(Vector3 destinationPosition)
    {
        _aiPath.SetPath(null);
        _aiPath.canMove = true;

        animator.SetBool(Movement, true);
        _aiPath.destination = destinationPosition;
        StartCoroutine(CheckReachDestination());
    }
    
    private IEnumerator CheckReachDestination()
    {
        while (true)
        {
            if (_aiPath.reachedEndOfPath && !_aiPath.pathPending)
            {
                break;
            }

            yield return null;
        }
        
        StopMovement();
        RunEventOnReachDestination();

        yield return null;
    }
    
    private void StopMovement()
    {
        _aiPath.SetPath(null);
        _aiPath.canMove = false;
        animator.SetBool(Movement, false);
        animator.SetBool(MakeBoost, false);
    }
    
    private void RunEventOnReachDestination()
    {
        switch (_currentState)
        {
            case ManagerState.GoToMagazine:
                transform.DORotate(new Vector3(0, 180, 0), 0.5f);
                break;
            case ManagerState.MakeBoost:
                MakeBoostAnimation();
                break;
            case ManagerState.Despawn:
                Despawn();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Despawn()
    {
        DespawnEvent?.Invoke();
        Destroy(gameObject, 0.5f);
    }

    private void MakeBoostAnimation()
    {
        animator.SetBool(MakeBoost, true);
        transform.DORotate(makeBoostRotation, 0.5f);
    }
    
    private enum ManagerState
    {
        GoToMagazine,
        MakeBoost,
        Despawn
    }
}
