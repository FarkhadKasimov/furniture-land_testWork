using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class ObjectsHolder : MonoBehaviour
{
    [SerializeField] private float objectsDistanceY = 0.2f;
    [SerializeField] private float objectsDistanceZ = 0.2f;
    [SerializeField] private Transform listBeginPosition;
    [SerializeField] private bool showCountText = true;
    [ShowIf(nameof(showCountText))] [SerializeField] private TextMeshPro capacityText;
    [SerializeField] private bool playVibration = false;
    
    private CharacterData _data;
    private List<CollectableObject> _objects = new List<CollectableObject>();

    public void SetCharacterData(CharacterData data)
    {
        _data = data;
    }

    public void UpdateBeginPosition(Transform beginPosition)
    {
        listBeginPosition = beginPosition;
    }
    
    public bool HandsIsEmpty()
    {
        return _objects.Count == 0;
    }

    public bool HandsIsFull()
    {
        return _objects.Count == _data.ActualInventoryCapacity();
    }

    public bool AddToList(CollectableObject collectableObject, Vector3 localScale, bool withAnimation = false)
    {
        if (HandsIsFull())
        {
            ResourcesCounter.Instance.ShuffleText();
            return false;
        }

        _objects.Add(collectableObject);
        
        if (withAnimation)
        {
            var rotationRange = Random.Range(0f, 360f);
            var movePosition = collectableObject.transform.position;
            movePosition.z += Random.Range(-1f, 1f);
            movePosition.x += Random.Range(-1f, 1f);
            movePosition.y += Random.Range(2.5f, 3f);

            collectableObject.SetScale(Vector3.zero);
            collectableObject.transform.SetParent(listBeginPosition);
            
            collectableObject.transform.DORotate(Vector3.one * rotationRange, 0.35f);
            collectableObject.transform.DOScale(localScale, 0.35f).SetEase(Ease.Linear);
            collectableObject.transform.DOMove(movePosition, 0.35f).SetEase(Ease.OutCubic)
                .OnComplete(() =>
                {
                    collectableObject.RotateToBaseValue(0.3f);
                    SortCollection(0.35f);
                });
        }
        else
        {
            collectableObject.transform.SetParent(listBeginPosition);
            collectableObject.SetScale(localScale);
            collectableObject.RotateToBaseValue(0.5f);
            SortCollection();
        }

        if (playVibration)
        {
            VibrationController.Instance.PlayVibration("CollectObject_Vibration");
            //AudioManager.Instance.PlayAudioByKey("CollectObject_Sound");
        }

        ActualizeObjectsCountText(_objects.Count);
        return true;
    }

    public int ObjectsCountInHand()
    {
        return _objects.Count;
    }
    
    public CollectableObject RemoveFromList(ObjectSettings resource)
    {
        if (HandsIsEmpty())
        {
            return null;
        }

        for (int i = 0; i < _objects.Count; i++)
        {
            if (_objects[i].PropertyMatch(resource))
            {
                var resourceInHand = _objects[i];
                _objects.Remove(resourceInHand);
                SortCollection();
                
                ActualizeObjectsCountText(_objects.Count);
                
                if (playVibration)
                {
                    VibrationController.Instance.PlayVibration("PutObject_Vibration");
                    //AudioManager.Instance.PlayAudioByKey("CollectObject_Sound");
                }
                
                return resourceInHand;
            }
        }
        
        return null;
    }
    
    public CollectableObject RemoveFromList()
    {
        if (HandsIsEmpty())
        {
            return null;
        }

        var resourceInHand = _objects[0];
        _objects.Remove(resourceInHand);
        SortCollection();
        
        ActualizeObjectsCountText(_objects.Count);
        
        if (playVibration)
        {
            VibrationController.Instance.PlayVibration("PutObject_Vibration");
                //AudioManager.Instance.PlayAudioByKey("CollectObject_Sound");
        }
        
        return resourceInHand;
    }

    public ObjectSettings GetLastObjectSettings()
    {
        return _objects[0].Settings;
    }
    
    private void SortCollection(float duration = 0.2f)
    {
        var beginPosition = Vector3.zero;
        var inRowCount = 0;
        
        for (int i = 0; i < _objects.Count; i++)
        {
            if (inRowCount >= _data.InventoryMaxHight)
            {
                beginPosition.z -= objectsDistanceZ;
                inRowCount = 0;
            }
            
            var pos = Vector3.zero;
            pos.y = beginPosition.y + (inRowCount * objectsDistanceY);
            pos.z = beginPosition.z;

            inRowCount += 1;
            
            _objects[i].SetPosition(pos, duration);
        }
    }

    private void ActualizeObjectsCountText(int currentObjectCount)
    {
        if (showCountText == false)
        {
            return;
        }
        
        string text = "";

        if (currentObjectCount >= _data.ActualInventoryCapacity())
        {
            text = "MAX";
        }

        capacityText.text = text;

        var beginYPosition = listBeginPosition.position;
        beginYPosition.y += (_data.InventoryMaxHight + 5) * objectsDistanceY;

        capacityText.transform.position = beginYPosition;
    }
}
