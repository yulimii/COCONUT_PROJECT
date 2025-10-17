using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneManager : MonoBehaviour
{
    const string nextSceneNamekey = "NextKey";
    const string transitionSceneName = "Scene Transition";

    public void LoadNextScene(string _nextSceneName)
    {
        PlayerPrefs.SetString(nextSceneNamekey, _nextSceneName);
        StartCoroutine(LoadScene());
    }

    IEnumerator LoadScene()
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(transitionSceneName);
    
        while (!asyncOperation.isDone)
        {
            yield return null;
        }
    }

}
