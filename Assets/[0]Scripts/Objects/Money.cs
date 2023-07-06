using System;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Floater))]

public class Money : MonoBehaviour
{
    [SerializeField] private bool animate = false;
    [SerializeField] private bool collectFromTrigger = true;
    [SerializeField] private int moneyMultiplier = 1;
    [SerializeField] private float collectAnimationDuration = 0.2f;
    [SerializeField] private AnimationCurve animationCurve;
    [SerializeField] private float power;
    [SerializeField] private float radius;
    [SerializeField] private float upwordPower;

    private Floater _floater = null;
    private Rigidbody _rigidbody;
    private MoneyManager _moneyManager => MoneyManager.Instance;
    
    private float _expiredSeconds = 0;
    private float _progress = 0;
    private float _startYPosition = 0;

    private void Awake()
    {
        _floater = GetComponent<Floater>();
        _floater.Animate = animate;
        _startYPosition = transform.position.y;
    }

    public void Initialize(bool Animate, bool CollectFromTrigger, int MoneyMultiplier = 1, Transform expPosition = null)
    {
        collectFromTrigger = CollectFromTrigger;
        moneyMultiplier = MoneyMultiplier;
        animate = Animate;
        _startYPosition = transform.position.y;
        
        _floater.Animate = animate;

        if (expPosition != null)
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.isKinematic = false;
            var position = expPosition.position;
            position.x += Random.Range(-1f, 1f);
            position.z += Random.Range(-1f, 1f);
            _rigidbody.AddExplosionForce(power, position ,radius,upwordPower);
        }
    }

    public void SetMultiplier(int multiplier)
    {
        moneyMultiplier *= multiplier;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (collectFromTrigger == false)
        {
            return;
        }
        
        if (other.GetComponent<PlayerController>())
        {
            CollectMoney(other.transform);
        }
    }

    public void CollectMoney(Transform player)
    {
        //AudioManager.Instance.PlayAudioByKey("Money_Sound");
        
        _floater.Animate = false;
        transform.DOScale(transform.localScale / 2f, collectAnimationDuration);
        transform.DOMove(player.position, collectAnimationDuration)
            .OnUpdate(Animate)
            .OnComplete(FinishAnimation);
    }
    
    public void CollectMoney(Vector3 playerPosition, float timeToCollect)
    {
        //AudioManager.Instance.PlayAudioByKey("Money_Sound");
        
        _floater.Animate = false;
        transform.DOMove(playerPosition, timeToCollect)
            .OnComplete(SimpleFinish);
    }

    private void Animate()
    {
        _expiredSeconds += Time.deltaTime;
        _progress = _expiredSeconds / collectAnimationDuration;

        var position = transform.position;
        position.y = _startYPosition + animationCurve.Evaluate(_progress);
        transform.position = position;
    }

    private void FinishAnimation()
    {
        VibrationController.Instance.PlayVibration("MoneyCollect_Vibration");

        _progress = 0;
        _expiredSeconds = 0;
        _moneyManager.AddMoney(moneyMultiplier);
        AnalyticsManager.Instance.OnMoneyGained(moneyMultiplier);
        Destroy(gameObject);
    }
    
    private void SimpleFinish()
    {
        _progress = 0;
        _expiredSeconds = 0;
        _moneyManager.AddMoney(moneyMultiplier);
        AnalyticsManager.Instance.OnMoneyGained(moneyMultiplier);
        Destroy(gameObject);
    }
}
