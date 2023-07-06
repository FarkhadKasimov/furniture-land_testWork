using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Pathfinding;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AIPath))]
public class Customer : MonoBehaviour
{
    public event Action<Customer, FactoryShop> CustomerRemoveEvent;
    public List<WantedResource> CustomerOrder => _order;

    [SerializeField] private Renderer customerBodyRenderer;
    [SerializeField] private Transform headObjectHolder;
    [SerializeField] private Transform resourcesHolder;
    [SerializeField] private Animator customerAnimator;
    [SerializeField] private float checkDelay = 0.2f;
    [SerializeField] private Image progressImage;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Money moneyPrefab;

    private MaterialPropertyBlock _materialProperty;
    private readonly int Color1 = Shader.PropertyToID("_Color");

    private Vector3 _beginPosition;
    private AIPath _aiPath;
    private List<WantedResource> _order = new List<WantedResource>();
    private List<CollectableObject> _orderedObjects = new List<CollectableObject>();

    public int CurrentItemsCount = 0;
    
    private int _currentOrder = 0;
    private int _currentOrderItemCount = 0;
    private bool _wantWrapping = true;
    private bool _targetReachedDestination = false;
    private Coroutine curentCheck;
    private CustomerState _currentState = CustomerState.GoToTarget;
    private readonly int Movement = Animator.StringToHash("Movement");
    private readonly int Holder = Animator.StringToHash("ObjectsInHands");
    private Coroutine fillCoroutine;
    private Vector2Int _dimention = new Vector2Int(2, 2);
    private Camera _camera;
    private FactoryShop _shop;

    private TargetOrderUI ui;

    public void SetupCustomer(Vector3 position, List<WantedResource> order, FactoryShop shop)
    {
        position.y = transform.position.y;
        _beginPosition = position;
        transform.position = position;
        _camera = Camera.main;
        _shop = shop;

        _order = order;
        _aiPath = GetComponent<AIPath>();

        _currentOrder = 0;

        SetCustomerDestination();
        curentCheck = StartCoroutine(CheckCustomersState());

        ui = MainInfoUI.Instance.GetOrderUI();
        ui.Initialize(_order[_currentOrder], transform);
    }

    public void SetupCustomer(Vector3 position, List<WantedResource> order, Color bodyColor, FactoryShop shop)
    {
        SetupCustomer(position, order, shop);
        ChangeBodyColor(bodyColor);
    }
    
    public void SetupCustomer(Vector3 position, List<WantedResource> order, GameObject head, FactoryShop shop)
    {
        SetupCustomer(position, order, shop);
        ChangeHead(head);
    }
    
    public void SetupCustomer(Vector3 position, List<WantedResource> order, Color bodyColor, GameObject head, FactoryShop shop)
    {
        SetupCustomer(position, order, shop);
        ChangeBodyColor(bodyColor);
        ChangeHead(head);
    }

    public void GoToExit()
    {
        StartCoroutine(CheckCustomerExitState());
    }

    public void GoToCassa()
    {
        _wantWrapping = false;
        SetCustomerDestination();
        _currentState = CustomerState.Wrapping;
        StartCoroutine(CheckCustomerWrappingState());
    }

    public void FillProgress(float fillAmount)
    {
        fillCoroutine = StartCoroutine(FillImage(fillAmount));
    }

    public void StopFillProgress()
    {
        if (fillCoroutine != null)
        {
            StopCoroutine(fillCoroutine);
            fillCoroutine = null;
        }

        progressImage.fillAmount = 0;
    }

    private IEnumerator FillImage(float fillAmount)
    {
        while (progressImage.fillAmount < 1)
        {
            progressImage.fillAmount += fillAmount;
            yield return null;
        }

        yield return null;
    }
    
    public CollectableObject GetOrderedResource()
    {
        var resource = _orderedObjects[_orderedObjects.Count - 1];
        _orderedObjects.Remove(resource);
        return resource;
    }

    public void MoveObjectToHand(Transform obj)
    {
        obj.transform.SetParent(resourcesHolder);
        obj.transform.DOMove(resourcesHolder.position, 0.3f);
    }
    
    public void CheckForWrapping(float wrappingChance)
    {
        //var chance = Random.Range(0f, 1f);
        //_wantWrapping = chance < wrappingChance;
        _wantWrapping = false;
    }

    private void ChangeBodyColor(Color bodyColor)
    {
        _materialProperty = new MaterialPropertyBlock();
        customerBodyRenderer.GetPropertyBlock(_materialProperty);
        _materialProperty.SetColor(Color1, bodyColor);
        customerBodyRenderer.SetPropertyBlock(_materialProperty);
    }

    private void ChangeHead(GameObject headObject)
    {
        Instantiate(headObject, headObjectHolder);
    }

    private void SetCustomerDestination()
    {
        Vector3 position = Vector3.zero;
        
        if (_currentOrder < _order.Count)
        {
            var resource = _order[_currentOrder].type;
            position = FactorySaves.Instance.GetCustomerTargetByResource(resource, _shop).position;
        }
        else
        {
            ui.EnableCassa(); 
            position = _shop.cassaObject.GetCustomerTarget();
        }

        _aiPath.canMove = true;
        _aiPath.SetPath(null);
        _aiPath.destination = position;
        customerAnimator.SetBool(Movement, true);
        _currentState = CustomerState.GoToTarget;
    }
    
    private Transform GetTargetForOrder()
    {
        if (_currentOrder < _order.Count)
        {
            var resource = _order[_currentOrder].type;
            return FactorySaves.Instance.GetCustomerTargetByResource(resource, _shop);
        }

        ui.EnableCassa(); 
        return null;
    }

