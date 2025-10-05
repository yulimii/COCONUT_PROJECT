using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneManager : MonoBehaviour
{
    const string nextSceneNamekey = "NextKey";
    const string transitionSceneName = "Scene Transition";

    public void LoadNextScene(string _nextSceneName)
    {
        //PlayerPrefeb.SetString(nextSceneNamekey, _nextSceneName);
        //StartCorutine(LoadScene());
    }

    //IEnumerator LoadScene()
    //{
    //    AsyncOperation asyncOperation = MySceneManager.LoadSceneAsync(transitionSceneName);
    //
    //    while (!asyncOperation.isDone)
    //    {
    //        yield return null;
    //    }
    //}

}
