using System;
using UnityEngine;
using Utils;

public class PlayerInstruments : MonoBehaviour
{
    [SerializeField] private Instrument[] instruments;
    public SawFuelUI fuelUI;
    [SerializeField] private float baseSpeed = 0.5f;

    private float fuelCapacity = 0f;
    private PlayerController controller;

    public void SetupInstrument(InstrumentType type, PlayerController playerController, float capacity)
    {
        HideInstrument();

        controller = playerController;
        fuelCapacity = capacity;
        
        foreach (var instrument in instruments)
        {
            instrument.Show(type, playerController);
            fuelUI.FuelEmptyEvent += instrument.StopAnimation;
            fuelUI.FuelEmptyEvent += Debuff;
            instrument.RunAnimation();
        }
        
        ShowFuelUI();
    }

    public void HideInstrument()
    {
        foreach (var instrument in instruments)
        {
            instrument.Hide();
            fuelUI.FuelEmptyEvent -= instrument.StopAnimation;
            fuelUI.FuelEmptyEvent -= Debuff;
        }

        fuelUI.StopCollecting();
    }

    private void ShowFuelUI()
    {
        fuelUI.Initialize(fuelCapacity, baseSpeed);
        fuelUI.StartCollecting();
    }

    private void Debuff()
    {
        controller.Debuff();
        foreach (var instrument in instruments)
        {
            instrument.ResetSacale();
        }
    }
}
