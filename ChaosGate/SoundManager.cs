using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public AudioSource bgmSource;
    public AudioClip[] bgmClips;

    private Coroutine currentBGMCoroutine;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }


    public void PlayMusic(int index, float fadeDuration = 1f)
    {
        if (index >= 0 && index < bgmClips.Length)
        {
            if(currentBGMCoroutine != null)
            {
                StopCoroutine(currentBGMCoroutine);
            }

            currentBGMCoroutine = StartCoroutine(FadeOutBGM(fadeDuration, () =>
            {
                bgmSource.clip = bgmClips[index];
                bgmSource.Play();
                currentBGMCoroutine = StartCoroutine(FadeInBGM(fadeDuration));
            }));
        }
        else
        {
            Debug.LogWarning("배경음악 인덱스를 확인해주세요.");
        }
    }

    private IEnumerator FadeOutBGM(float duration, Action onFadeComplete)
    {
        float startVolume = bgmSource.volume;

        for(float t = 0; t < duration; t += Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        bgmSource.volume = 0;
        onFadeComplete?.Invoke();
    }

    private IEnumerator FadeInBGM(float duration)
    {
        float endVolume = 0.4f;
        bgmSource.volume = 0;

        for (float t = 0; t < duration; t+= Time.deltaTime)
        {
            bgmSource.volume = Mathf.Lerp(0f, endVolume, t / duration);
            yield return null;
        }

        bgmSource.volume = endVolume;
    }
}
