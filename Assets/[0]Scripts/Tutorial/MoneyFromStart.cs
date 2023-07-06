using UnityEngine;

public class MoneyFromStart : MonoBehaviour
{
    private const string moneyCollectedSaveKey = "START_MONEY_COLLECTED";

    private bool collected;

    private void Awake()
    {
        collected = ES3.Load(moneyCollectedSaveKey, false);
        if (collected)
        {
            gameObject.SetActive(false);
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>())
        {
            ES3.Save(moneyCollectedSaveKey, true);
            Tutorial.Instance.MoveNext(Tutorial.TutorialPath.CollectMoney);
            AnalyticsManager.Instance.OnFirstMoneyCollected();
            GetComponent<Collider>().enabled = false;
        }
    }
}
