using System;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using Utils;

public class GamePlayZone : MonoBehaviour
{
    [SerializeField] private GamePlayZoneType type;
    [SerializeField] private Transform playerPosition;
    [SerializeField] private CinemachineVirtualCamera camera;

    private void Start()
    {
        if (type == GamePlayZoneType.Facture && Tutorial.Instance.TutorialCompleted)
        {
            DOVirtual.DelayedCall(0.1f, () =>
            {
                UIManager.Instance.teleportWindow.TeleportToLastPosition();
            });
        }
    }

    public bool MatchType(GamePlayZoneType zoneType)
    {
        return zoneType == type;
    }

    public void SetVisibility(GamePlayZoneType zoneType, Transform player, bool needToHide, bool changePosition = true, bool showRocks = false)
    {
        var active = MatchType(zoneType);
        if (active)
        {
            if (changePosition)
            {
                if (type == GamePlayZoneType.Facture)
                {
                    //UIManager.Instance.teleportWindow.TeleportToLastPosition();
                }
                else
                {
                    player.position = playerPosition.position;
                    TreeZonesManager.Instance.SetRocksVisibility(showRocks);
                }
            }
            camera.Priority = 11;
        }
        else 
        {
            if (needToHide == false)
                active = true;
            
            camera.Priority = 10;

            if (type == GamePlayZoneType.Resources)
            {
                TreeZonesManager.Instance.DestroyUnUsedZones();
                TreeZonesManager.Instance.SetRocksVisibility(false);
            }
        }

        gameObject.SetActive(active);
    }


    public void Disable()
    {
        gameObject.SetActive(false);
    }
}
