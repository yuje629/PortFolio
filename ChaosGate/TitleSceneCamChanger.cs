using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TitleSceneCamChanger : MonoBehaviour
{
    public GameObject[] cams;
    public CinemachineBrain mainCam;
    public GameObject[] buttonWindow;

    private GameObject currentCam;

    public void SwitchToCamera(GameObject cam)
    {
        if (buttonWindow[0].activeSelf || buttonWindow[1].activeSelf || buttonWindow[2].activeSelf)
            return;

        for (int i = 0; i < cams.Length; i++)
        {
            if (cams[i] == cam)
            {
                cam.SetActive(true);
                currentCam = cam;
            }
            else
                cams[i].SetActive(false);
        }
    }

    public void TransitionToAnimation(Animator charAnim)
    {
        StartCoroutine(TransitionToIsPicked(charAnim));
    }

    IEnumerator TransitionToIsPicked(Animator charAnim)
    {
        if(charAnim != null) charAnim.ResetTrigger("PickOther");

        yield return new WaitForSeconds(0.2f);

        if (charAnim.GetBool("PickOther"))
            yield break;

        while (mainCam.IsBlending)
        {
            yield return null; 
        }

        if(charAnim != null) charAnim.SetTrigger("IsPicked");

    }

    public void TransitionToIdle(Animator charAnim)
    {
        if (charAnim != null)
        {
            charAnim.ResetTrigger("IsPicked");
            charAnim.SetTrigger("PickOther");
        }
    }

    public ICinemachineCamera GetCamera()
    {
        return currentCam.GetComponent<ICinemachineCamera>();
    }
}
