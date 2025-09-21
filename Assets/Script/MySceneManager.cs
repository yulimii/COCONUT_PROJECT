using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneManager : MonoBehaviour
{
    const string nextSceneNamekey = "NextKey";
    const string transitionSceneName = "Scene Transition";

    public void LoadNextScene(string _nextSceneName)
    {
        PlayerPer.SetString(nextSceneNamekey, _nextSceneName);
        StartCorutine(LoadScene());
    }

    IEumerator LoadScene()
    {
        AsyncOperation asyncOperation = MySceneManager.LoadSceneAsync(transitionSceneName);

        while (!asyncOperation.isDone)
        {
            yield return null;
        }
    }

}
