using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class TreeZone : MonoBehaviour
{
    private const string CuttedTreesSaveKey = "CuttedTrees_SAVEKEY";
    
    public Vector3 EndPoint => endPoint.position;

    [SerializeField] private Transform endPoint;
    [SerializeField] private ZoneTrigger trigger;
    [SerializeField] private Tree[] resources;
    [SerializeField] private GameObject sing;
    [SerializeField] private TextMeshProUGUI levelCount;
    [SerializeField] private Transform rocksParent;
    [SerializeField] private GameObject buff;

    private int _strength = -1;
    private int _cuttedTreesCount = 0;
    private bool _enteresTrigger = false;
    private string treesSavekey => CuttedTreesSaveKey + _strength;

    private void OnEnable()
    {
        trigger.PlayerEnterEvent += OnPlayerEnter;
    }

    private void OnDisable()
    {
        trigger.PlayerEnterEvent -= OnPlayerEnter;
    }

    public void InitializeZone(int strength, float collectingTime, int resourcesCount , bool withRocks, Color color, bool showBuff)
    {
        _strength = strength;
        _cuttedTreesCount = ES3.Load(treesSavekey, 0);
        
        levelCount.text = "Zone " + (strength + 2);

        var cuttedCount = 0;
        foreach (var resource in resources)
        {
            if (cuttedCount < _cuttedTreesCount)
            {
                resource.DestroyTree();
                cuttedCount += 1;
            }
            else
            {
                resource.CuttedTreeEvent += OnTreeCutted;
                resource.Initialize(collectingTime, resourcesCount, color);
            }
        }
        
        rocksParent.gameObject.SetActive(withRocks);

        if (showBuff)
        {
            buff.SetActive(true);
        }
    }

    public void SetRocksVisible(bool state)
    {
        rocksParent.gameObject.SetActive(state);

        if (state)
        {
            for (int i = 0; i < rocksParent.childCount; i++)
            {
                var rock = rocksParent.GetChild(i);
                var position = rock.position;
                position.z += Random.Range(-4f, 4f);
                
                rock.position = position;
            }
        }
    }
    
    public void OnTreeCutted(RespurcesGenerator resource)
    {
        resource.CuttedTreeEvent -= OnTreeCutted;
        _cuttedTreesCount += 1;
        ES3.Save(treesSavekey, _cuttedTreesCount);

        var precent = (_cuttedTreesCount * 100) / resources.Length;
        if (precent > 90)
        {
            sing.transform.DOMoveY(-4f, 1f).OnComplete(() => sing.gameObject.SetActive(false));

            if (_enteresTrigger == false)
            {
                TreeZonesManager.Instance.SpawnNewZone(_strength);
                _enteresTrigger = true;
            }
        }
    }

    private void OnPlayerEnter(PlayerController _controller)
    {
        if (_enteresTrigger)
        {
            return;
        }

        _enteresTrigger = true;
        TreeZonesManager.Instance.SpawnNewZone(_strength);
    }

    public void ShowZoneLock()
    {
        sing.gameObject.SetActive(true);
    }

    public Transform NearestResource(Vector3 point)
    {
        var minPos = float.MaxValue;
        Transform resource = null;
        foreach (var res in resources)
        {
            if (res.HasAnyResources() == false)
            {
                continue;
            }

            var dist = Vector3.Distance(res.transform.position, point);
            if (dist < minPos)
            {
                minPos = dist;
                resource = res.transform;
            }
        }

        return resource;
    }
}
