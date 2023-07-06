using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PurchaserInfo : MonoBehaviour
{
    public event Action BuyEvent;
    public event Action<int> BuyCancelationEvent;
    
    [SerializeField] private TextMeshPro priceLable;
    [SerializeField] private float delayTime = 0.1f;
    [SerializeField] private UnityEvent tutorialEvent;
    
    private bool _exitTrigger = false;
    private bool _bought = false;
    private Vector3 _initialScale = Vector3.zero;
    private int _currentAddedMoney = 0;
    private int _price = 0;
    private float _delayTime = 0;
    private Coroutine _buyRoutine;

    private MoneyManager _moneyManager => MoneyManager.Instance;

    public void Initialize(int price, int currentAddedMoney = 0)
    {
        gameObject.SetActive(true);
        _initialScale = transform.localScale;
        _price = price;
        _currentAddedMoney = currentAddedMoney;
        priceLable.text = "<sprite=0>\n" + (_price - _currentAddedMoney);
        _buyRoutine = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            _buyRoutine = StartCoroutine(BuyProcess());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            StopRoutine();
            
            transform.DOScale(_initialScale, 0.2f);
            _exitTrigger = true;
            if (_bought == false)
            {
                BuyCancelationEvent?.Invoke(_currentAddedMoney);
            }
        }
    }

    private IEnumerator BuyProcess()
    {
        transform.DOScale(_initialScale + (Vector3.one * 0.2f), 0.2f);
        _exitTrigger = false;
        
        yield return new WaitForSeconds(0.5f);

        _delayTime = delayTime;
        
        while (true)
        {
            if (_moneyManager.SpentMoney(1, "new_factory_element", "factory_purchase") == false)
            {
                break;
            }
            
            _currentAddedMoney += 1;
            if (_currentAddedMoney == _price)
            {
                priceLable.text = "<sprite=0>\n" + 0;
                break;
            }
            
            priceLable.text = "<sprite=0>\n" + (_price - _currentAddedMoney);
            
            yield return new WaitForSeconds(_delayTime);
            if (_delayTime > 0)
            {
                _delayTime -= Time.deltaTime;
            }
        }

        if (_currentAddedMoney == _price)
        {
            BuyEvent?.Invoke();
            tutorialEvent.Invoke();
            _bought = true;
            
            VibrationController.Instance.PlayVibration("BuyNewItem_Vibration");
        }

        StopRoutine();

        yield return null;
    }

    private void StopRoutine()
    {
        if (_buyRoutine != null)
        {
            StopCoroutine(_buyRoutine);
            _buyRoutine = null;
        }
    }
}
