using System;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : UIPanel
{
    private const string VibrationSavekey = "VIBRATION_SAVEKEY";
    private const string SoudSavekey = "SOUND_SAVEKEY";

    [SerializeField] private Toggle vibration;
    [SerializeField] private Toggle sound;
    
    private bool _vibration = true;
    private bool _sound = true;

    private void Start()
    {
        LoadSettings();
    }

    public void ShowPopUP()
    {
        GetCanvasGroup();
        SwitchPanelHard();
    }

    public void ToggleVibration()
    {
        _vibration = vibration.isOn;
        ApplySettings();
    }

    public void ToggleSound()
    {
        _sound = sound.isOn;
        ApplySettings();
    }

    private void ApplySettings()
    {
        //VibrationController.Instance.ChangeVibrationState(_vibration);
        //AudioManager.Instance.ChangeSoundState(_sound);
        
        SaveSettings();
    }

    private void SaveSettings()
    {
        ES3.Save(VibrationSavekey, _vibration);
        ES3.Save(SoudSavekey, _sound);
    }

    private void LoadSettings()
    {
        _vibration = ES3.Load(VibrationSavekey, true);
        _sound = ES3.Load(SoudSavekey, true);
        
        vibration.isOn = _vibration;
        sound.isOn = _sound;
    }
}