    private void GetCash()
    {
        int moneyCount = 0;

        foreach (var order in _order)
        {
            moneyCount += order.type.cost * order.count;
        }

        if (IsVisible() == false)
        {
            MoneyManager.Instance.AddMoney(moneyCount);
        }
        else
        {
            StartCoroutine(SpawnMoney(moneyCount));
        }
        
        GoToExit();
    }

    private IEnumerator SpawnMoney(int moneyCount)
    {
        for (int i = 0; i < moneyCount; i++)
        {
            var money = Instantiate(moneyPrefab, transform.position, transform.rotation);
            money.Initialize(false, true, 1, transform);
            yield return new WaitForSeconds(0.1f);
        }

        yield return null;
    }

    private bool IsVisible()
    {
        return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(_camera), customerBodyRenderer.bounds);
    }
    
    private IEnumerator CheckCustomersState()
    {
        while (_currentState != CustomerState.WaitingForBuy)
        {
            switch (_currentState)
            {
                case CustomerState.GoToTarget:
                    CheckTargetPlace();
                    break;
                case CustomerState.TakeProducts:
                    CheckResourceCollection();
                    break;
            }
        
            yield return new WaitForSeconds(checkDelay);
        }

        yield return null;
    }

    private IEnumerator CheckCustomerSellingState()
    {
        while (_currentState == CustomerState.WaitingForBuy)
        {
            if (_aiPath.reachedEndOfPath && !_aiPath.pathPending)
            {
                StopMoving();
                _shop.cassaObject.AddCustomerToQueue(this);
                _currentState = CustomerState.GoToExit;
            }
            
            yield return new WaitForSeconds(checkDelay);
        }

        yield return null;
    }
    
    private IEnumerator CheckCustomerWrappingState()
    {
        _aiPath.SetPath(null);
        //_aiPath.destination = _magazineManager.Cassa.GetCustomerTarget().position;
        ui.EnableCassa();
        while (_currentState == CustomerState.Wrapping)
        {
            if (_aiPath.reachedEndOfPath && !_aiPath.pathPending)
            {
                StopMoving();
                //_magazineManager.Cassa.AddCustomerToQueue(this);
                _currentState = CustomerState.GoToExit;
            }
            
            yield return new WaitForSeconds(checkDelay);
        }

        yield return null;
    }

    private IEnumerator CheckCustomerExitState()
    {
        ui.DisableAll();       
        _currentState = CustomerState.GoToExit;
        _aiPath.canMove = true;
        _aiPath.SetPath(null);
        var destination = _beginPosition;
        destination.x -= 3f;
        _aiPath.destination = destination;
        customerAnimator.SetBool(Movement, true);
        
        while (_currentState == CustomerState.GoToExit)
        {
            if (_aiPath.reachedEndOfPath && !_aiPath.pathPending)
            {
                StopMoving();
                Destroy(ui.gameObject);
                CustomerRemoveEvent?.Invoke(this, _shop);
                Destroy(gameObject);
            }
            
            yield return new WaitForSeconds(checkDelay);
        }

        yield return null;
    }
    
    private void CheckTargetPlace()
    {
        if (_aiPath.reachedEndOfPath && !_aiPath.pathPending)
        {
            StopMoving();
            _currentState = CustomerState.TakeProducts;
        }
    }

    private void StopMoving()
    {
        _aiPath.canMove = false;
        customerAnimator.SetBool(Movement, false);
    }

    private void CheckResourceCollection()
    {
        if (_currentOrder >= _order.Count && _currentState != CustomerState.WaitingForBuy)
        {
            SetCustomerDestination();
            _currentState = CustomerState.WaitingForBuy;
            StopCoroutine(curentCheck);
            StartCoroutine(CheckCustomerSellingState());

            return;
        }

        if (_currentOrderItemCount >= _order[_currentOrder].count)
        {
            _order[_currentOrder].orderCompleted = true;
            _currentOrder += 1;
            _currentOrderItemCount = 0;

            SetCustomerDestination();
            
            _currentState = CustomerState.GoToTarget;
        }
        else
        {
            var magazine = FactorySaves.Instance.GetFabriqueByType(_order[_currentOrder].type, _shop);
            var resource = magazine.GetResource();

            if (resource != null)
            {
                PositionateOrder(resource);

                _currentOrderItemCount += 1;
                CurrentItemsCount += 1;
                _order[_currentOrder].currentCount = _currentOrderItemCount;
            }
        }

        var currentOder = Mathf.Clamp(_currentOrder, 0, _order.Count - 1);
        ui.ActualizeRow(_order[currentOder]);
    }

    private void PositionateOrder(CollectableObject resource)
    {
        resource.transform.SetParent(resourcesHolder);
        
        var positionToMove = Vector3.zero;

        int rowsUpCount = _orderedObjects.Count / (_dimention.x * _dimention.y);

        int columnIndex = _orderedObjects.Count / _dimention.y;
        columnIndex -= rowsUpCount * _dimention.x;

        int rowIndex = _orderedObjects.Count - (columnIndex * _dimention.y);
        rowIndex -= rowsUpCount * _dimention.y * _dimention.x;

        positionToMove.x += offset.x * columnIndex;
        positionToMove.z += offset.z * rowIndex;
        positionToMove.y += offset.y * rowsUpCount;

        resource.SetPosition(positionToMove);
        resource.RotateToBaseValue(0.5f);
        _orderedObjects.Add(resource);
    }
}
