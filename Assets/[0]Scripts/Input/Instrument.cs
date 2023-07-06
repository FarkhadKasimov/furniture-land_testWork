using System;
using System.Collections;
using UnityEngine;
using Utils;

public class Instrument : MonoBehaviour
{
    [SerializeField] private InstrumentType instrumentType;
    [SerializeField] private Transform objectToScale;
    [SerializeField] private Floater animation;

    public PlayerController Player;

    public void Show(InstrumentType type, PlayerController playerController)
    {
        if (instrumentType != type)
        {
            return;
        }
        
        gameObject.SetActive(true);
        var scale = objectToScale.localScale;
        scale.x = 2 + UpgradeSaves.Instance.playerSawScaleUpgrade * 0.3f;
        scale.z = 2 + UpgradeSaves.Instance.playerSawScaleUpgrade * 0.3f;
        objectToScale.localScale = scale;
        Player = playerController;
    }

    public void ResetSacale()
    {
        var scale = objectToScale.localScale;
        scale.x = 2 + UpgradeSaves.Instance.playerSawScaleUpgrade * 0.3f;
        scale.z = 2 + UpgradeSaves.Instance.playerSawScaleUpgrade * 0.3f;
        objectToScale.localScale = scale;
    }
    
    public void RunAnimation()
    {
        animation.Animate = true;
    }

    public void StopAnimation()
    {
        animation.Animate = false;
    }
    
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
