using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Utils;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CharacterData))]
[RequireComponent(typeof(ObjectsHolder))]
[RequireComponent(typeof(PlayerInstruments))]
[RequireComponent(typeof(AudioSource))]

public class PlayerController : MonoBehaviour
{
    public event Action ChopHitEvent;
    public PlayerInstruments PlayerInstruments => _playerInstruments;
    
    [SerializeField] private List<Animator> playerAnimators = new List<Animator>();
    [SerializeField] private ParticleSystem moveParticles;
    [SerializeField] private bool showHoldObjectsAnimation = false;

    private SimpleJoystick _joystick => SimpleJoystick.Instance;

    private Rigidbody _rigidbody;
    private CharacterData _playerData;
    private ObjectsHolder _objectsHolder;
    private PlayerInstruments _playerInstruments;
    
    private Vector3 _initialRotation;
    private AudioSource _audioSource;
    
    private bool _blockInput = false;
    private bool _objectTaken = false;
    
    private readonly int Movement = Animator.StringToHash("Movement");
    private readonly int ObjectsInHands = Animator.StringToHash("ObjectsInHands");
    private readonly int Chop = Animator.StringToHash("Chop");
    private int _lastUpgradeSaw = 0;
    private int _lastUpgradeChopp = 0;

    private void Awake()
    {
        _playerData = GetComponent<CharacterData>();
        _rigidbody = GetComponent<Rigidbody>();
        _objectsHolder = GetComponent<ObjectsHolder>();
        _audioSource = GetComponent<AudioSource>();
        _playerInstruments = GetComponent<PlayerInstruments>();

        _lastUpgradeSaw = 0;
        _lastUpgradeChopp = 0;
        
        Application.targetFrameRate = 60;
    }

    private void OnEnable()
    {
        _playerInstruments.fuelUI.FuelEmptyEvent += StopWorking;
    }

    private void OnDisable()
    {
        _playerInstruments.fuelUI.FuelEmptyEvent -= StopWorking;
    }
    
    private void Start()
    {
        _objectsHolder.SetCharacterData(_playerData);
        _initialRotation = transform.localEulerAngles;
    }

    private void FixedUpdate()
    {
        if (_blockInput || gameObject.activeInHierarchy == false)
        {
            return;
        }

        Move();
    }

    public void AddAnimator(Animator animator)
    {
        playerAnimators.Clear();
        playerAnimators.Add(animator);
    }
    
    private void Move()
    {
        var horizontal = _joystick.Horizontal();
        var vertical = _joystick.Vertical();
        var direction = new Vector3(horizontal, 0, vertical).normalized;
        var defaultMagnitude = Mathf.Clamp01(new Vector3(horizontal, 0, vertical).magnitude + 0.3f);

        if (direction.magnitude > 0)
        {
            RotateTowardsDirection(direction);
            MoveTowardsDirection(_joystick.ChangePLayerSpeed ? defaultMagnitude : 1);

            if (direction.magnitude > 0.1f)
            {
                if (moveParticles.isPlaying == false || moveParticles.isEmitting == false)
                {
                    moveParticles.Play();
                    if (ES3.Load("SOUND_SAVEKEY", true))
                    {
                        _audioSource.Play();
                    }
                }
            }
        }
        else
        {
            if (moveParticles.isPlaying || moveParticles.isEmitting)
            {
                moveParticles.Stop();
                _audioSource.Stop();
            }
        }
        
        foreach (var animator in playerAnimators)
        {
            animator.SetFloat(Movement, direction.magnitude);
        }
    }

    private void RotateTowardsDirection(Vector3 direction)
    {
        var targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        transform.DOLocalRotate(_initialRotation + new Vector3(0f, targetAngle, 0f), 0.2f).SetEase(Ease.Linear);
    }
    
    private void MoveTowardsDirection(float magnitude)
    {
        var positionToMove = _rigidbody.position + (transform.forward * magnitude * _playerData.MovementSpeed * Time.fixedDeltaTime);
        _rigidbody.MovePosition(positionToMove);
    }

