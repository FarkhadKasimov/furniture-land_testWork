using System;
using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.Tools;
using UnityEngine;
using Utils;

public class TeleportUI : UIPanel
{
    [SerializeField] private Transform teleportRowParent;
    [SerializeField] private TeleportRow teleportRow;
    [SerializeField] private PlayerController player;

    private List<TeleportRow> _rows = new List<TeleportRow>();

    private int currentMagazineIndex = 0;
    private const string teleportIndexSavekey = "TELEPORT_SAVEKEY";
    
    private FactorySaves _magazines => FactorySaves.Instance;

    private void Awake()
    {
        currentMagazineIndex = ES3.Load(teleportIndexSavekey, 0);
        GetCanvasGroup();
    }

    public void TeleportToLastPosition()
    {
        ConstructUI();
        _rows[currentMagazineIndex].Teleport();
    }

    public void TeleportToLastPositionFromResources()
    {
        ConstructUI();
        _rows[currentMagazineIndex].Teleport();
    }
    
    public void ShowPanel(bool constructUI = false)
    {
        SwitchPanel();
        if (constructUI)
        {
            ConstructUI();
        }
    }

    private void ConstructUI()
    {
        _rows.Clear();
        teleportRowParent.MMDestroyAllChildren();
        foreach (var magazine in _magazines.factoryShops)
        {
            if (magazine.shop.IsBought == false)
            {
                return;
            }
            
            var row = Instantiate(teleportRow, teleportRowParent);
            _rows.Add(row);
            var canTeleport = (_rows.Count - 1) != currentMagazineIndex;
            row.Initialize(this, magazine.shop.Name, magazine.shop.TeleportPosition, canTeleport);
        }
    }

    public void TeleportUpdateInfo(TeleportRow row, Vector3 position, bool movePlayer = true)
    {
        foreach (var _row in _rows)
        {
            if (_row == row)
            {
                currentMagazineIndex = _rows.IndexOf(_row);
                
                if (movePlayer)
                {
                    if (GamePlayZoneList.Instance.CheckZone())
                    {
                        player.MoveToPosition(position);
                    }
                    else
                    {
                        GamePlayZoneList.Instance.StartUseTeleportCoroutine(player, position);
                    }
                }
                
                ES3.Save(teleportIndexSavekey, currentMagazineIndex);
            }
        }

        if (IsOpen())
        {
            ShowPanel();
            UIManager.Instance.bottomMenu.EanbleSelectionZone(1);
        }
    }

    public void TeleportFromScene(int magazineIndex)
    {
        ConstructUI();
        TeleportUpdateInfo(_rows[magazineIndex], Vector3.zero, false);
    }

    public FactoryShop CurrentMagazine()
    {
        return _magazines.factoryShops[currentMagazineIndex].shop;
    }
    
    public int CurrentMagazineIndex()
    {
        return currentMagazineIndex;
    }
}
