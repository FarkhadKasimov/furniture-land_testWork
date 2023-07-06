using System.Collections;
using UnityEngine;

public class WorkerLogs : MagazineObjects
{
    [SerializeField] private WorkerLogBehaviour worker;
    [SerializeField] private ParticleSystem onBuyParticle;
    public override void Initialize(bool bought, bool canBuy, int moneyAdded = 0, int currentObjectsCount = 0)
    {
        base.Initialize(bought, canBuy, moneyAdded, currentObjectsCount);
        IsWorker = true;
        if (IsBought())
        {
            worker.Initialize(bought, canBuy);
            DisablePurchaser();
        }
    }

    public override void OnBuy()
    {
        base.OnBuy();
        if(onBuyParticle != null)
            onBuyParticle.Play();
    }

    public void EnableForBuy()
    {
        if (IsBought() && CanBuy())
        {
            return;
        }
        
        Initialize(false, true, 0);
        FactorySaves.Instance.ActualizeMagazinesData(this, false, true, 0);
    }
}
