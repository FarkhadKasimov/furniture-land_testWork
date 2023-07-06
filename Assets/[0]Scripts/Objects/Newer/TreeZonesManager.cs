using System;
using System.Collections.Generic;
using Project.Internal;
using UnityEngine;
using Random = UnityEngine.Random;

public class TreeZonesManager : Singleton<TreeZonesManager>
{
    private const string StrengthSavekey = "TreeZoneStrength_SAVEKEY";

    [SerializeField] private float treesCollectingTime = 0.5f;
    [SerializeField] private float resourceGivingStep = 1;
    [SerializeField] private int showingTableStep = 5;
    
    [SerializeField] private int distance = 2;
    [SerializeField] private Transform beginZonePosition;
    [SerializeField] private TreeZone zonePrefab;
    [SerializeField] private Color[] treeColors;
    [SerializeField] private AstarPath path;

    private int _lastZoneStrength = 0;
    private List<TreeZone> _zoneList = new List<TreeZone>();
    
    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        _lastZoneStrength = ES3.Load(StrengthSavekey, 0);
        _lastZoneStrength = Mathf.Clamp(_lastZoneStrength - 1, 0, _lastZoneStrength);
        //if (_lastZoneStrength < distance)
        //{
        //    _lastZoneStrength = distance;
        //}

        var position = beginZonePosition.position;
        var zone_tmp = InstantiateZone(position, _lastZoneStrength);
        InstantiateZone(zone_tmp.EndPoint);
        
        //for (int i = distance; i > 0; i--)
        //{
            //var strength = Mathf.Clamp(_lastZoneStrength - i, 0, _lastZoneStrength);
            //var zone = InstantiateZone(position, strength);
            //position = zone.EndPoint;
        //}
        
        //_zoneList.Reverse();
        
        position = beginZonePosition.position;
        foreach (var zone in _zoneList)
        {
            zone.transform.position = position;
            position = zone.EndPoint;
        }
        
        path.Scan();
    }

    private TreeZone InstantiateZone(Vector3 position, int strength = -1)
    {
        var zone = Instantiate(zonePrefab, position, Quaternion.identity);

        if (strength == -1)
        {
            _lastZoneStrength += 1;
        }
        
        var strengthLevel = strength == -1 ? _lastZoneStrength : strength;
        
        var collectingTime = strengthLevel * treesCollectingTime;
        var resourcesCount = Mathf.FloorToInt(strengthLevel * resourceGivingStep);
        if (resourcesCount == 0)
        {
            resourcesCount = 1;
        }

        var a = strengthLevel / treeColors.Length;
        var colorIndex = strengthLevel - (a * treeColors.Length);
        var color = treeColors[colorIndex];

        var random = Random.Range(0f, 1f);
        var showBuff = _lastZoneStrength != 0 && _lastZoneStrength % 2 == 0;
        
        zone.InitializeZone(strengthLevel, collectingTime, resourcesCount, false, color, showBuff);
        zone.ShowZoneLock();
        
        _zoneList.Add(zone);

        return zone;
    }

    public void DestroyUnUsedZones()
    {
        if (_zoneList.Count <= distance)
        {
            return;
        }

        var zonesToRemove = _zoneList.Count - distance;
        List<Vector3> positions = new List<Vector3>();
        positions.Add(beginZonePosition.position);

        for (int i = 0; i < zonesToRemove; i++)
        {
            var zone = _zoneList[i];
            positions.Add(zone.EndPoint);
            Destroy(zone.gameObject);
            _zoneList[i] = null;
        }

        _zoneList.RemoveAll(IsNull);

        var index = 0;
        foreach (var zone in _zoneList)
        {
            zone.transform.position = positions[index];
            index += 1;
        }
    }

    public void SetRocksVisibility(bool state)
    {
        foreach (var zone in _zoneList)
        {
            zone.SetRocksVisible(state);
        }
    }
    
    private bool IsNull(TreeZone zone)
    {
        return zone == null;
    }
    
    public void SpawnNewZone(int strength)
    {
        ES3.Save(StrengthSavekey, _lastZoneStrength + 1);
        InstantiateZone(_zoneList[_zoneList.Count - 1].EndPoint);
    }

    public Transform GentNearestTree(Vector3 position)
    {
        var min = float.MaxValue;
        Transform resource = null;
        foreach (var zone in _zoneList)
        {
            var zoneT = zone.NearestResource(position);
            if (zoneT == null)
            {
                continue;
            }

            var dist = Vector3.Distance(zoneT.position, position);
            if (dist < min)
            {
                min = dist;
                resource = zoneT;
            }
        }

        return resource;
    }
}
