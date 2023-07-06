using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class MainLoader : MonoBehaviour
{
    public float delayToLoadScene = 0.1f;
    public UnityEvent InitializeEvent;
    public string sceneToLoad;

    void Start()
    {
        InitializeEvent.Invoke();
        StartCoroutine(LoadMainSceneWithDelay());
    }

    public IEnumerator LoadMainSceneWithDelay()
    {
        yield return new WaitForSeconds(delayToLoadScene);
        SceneManager.LoadScene(sceneToLoad);
    }
}
