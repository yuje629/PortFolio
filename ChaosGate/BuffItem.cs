using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffItem : MonoBehaviour
{
    public ShopItem shopitem;

    public bool isOnBuff;

    public void ActivateBuff()
    {
        gameObject.SetActive(true);
        GameManager.instance._damage += shopitem.attackDamage;
        GameManager.instance._critdmg += shopitem.criticalDamage;
        GameManager.instance._critchance += shopitem.criticalRate;

        shopitem.isItemUnbuyable = true;
        isOnBuff = true;
    }

    public void DeactivateBuff()
    {
        GameManager.instance._damage -= shopitem.attackDamage;
        GameManager.instance._critdmg -= shopitem.criticalDamage;
        GameManager.instance._critchance -= shopitem.criticalRate;

        shopitem.isItemUnbuyable = false;
        isOnBuff = false;
        gameObject.SetActive(false);
    }
}

