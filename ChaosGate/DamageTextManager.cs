using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTextManager : MonoBehaviour
{
    public GameObject damTxtObject;

    public static DamageTextManager instance;

    List<GameObject> damTxtPool;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        damTxtPool = new List<GameObject>();
    }

    public GameObject GetDamageTextObject()
    {
        GameObject select = null;

        foreach (GameObject item in damTxtPool)
        {
            if (!item.activeSelf)
            {
                select = item;
                select.SetActive(true);
                break;
            }
        }
        if (!select)
        {
            select = Instantiate(damTxtObject, transform);
            damTxtPool.Add(select);
        }
        return select;
    }
}