    public bool TakeResource(CollectableObject collectableObject, Vector3 localScale, bool withAnimation = false)
    {
        var result = _objectsHolder.AddToList(collectableObject, localScale, withAnimation);
        
        if (result)
        {
            ResourcesCounter.Instance.AddResource(collectableObject.Settings);
            
            if (showHoldObjectsAnimation)
            {
                foreach (var animator in playerAnimators)
                {
                    animator.SetBool(ObjectsInHands, true);
                }
            }

            if (ObjectsCountInHand() >= 1)
            {
                Tutorial.Instance.MoveNext(Tutorial.TutorialPath.CollectPrecessedResources);
            }
        }

        return result;
    }

    public CollectableObject GiveResource(ObjectSettings resource)
    {
        var result = _objectsHolder.RemoveFromList(resource);
        if (_objectsHolder.HandsIsEmpty() && showHoldObjectsAnimation)
        {
            foreach (var animator in playerAnimators)
            {
                animator.SetBool(ObjectsInHands, false);
            }
        }
        
        ResourcesCounter.Instance.RemoveResource(resource);

        return result;
    }
    
    public CollectableObject GiveResource()
    {
        var result = _objectsHolder.RemoveFromList();
        if (_objectsHolder.HandsIsEmpty() && showHoldObjectsAnimation)
        {
            foreach (var animator in playerAnimators)
            {
                animator.SetBool(ObjectsInHands, false);
            }
        }
        
        ResourcesCounter.Instance.RemoveResource(result.Settings);

        return result;
    }
    
    public bool CanTake()
    {
        if (_objectsHolder.HandsIsFull())
        {
            ResourcesCounter.Instance.ShuffleText();
        }
        
        return !_objectsHolder.HandsIsFull();
    }

    public int ObjectsCountInHand()
    {
        return _objectsHolder.ObjectsCountInHand();
    }

    public void MoveToPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void StartWorking(InstrumentType type)
    {
        var upgradableValue = Mathf.CeilToInt(_playerData.fuelCapacity * 0.1f);
        _playerInstruments.SetupInstrument(type, this, _playerData.fuelCapacity + UpgradeSaves.Instance.playerSawFuel * upgradableValue);
        if (type == InstrumentType.Axe)
        {
            foreach (var animator in playerAnimators)
            {
                animator.SetBool(Chop, true);
                animator.speed = _playerData.CollectingSpeed;
            }
        }
    }

    public void BuffPlayer()
    {
        _lastUpgradeSaw = UpgradeSaves.Instance.playerSawScaleUpgrade;
        _lastUpgradeChopp = UpgradeSaves.Instance.playerChoppingSpeedUpgrades;

        UpgradeSaves.Instance.playerSawScaleUpgrade = 5;
        UpgradeSaves.Instance.playerChoppingSpeedUpgrades = 100;

        StartWorking(InstrumentType.Axe);
    }

    public void Debuff()
    {
        if (_lastUpgradeChopp == 0 && _lastUpgradeChopp == 0)
        {
            return;
        }
        
        UpgradeSaves.Instance.playerSawScaleUpgrade = _lastUpgradeSaw;
        UpgradeSaves.Instance.playerChoppingSpeedUpgrades = _lastUpgradeChopp;

        _lastUpgradeSaw = 0;
        _lastUpgradeChopp = 0;
    }
    
    public void StopWorking()
    {
        _playerInstruments.HideInstrument();
        SetPlayerSpeed(5);
        foreach (var animator in playerAnimators)
        {
            animator.SetBool(Chop, false);
            animator.speed = 1;
        }
    }

    public void ChopHit()
    {
        ChopHitEvent?.Invoke();
    }

    public void SetPlayerSpeed(float speed)
    {
        _playerData.MovementSpeed = speed;
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            if (_lastUpgradeChopp != 0 && _lastUpgradeChopp != 0)
            {
                Debuff();
            }
        }
    }

    private void OnApplicationQuit()
    {
        if (_lastUpgradeChopp != 0 && _lastUpgradeChopp != 0)
        {
            Debuff();
        }
    }
}
