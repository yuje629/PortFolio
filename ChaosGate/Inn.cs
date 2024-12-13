using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inn : BaseShop
{
    public BuffItem[] buffItems;
    public AutoPotion[] autoPotions;
    public Dictionary<ShopItem, BuffItem> buffItemDic = new Dictionary<ShopItem, BuffItem>();
    public Dictionary<ShopItem, AutoPotion> potionDic = new Dictionary<ShopItem, AutoPotion>();

    override public void Start()
    {
        base.Start();

        for(int i = 0; i < buffItems.Length; i++)
        {
            buffItemDic.Add(buffItems[i].shopitem, buffItems[i]);
        }

        for (int j = 0; j < autoPotions.Length; j++)
        {
            potionDic.Add(autoPotions[j].shopitem, autoPotions[j]);
        }
    }

    override public bool CheckCanBuyMoreItems()  //������ �� ������ �� �ִ���
    {
        if (selectedShopItem.isItemUnbuyable ||     //������ �� ���� ������, ü���� ���� �� ������ ü��ȸ�� ������(�ڵ����� ����)�� ���� �Ұ�
            (!selectedShopItem.isAutoPotion && Mathf.Ceil(gm._hp) == Mathf.Ceil(gm._maxhp) && (selectedShopItem.hp > 0 || selectedShopItem.hpRate > 0)))
        {
            return false;
        }
        
        return true;
    }

    override public void BuySelectedShopItem()
    {
        if (selectedShopItem.isAutoPotion)      //�ڵ�ȸ�� �������̶�� ��� ȸ������ �ʰ� ��� ������ ���·� ����
        {
            potionDic[selectedShopItem].SetAblePotion();
        }
        else if (selectedShopItem.isBuffItem)        // �����������̶�� ���� Ȱ��ȭ ���·� ����
        {
            buffItemDic[selectedShopItem].ActivateBuff();
        }
        else
            selectedShopItem.ActivateItemAbility(); //������ ��� ����
    }

    public void RemoveBuffs()
    {
        foreach(var key in buffItemDic.Keys)
        {
            if (buffItemDic[key].isOnBuff)
                buffItemDic[key].DeactivateBuff();
        }
    }
}
