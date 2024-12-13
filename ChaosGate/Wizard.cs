using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wizard : BaseShop
{
    public StatItem[] StatItems;
    public Dictionary<ShopItem, StatItem> statItemDic = new Dictionary<ShopItem, StatItem>();

    override public void Start()
    {
        base.Start();

        for (int i = 0; i < StatItems.Length; i++)
        {
            statItemDic.Add(StatItems[i].shopitem, StatItems[i]);
        }
    }

    override public bool CheckCanBuyMoreItems()
    {
        int itemCurLevel = selectedShopItem.GetComponent<StatItem>().cur_Level;
        int itemMaxLevel = selectedShopItem.GetComponent<StatItem>().max_Level;

        if (itemCurLevel < itemMaxLevel)
        {
            return true;
        }
        else
            return false;
    }

    override public void BuySelectedShopItem()
    {
        statItemDic[selectedShopItem].ApplyStat();
    }
}
