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

    override public bool CheckCanBuyMoreItems()  //아이템 더 구매할 수 있는지
    {
        if (selectedShopItem.isItemUnbuyable ||     //구매할 수 없는 아이템, 체력이 가득 차 있으면 체력회복 아이템(자동포션 제외)은 구매 불가
            (!selectedShopItem.isAutoPotion && Mathf.Ceil(gm._hp) == Mathf.Ceil(gm._maxhp) && (selectedShopItem.hp > 0 || selectedShopItem.hpRate > 0)))
        {
            return false;
        }
        
        return true;
    }

    override public void BuySelectedShopItem()
    {
        if (selectedShopItem.isAutoPotion)      //자동회복 아이템이라면 즉시 회복하지 않고 사용 가능한 상태로 변경
        {
            potionDic[selectedShopItem].SetAblePotion();
        }
        else if (selectedShopItem.isBuffItem)        // 버프아이템이라면 버프 활성화 상태로 변경
        {
            buffItemDic[selectedShopItem].ActivateBuff();
        }
        else
            selectedShopItem.ActivateItemAbility(); //아이템 기능 적용
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
