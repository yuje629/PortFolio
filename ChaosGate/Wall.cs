using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public Material wallMat;

    private float distance;
    private float maxDistance = 25f;
    private float alphaValue;
    private float maxAlphaValue;
    private bool isLR;
    private bool isBT;
    private Transform player;

    private void Start()
    {
        wallMat = transform.GetChild(0).GetComponent<MeshRenderer>().materials[1];

        if (transform.name == "L" || transform.name == "R")
            isLR = true;

        if (transform.name == "B" || transform.name == "T")
            isBT = true;

        if (transform.name == "B")
            maxAlphaValue = 0.04f;
        else
            maxAlphaValue = 0.6f;

        player = GameObject.FindWithTag("Player").transform;
    }

    void SetWallAlphaValue(float value)
    {
        value = Mathf.Clamp(value, 0, 1);
        wallMat.SetFloat("_DotsAlpha", value);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            if (isLR)
                maxDistance = Mathf.Abs(transform.position.x - player.position.x);
            if (isBT)
                maxDistance = Mathf.Abs(transform.position.z - player.position.z);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            if(isLR)
            {
                distance = Mathf.Abs(transform.position.x - player.position.x);

                SetWallAlphaValue((1 - distance / maxDistance) * maxAlphaValue);
            }

            if (isBT)
            {
                distance = Mathf.Abs(transform.position.z - player.position.z);

                SetWallAlphaValue((1 - distance / maxDistance) * maxAlphaValue);
            }
        }
    }
}
