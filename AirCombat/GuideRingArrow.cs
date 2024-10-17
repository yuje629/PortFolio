using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideRingArrow : MonoBehaviour
{
    public GameObject[] guideRings;
    public CameraCtrl cameraCtrl;
    public PlayerCtrl player;
    public GameObject arrowObject;
    int ringActivate;

    RectTransform rectTransform;


    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        player = GameManager.PlayerCtrl;
    }

    private void Update()
    {
        foreach (var guideRing in guideRings)
        {
            if (guideRing.activeSelf)
            {
                ringActivate = 0;

                arrowObject.SetActive(true);

                if (Vector3.Distance(guideRing.transform.position, player.transform.position) >= 700f)
                {
                    Vector3 relativePos;

                    if (player != null && cameraCtrl != null)
                    {
                        relativePos = guideRing.transform.position - player.transform.position;

                        float angle = Mathf.Atan2(-relativePos.x, relativePos.z) * Mathf.Rad2Deg;

                        angle += cameraCtrl.GetActiveCamera().transform.eulerAngles.y;
                        rectTransform.localEulerAngles = new Vector3(0, 0, angle);
                    }
                }
                else
                {
                    arrowObject.SetActive(false);
                }
            }
            else
                ringActivate++;
        }

        if (ringActivate >= guideRings.Length * 2)
            gameObject.SetActive(false);
    }
}
