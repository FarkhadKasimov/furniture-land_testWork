using System;
using System.Collections;
using Project.Internal;
using UnityEngine;
using Utils;

public class GamePlayZoneList : Singleton<GamePlayZoneList>
{
    [SerializeField] private float travelTime = 0.1f;
    [SerializeField] private GamePlayZone[] gamePlayZones;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private bool needToHideGameObjects = false;

    [SerializeField] private Material[] materialsToChange;
    
    private GamePlayZoneType currentType = GamePlayZoneType.Facture;
    private Camera mainCamera;

    private UIManager _uiManager => UIManager.Instance;

    private void Awake()
    {
        SetMaterialsOutlineWidth(2);
    }

    public void EnableZoneByType(GamePlayZoneType type, bool showRocks = false)
    {
        if (currentType == type)
        {
            return;
        }
        
        StartCoroutine(EnableZone(type, showRocks));
    }

    public bool CheckZone()
    {
        return currentType == GamePlayZoneType.Facture;
    }

    private void DisableZones()
    {
        if (needToHideGameObjects == false)
        {
            return;
        }
        
        foreach (var zone in gamePlayZones)
        {
            zone.Disable();
        }
    }
    
    private IEnumerator EnableZone(GamePlayZoneType type, bool showRocks)
    {
        _uiManager.ShowFadePanel(travelTime);
        yield return new WaitForSeconds(travelTime);

        DisableZones();
        Tutorial.Instance.HideArrowToExit();

        foreach (var zone in gamePlayZones)
        {
            zone.SetVisibility(type, playerController.transform, needToHideGameObjects, true, showRocks);
        }

        currentType = type;
        mainCamera = Camera.main;
        
        if (currentType == GamePlayZoneType.Resources)
        {
            playerController.StartWorking(InstrumentType.Axe);
            mainCamera.orthographic = false;
            SetMaterialsOutlineWidth(20);
            Tutorial.Instance.MoveNext(Tutorial.TutorialPath.GoToTheResources);
        }
        else
        {
            mainCamera.orthographic = true;
            SetMaterialsOutlineWidth(2);
            playerController.StopWorking();
            playerController.PlayerInstruments.fuelUI.Hide();
            Tutorial.Instance.MoveNext(Tutorial.TutorialPath.GoToTheFactory);
        }
        
        if (type == GamePlayZoneType.Facture)
        {
            UIManager.Instance.teleportWindow.TeleportToLastPosition();
        }
        
        yield return new WaitForSeconds(0.2f);
        _uiManager.HideFadePanel(travelTime);
        yield return new WaitForSeconds(travelTime);
    }

    private void SetMaterialsOutlineWidth(float width)
    {
        foreach (var material in materialsToChange)
        {
            material.SetFloat("_OutlineSize", width);       
        }
    }
    
    public void StartUseTeleportCoroutine(PlayerController player, Vector3 position)
    {
        StartCoroutine(UzeTeleport(player, position));
    }
    
    private IEnumerator UzeTeleport(PlayerController player, Vector3 position)
    {
        _uiManager.ShowFadePanel(travelTime);
        yield return new WaitForSeconds(travelTime);

        DisableZones();

        foreach (var zone in gamePlayZones)
        {
            zone.SetVisibility(GamePlayZoneType.Facture, playerController.transform, needToHideGameObjects, false);
        }

        player.MoveToPosition(position);
        currentType = GamePlayZoneType.Facture;
        mainCamera = Camera.main;
        
        mainCamera.orthographic = true;
        playerController.StopWorking();
        Tutorial.Instance.MoveNext(Tutorial.TutorialPath.GoToTheFactory);

        yield return new WaitForSeconds(0.2f);
        _uiManager.HideFadePanel(travelTime);
        yield return new WaitForSeconds(travelTime);
    }
}
