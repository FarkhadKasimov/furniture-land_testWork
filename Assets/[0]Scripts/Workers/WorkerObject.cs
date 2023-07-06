using UnityEngine;
using Utils;

public class WorkerObject : MagazineObjects
{
    [SerializeField] private WorkerBehaviour worker;
    [SerializeField] private ParticleSystem onBuyParticle;
    public override void Initialize(bool bought, bool canBuy, int moneyAdded = 0, int currentObjectsCount = 0)
    {
        base.Initialize(bought, canBuy, moneyAdded, currentObjectsCount);
        IsWorker = true;
        if (IsBought())
        {
            worker.Initialize(bought, canBuy);
            if (worker.MatchType(WorkerType.FabriquePlacer))
            {
                Tutorial.Instance.DisableLastStep();
            }
        }

        DisablePurchaser();
    }

    public override void OnBuy()
    {
        base.OnBuy();
        onBuyParticle.Play();
    }

    public CharacterData GetData()
    {
        return worker.Data;
    }

    public WorkerBehaviour GetWorker()
    {
        return worker;
    }
}
