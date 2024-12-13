using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public GameObject villageCam;
    public GameObject fieldCam;
    private CinemachineVirtualCamera currentCam;
    private CinemachineVirtualCamera shakeCam;
    private float shakeTimer;

    private void Start()
    {
        currentCam = villageCam.GetComponent<CinemachineVirtualCamera>();
    }

    private void Update()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            if(shakeTimer <= 0f)
            {
                CinemachineBasicMultiChannelPerlin cBMCP 
                    = shakeCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

                cBMCP.m_AmplitudeGain = 0f;
            }
        }
    }

    public void ShowVillageView()
    {
        currentCam = villageCam.GetComponent<CinemachineVirtualCamera>();
        villageCam.SetActive(true);
        fieldCam.SetActive(false);
    }

    public void ShowFieldView()
    {
        currentCam = fieldCam.GetComponent<CinemachineVirtualCamera>();
        fieldCam.SetActive(true);
        villageCam.SetActive(false);
    }

    public CinemachineVirtualCamera GetCamera()
    {
        return currentCam;
    }

    public void ShakeCamera(float intensity, float time)
    {
        shakeCam = GetCamera();
        CinemachineBasicMultiChannelPerlin cBMCP = currentCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        cBMCP.m_AmplitudeGain = intensity;
        shakeTimer = time;
    }
}
