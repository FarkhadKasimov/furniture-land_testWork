//using Facebook.Unity;
using UnityEngine;

public class FaceBoockInitializer : MonoBehaviour
{
    private void Awake()
    {
        //if (FB.IsInitialized == false) 
        //{
        //    FB.Init(InitCallback, OnHideUnity);
        //} else 
        //{
        //    FB.ActivateApp();
        //}
    }

    private void InitCallback()
    {
        //if (FB.IsInitialized) 
        //{
        //    FB.ActivateApp();
        //} 
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown) {
            // Pause the game - we will need to hide
            Time.timeScale = 0;
        } else {
            // Resume the game - we're getting focus again
            Time.timeScale = 1;
        }
    }
}
