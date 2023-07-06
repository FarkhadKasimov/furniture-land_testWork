using System;
using System.Collections;
using DG.Tweening;
using Pathfinding;
using UnityEngine;
using UnityEngine.UI;

public class WorkerLogBehaviour : MonoBehaviour
{
    private ObjectsHolder _objectHolder;
    private CharacterData _characterData;

    [SerializeField] private float activateTime;
    [SerializeField] private float collectingResourcesTime;
    [SerializeField] private Transform teleportToFactory;
    [SerializeField] private Transform teleportToResources;
    [SerializeField] private Transform warehouseTarget;
    [SerializeField] private Transform idlePosition;
    [SerializeField] private GameObject saw;
    [SerializeField] private Collider sawCollider;
    [SerializeField] private ZoneTrigger zone;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject zzSign;
    [SerializeField] private GameObject ui;
    [SerializeField] private Image fillImage;

    private Transform currentTarget = null;
    private AIPath _aiPath;
    
    private float currentTime = 0f;
    private float currentTimeOnResourcesIsland = 0f;
    private bool activated = false;
    private bool activating = false;
    private bool isOnFactory = true;
    private bool calculateTimeOnResources = false;
    private float fillamount;

    private Coroutine checkRoutine = null;

    private void OnEnable()
    {
        zone.PlayerEnterEvent += OnPlayerEnter;
        zone.PlayerExitEvent += OnPlayerExit;
    }

    private void OnDisable()
    {
        zone.PlayerEnterEvent -= OnPlayerEnter;
        zone.PlayerExitEvent -= OnPlayerExit;
    }

    public void Initialize(bool bought, bool canBuy)
    {
        if (bought == false)
        {
            return;
        }

        _objectHolder = GetComponent<ObjectsHolder>();
        _characterData = GetComponent<CharacterData>();
        _aiPath = GetComponent<AIPath>();
        _objectHolder.SetCharacterData(_characterData);
        fillamount = 1f / activateTime * Time.deltaTime;

        currentTime = activateTime;
        isOnFactory = true;
    }

    public void OnPlayerEnter(PlayerController playerController)
    {
        if (activated)
        {
            return;
        }

        activating = true;
    }
    
    public void OnPlayerExit(PlayerController playerController)
    {
        if (activated)
        {
            return;
        }

        activating = false;
        currentTime = activateTime;
    }
    
    private void Update()
    {
        if (activated)
        {
            if (calculateTimeOnResources)
            {
                if (currentTimeOnResourcesIsland < collectingResourcesTime)
                {
                    currentTimeOnResourcesIsland += Time.deltaTime;
                }
                else
                {
                    FindWorkerTarget();
                    calculateTimeOnResources = false;
                }
            }
            
            return;
        }

        if (activating == false)
        {
            return;
        }

        if (currentTime <= 0)
        {
            ActivateWorker();
        }
        
        currentTime -= Time.deltaTime;
        fillImage.fillAmount += fillamount;
    }

    private void ActivateWorker()
    {
        activated = true;
        activating = false;
        currentTime = activateTime;
        FindWorkerTarget();
        animator.SetBool("Movement", true);
        fillImage.fillAmount = 0;
        zzSign.SetActive(false);
        ui.SetActive(false);
    }

    private void DeactivateWorker()
    {
        print("DEACTIVATED!");
        activated = false;
        activating = false;
        currentTime = activateTime;
        zzSign.SetActive(true);
        ui.SetActive(true);
        transform.DOMove(idlePosition.position, 0.2f);
        transform.DORotate(new Vector3(0, 180, 0), 0.2f);
    }

    private void ShowDeactivationVisual()
    {
        
    }
    
    private void FindWorkerTarget()
    {
        if (activated == false)
        {
            return;
        }
        
        currentTarget = null;

        if (activated && isOnFactory && _objectHolder.ObjectsCountInHand() == 0)
        {
            currentTarget = teleportToResources;
        }
        else if (activated && isOnFactory && _objectHolder.ObjectsCountInHand() > 0)
        {
            currentTarget = warehouseTarget;
        }
        else if (isOnFactory == false && currentTimeOnResourcesIsland >= collectingResourcesTime)
        {
            currentTarget = teleportToFactory;
        }
        else if (isOnFactory == false && currentTimeOnResourcesIsland <= collectingResourcesTime)
        {
            currentTarget = TreeZonesManager.Instance.GentNearestTree(transform.position);
        }

        if (currentTarget == null)
        {
            FindWorkerTarget();
        }
        else
        {
            SetTargetDestination(currentTarget.position);
        }
    }
    
    private void SetTargetDestination(Vector3 destinationPosition)
    {
        if (checkRoutine != null)
        {
            StopCoroutine(checkRoutine);
            checkRoutine = null;
        }
        
        _aiPath.SetPath(null);
        _aiPath.canMove = true;
        _aiPath.maxSpeed = _characterData.MovementSpeed;
        _aiPath.destination = destinationPosition;
        checkRoutine = StartCoroutine(CheckReachDestination());
    }
    
    public void TakeResource(CollectableObject resource, Vector3 scale, bool withAnimation = false)
    {
        var result = _objectHolder.AddToList(resource, scale, withAnimation);
        FindWorkerTarget();
    }

    public int ObjectsCountInHand()
    {
        return _objectHolder.ObjectsCountInHand();
    }
    
    public CollectableObject GiveResource(ObjectSettings resource)
    {
        var result = _objectHolder.RemoveFromList(resource);
        if (_objectHolder.HandsIsEmpty())
        {
            GoToInitialPosition();
        }

        return result;
    }
    
    public CollectableObject GiveResource()
    {
        var result = _objectHolder.RemoveFromList();
        if (_objectHolder.HandsIsEmpty())
        {
            GoToInitialPosition();
        }

        return result;
    }

    private void StopMovement()
    {
        _aiPath.SetPath(null);
        _aiPath.canMove = false;
    }

    private void GoToInitialPosition()
    {
        if (checkRoutine != null)
        {
            StopCoroutine(checkRoutine);
            checkRoutine = null;
        }
        
        activated = false;
        _aiPath.SetPath(null);
        _aiPath.canMove = true;
        _aiPath.maxSpeed = _characterData.MovementSpeed;
        _aiPath.destination = idlePosition.position;
        checkRoutine = StartCoroutine(CheckReachBeginPoint());
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
    
    private IEnumerator CheckReachBeginPoint()
    {
        while (_aiPath.reachedEndOfPath == false)
        {
            if (_aiPath.reachedEndOfPath && !_aiPath.pathPending)
            {
                break;
            }

            yield return null;
        }
        
        animator.SetBool("Movement", false);
        StopMovement();
        DeactivateWorker();

        yield return null;
    }

    private void EnableSaw()
    {
        animator.SetBool("ObjectsInHands", true);
        saw.SetActive(true);
        sawCollider.enabled = true;
    }

    private void DisableSaw()
    {
        animator.SetBool("ObjectsInHands", false);
        saw.SetActive(false);
        sawCollider.enabled = false;
    }
    
    private void RunEventOnReachDestination()
    {
        if (isOnFactory && currentTarget == teleportToResources)
        {
            transform.position = teleportToFactory.position;
            isOnFactory = false;
            calculateTimeOnResources = true;
            currentTimeOnResourcesIsland = 0f;
            EnableSaw();
        }

        if (!isOnFactory && currentTarget == teleportToFactory)
        {
            transform.position = teleportToResources.position;
            isOnFactory = true;
            DisableSaw();
        }

        if (activated == false)
        {
            ShowDeactivationVisual();
        }
        
        FindWorkerTarget();
    }
}
