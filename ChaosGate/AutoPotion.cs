using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AutoPotion : MonoBehaviour         //�ڵ� ȸ�� �����ִ� ��ũ��Ʈ
{
    public ShopItem shopitem;
    [HideInInspector] public bool isAble;       

    private int recoveryCount;

    private GameManager gm;

    private void Start()
    {
        gm = GameManager.instance;
    }

    private void Update()
    {
        if (gm == null) Debug.LogError("gm NULL!");
        if (shopitem == null) Debug.LogError("item NULL!");
        if (CheckHealthThreshold() && isAble)
        {
            if(shopitem != null)
                shopitem.ActivateItemAbility();

            recoveryCount--;

            if (recoveryCount <= 0)
                SetDisablePotion();
        }
    }
    private void InitRecoveryCount()
    {
        if(shopitem != null)
            recoveryCount = shopitem.recoveryCount;
    }

    public void SetAblePotion()
    {
        isAble = true;
        shopitem.isItemUnbuyable = true;
        InitRecoveryCount();
        gameObject.SetActive(true);
    }

    public void SetDisablePotion()
    {
        isAble = false;
        shopitem.isItemUnbuyable = false;
        gameObject.SetActive(false);
    }

    public bool CheckHealthThreshold() //�ǰ� ���� ��ġ�� �Ǿ����� Ȯ��
    {
        if(shopitem != null)
        {
            if (gm._hp <= gm._maxhp * 0.01f * shopitem.recoveryThreshold)
                return true;
            else
                return false;
        }
        return false;
    }
}
