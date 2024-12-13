using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    SoundManager soundManager;
    FadeInOut fadeInOut;
    WaitForSeconds fadeTime = new WaitForSeconds(2f);

    [SerializeField]
    bool isLoading = false;

    private void Start()
    {
        isLoading = false;
        soundManager = SoundManager.instance;
        fadeInOut = FindObjectOfType<FadeInOut>();
    }

    public void LoadMainScene()
    {
        if (isLoading)
            return;

        Time.timeScale = 1.0f;
        if (fadeInOut != null)
            fadeInOut.FadeOut();
        StartCoroutine(_LoadMainScene());
    }

    public void LoadTitleScene()
    {
        if (isLoading)
            return;

        Time.timeScale = 1.0f;
        if (fadeInOut != null)
            fadeInOut.FadeOut();
        StartCoroutine(_LoadTitleScene());
    }

    public void LoadTutorialScene()
    {
        if (isLoading)
            return;

        if (fadeInOut != null)
            fadeInOut.FadeOut();
        StartCoroutine(_LoadTutorialScene());
    }

    public void LoadEndingCutScene()
    {
        if (isLoading)
            return;

        if (fadeInOut != null)
            fadeInOut.FadeOut();
        SceneManager.LoadScene(3);
        soundManager.PlayMusic(5, 2f);
    }

    IEnumerator _LoadTitleScene()
    {
        isLoading = true;
        yield return fadeTime;

        SceneManager.LoadScene(0);
        soundManager.PlayMusic(0, 2f);
    }
    IEnumerator _LoadTutorialScene()
    {
        isLoading = true;
        yield return fadeTime;

        SceneManager.LoadScene(1);
        soundManager.PlayMusic(1, 2f);
    }

    IEnumerator _LoadMainScene()
    {
        isLoading = true;
        yield return fadeTime;

        SceneManager.LoadScene(2);
        soundManager.PlayMusic(1, 2f);
    }

    public void Quit()
    {
#if UNITY_STANDALONE
        Application.Quit();
#endif

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
