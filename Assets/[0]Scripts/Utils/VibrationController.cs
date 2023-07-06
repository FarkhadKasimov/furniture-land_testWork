using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
//using MoreMountains.NiceVibrations;
using Project.Internal;

public class VibrationController : Singleton<VibrationController>
{
    [System.Serializable]
    public class VibrationPreset
    {
        public string key;
        //public HapticTypes type;
        public float delay;
        public long  impact;
        public int amplitude;
    }

    public List<VibrationPreset> pressets = new List<VibrationPreset>();

    private bool vibrationEnabled = true;
    private bool vibrationCurrentlyRunning = false;

    public void PlayVibration(string _key)
    {
        //if (MMVibrationManager.Android() && MMVibrationManager.HapticsSupported() == false)
        //{
        //    Debug.Log("Haptic is not supported on this platform!");
        //    return;
        //}

        if (vibrationEnabled == false)
        {
            return;
        }

        if (vibrationCurrentlyRunning)
        {
            return;
        }

        vibrationCurrentlyRunning = true;
        
        VibrationPreset preset = FindVibrationPressetByKey(_key);

        //if (preset.type == HapticTypes.Custom || preset.type == HapticTypes.None)
        //{
        //    MMVibrationManager.Haptic(preset.impact, preset.amplitude);
        //}
        //else
        //{
        //    MMVibrationManager.Haptic(preset.type, false, true, this);
        //}

        DOVirtual.DelayedCall(preset.delay, () =>
        {
            vibrationCurrentlyRunning = false;
        });
        
        Debug.Log("Vibration runned and completed!");
    }


    private VibrationPreset FindVibrationPressetByKey(string _key)
    {
        foreach (VibrationPreset preset in pressets)
        {
            if (_key.Equals(preset.key, System.StringComparison.Ordinal))
            {
                return preset;
            }
        }

        return pressets[0];
    }

    public void ChangeVibrationState(bool state)
    {
        vibrationEnabled = state;
    }
}
