using System;
using System.IO;
using DG.Tweening;
using Project.Internal;
using UnityEngine;

public class Tutorial : Singleton<Tutorial>
{
    [System.Serializable]
    public class Data
    {
        public Transform position;
        public TutorialPath path;
        public bool showArrow;
        public bool uiArrowShow;
    }
    
    [SerializeField] private Transform arrow;
    [SerializeField] private Transform uiArrow;
    [SerializeField] private Transform fadePanel;
    [SerializeField] private float arrowShowDistance = 0.5f;
    [SerializeField] private Transform player;
    [SerializeField] private SawFuelUI sawFuelUi;
    [SerializeField] private Transform exitTransform;
    [SerializeField] private Data[] tutorialData;

    public bool TutorialCompleted => tutorialCompleted;
    public int CurrentPath => currentTutorialIndex;

    private int currentTutorialIndex = 0;
    private bool tutorialCompleted = false;
    private bool cassierIsBought = false;
    private Transform currentTraget;
    private const string currentTutorialSavekey = "CURRENT_TUTORIAL_SAVEKEY";
    private const string tutorialCompletition = "TUTORIALCOMPLETITION_SAVEKEY";

    private bool showArrowToExit = false;

    private void OnEnable()
    {
        sawFuelUi.FuelEmptyEvent += ShowArrowToExit;
    }

    private void OnDisable()
    {
        sawFuelUi.FuelEmptyEvent -= ShowArrowToExit;
    }

    private void Awake()
    {
        currentTutorialIndex = ES3.Load(currentTutorialSavekey, 0);
        tutorialCompleted = ES3.Load(tutorialCompletition, false);

        player = FindObjectOfType<PlayerController>().transform;
        
        if (tutorialCompleted)
        {
            return;
        }

        EnableTutorialPart();
    }

    private void Update()
    {
        if (tutorialCompleted)
        {
            if (showArrowToExit)
            {
                arrow.position = player.position;
                arrow.DOLookAt(exitTransform.position, Time.deltaTime, AxisConstraint.Y);
                arrow.gameObject.SetActive(Vector3.Distance(player.position, exitTransform.position) > arrowShowDistance);
            }
            return;
        }

        if (tutorialData[currentTutorialIndex].showArrow)
        {
            arrow.position = player.position;
            arrow.DOLookAt(currentTraget.position, Time.deltaTime, AxisConstraint.Y);
            arrow.gameObject.SetActive(Vector3.Distance(player.position, currentTraget.position) > arrowShowDistance);
        }
        else
        {
            if (arrow.gameObject.activeInHierarchy)
            {
                arrow.gameObject.SetActive(false);
            }
        }
    }

    public void ShowArrowToExit()
    {
        showArrowToExit = true;
    }

    public void HideArrowToExit()
    {
        showArrowToExit = false;
    }
    
    private void EnableTutorialPart()
    {
        var data = tutorialData[currentTutorialIndex];
        currentTraget = data.position;

        if (data.uiArrowShow)
        {
            uiArrow.gameObject.SetActive(true);
            fadePanel.gameObject.SetActive(true);
            fadePanel.transform.position = currentTraget.transform.position;
            uiArrow.DOMoveX(currentTraget.position.x, 0.5f);
        }
    }

    public void MoveNext(TutorialPath path)
    {
        if (tutorialCompleted)
        {
            return;
        }

        if (tutorialData[currentTutorialIndex].path != path)
        {
            return;
        }
        
        arrow.gameObject.SetActive(tutorialData[currentTutorialIndex].showArrow);
        uiArrow.gameObject.SetActive(false);
        fadePanel.gameObject.SetActive(false);
        
        /*if (tutorialData[currentTutorialIndex].path == TutorialPath.GoToVitrine && cassierIsBought)
        {
            currentTutorialIndex += 1;
        }*/
        
        var nextIndex = currentTutorialIndex + 1;
        if (nextIndex >= tutorialData.Length)
        {
            FinishTutorial();
            return;
        }

        currentTutorialIndex = nextIndex;
        ES3.Save(currentTutorialSavekey, currentTutorialIndex);
        EnableTutorialPart();
    }

    public void MoveNext(int path)
    {
        MoveNext((TutorialPath) path);
    }
    
    public void DisableLastStep()
    {
        cassierIsBought = true;
    }

    public void FinishTutorial()
    {
        tutorialCompleted = true;
        arrow.gameObject.SetActive(false);
        ES3.Save(tutorialCompletition, true);
    }
    
    public enum TutorialPath
    {
        CollectMoney = 0,
        BuyWarehouse = 1,
        BuyProcessingMachine = 2,
        BuyWareHouseExit = 3,
        BuyChairMachine = 4,
        BuyChairVitrine = 5,
        CollectPrecessedResources = 6,
        MakeChair = 7,
        GoToTheResources = 8,
        CollectResources = 9,
        GoToTheFactory = 10,
        GoToTheWarehouse = 11,
        GoToCassa = 12,
        BuyCassaObject = 13,
        waitFuel = 14
    }
}
