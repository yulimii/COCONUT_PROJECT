using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BaseSceneManager : MonoBehaviour
{
    public string[] nextSceneName;
    public MySceneManager mySceneManager;
    public GraphicRaycaster graphicRaycaster;
    public Image image;
    Color baseColor;
    WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        baseColor = image.color;
        image.enabled = false;
        graphicRaycaster.enabled = false;
    }

    protected void LoadScene(int sceneindex)
    {
        StartCoroutine(WaitTimes(sceneindex));
    }

    IEnumerator WaitTimes(int sceneindex)
    {
        yield return FadeOutAnimation();
        yield return new WaitForSeconds(1.0f);
        mySceneManager.LoadNextScene(nextSceneName[sceneindex]);
    }
    
    IEnumerator FadeOutAnimation()
    {
        graphicRaycaster.enabled = true;
        float alphaValue = 0.0f;
        image.color = new Color(baseColor.r, baseColor.g, baseColor.b, alphaValue);
        image.enabled = true;
    
        while (1.0f > alphaValue)
        {
            yield return waitForEndOfFrame;
            alphaValue += Time.deltaTime;
            image.color = new Color(baseColor.r, baseColor.g, baseColor.b, alphaValue);
        }
        yield return new WaitForSecondsRealtime(0.5f);
    }
}
