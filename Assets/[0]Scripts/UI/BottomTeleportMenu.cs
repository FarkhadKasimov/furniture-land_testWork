using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class BottomTeleportMenu : MonoBehaviour
{
    [SerializeField] private Transform selectorTransform;
    [SerializeField] private Image[] buttonImages;
    [SerializeField] private CanvasGroup mainGroup;
    private GamePlayZoneList _zoneList => GamePlayZoneList.Instance;
    private int lastIndex = 1;

    public void Start()
    {
        if (Tutorial.Instance.TutorialCompleted)
        {
            InteractionState(true);
        }
        else
        {
            if (Tutorial.Instance.CurrentPath >= 8)
            {
                InteractionState(true);
            }
        }
    }

    public void ShowGamePlayZone(int i)
    {
        _zoneList.EnableZoneByType((GamePlayZoneType)i);
        selectorTransform.DOMoveX(buttonImages[i].transform.position.x, 0.5f);
        lastIndex = i;
        VibrationController.Instance.PlayVibration("BottomSwipe_Vibration");
    }

    public void EanbleSelectionZone(int i)
    {
        selectorTransform.DOMoveX(buttonImages[i].transform.position.x, 0.5f);
        VibrationController.Instance.PlayVibration("BottomSwipe_Vibration");
    }
    
    public void DisableSelectionZone(int i)
    {
        if (i == lastIndex)
        {
            return;
        }
        
        selectorTransform.DOMoveX(buttonImages[lastIndex].transform.position.x, 0.5f);
    }

    public void InteractionState(bool state)
    {
        mainGroup.interactable = state;
    }
}
