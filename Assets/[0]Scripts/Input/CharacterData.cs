using System;
using UnityEngine;

public class CharacterData : MonoBehaviour
{
    public float MovementSpeed = 1f;
    public float baseSpeed = 1f;
    public int InventoryCapacity = 4;
    public int InventoryMaxHight = 10;
    public float fuelCapacity;
    public int MaximumUpgrades = 3;
    public float CollectingSpeed = 1f;
    public float HirePrice;
    public float UpgradePrice;
    public float NextUpgradeCostOffset;
    public Sprite uiUpgradeImage;

    [SerializeField] private bool isPlayer = false;

    [HideInInspector] public int CurrentUpgrades = 0;
    
    public void Upgrade()
    {
        CurrentUpgrades = Mathf.Clamp(CurrentUpgrades + 1, 0, MaximumUpgrades);
        InitializeUpgrades();
    }

    public void InitializeUpgrades()
    {
        MovementSpeed = baseSpeed + (CurrentUpgrades/2);
    }

    public int ActualInventoryCapacity()
    {
        var capacity = InventoryCapacity;

        if (isPlayer)
        {
            capacity += UpgradeSaves.Instance.playerCapacityUpgrades * 10;
        }

        return capacity;
    }
}
