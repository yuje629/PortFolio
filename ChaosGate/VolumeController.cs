using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    private SoundManager soundManager;

    public AudioMixer audioMixer;

    public Slider[] slider;                 //0: BGM, 1:SFX, 2:ITEM

    private void Start()
    {
        soundManager = FindObjectOfType<SoundManager>();
        slider[0].value = GetBGMVolume();
        slider[1].value = GetSFXVolume();
        slider[2].value = GetITEMVolume();
    }

    public void SetBGMVolume()
    {
        float value = slider[0].value;

        if (value <= -40)
            value = -80;

        if (audioMixer != null)
            audioMixer.SetFloat("BGM", value);
    }

    public void SetSFXVolume()
    {
        float value = slider[1].value;

        if (value <= -40)
            value = -80;

        if (audioMixer != null)
            audioMixer.SetFloat("SFX", value);
    }

    public void SetITEMVolume()
    {
        float value = slider[2].value;

        if (value <= -40)
            value = -80;

        if (audioMixer != null)
            audioMixer.SetFloat("ITEM", value);
    }

    public float GetBGMVolume()
    {
        float volume;
        audioMixer.GetFloat("BGM", out volume);

        return volume;
    }

    public float GetSFXVolume()
    {
        float volume;
        audioMixer.GetFloat("SFX", out volume);

        return volume;
    }

    public float GetITEMVolume()
    {
        float volume;
        audioMixer.GetFloat("ITEM", out volume);

        return volume;
    }
}
