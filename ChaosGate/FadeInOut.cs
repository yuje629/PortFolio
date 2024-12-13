using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInOut : MonoBehaviour
{
    public CanvasGroup canvasgroup;
    public bool fadein = false;
    public bool fadeout = false;

    public float TimeToFade;

    private void Start()
    {
        FadeIn();
    }

    private void Update()
    {
        if(fadeout == true)
        {
            if(canvasgroup.alpha <= 1)
            {
                canvasgroup.alpha += TimeToFade * Time.deltaTime;
                if(canvasgroup.alpha == 1)
                {
                    fadeout = false;
                }
            }
        }
        if (fadein == true)
        {
            if (canvasgroup.alpha >= 0)
            {
                canvasgroup.alpha -= TimeToFade * Time.deltaTime;
                if (canvasgroup.alpha == 0)
                {
                    fadein = false;
                }
            }
        }
    }

    public void FadeOut()
    {
        fadeout = true;
        canvasgroup.alpha = 0;
    }

    public void FadeIn()
    {
        fadein = true;
        canvasgroup.alpha = 1;
    }
}
