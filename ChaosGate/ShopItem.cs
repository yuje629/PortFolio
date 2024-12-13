using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ShopItem : MonoBehaviour
{
    public int itemID;
    public Sprite itemImage;
    public string itemName;
    public string itemInfo;
    public int price;
    public int maxHP;
    public int maxLevel;
    public float attackDamage;
    public float diffence;
    public float recoveryThreshold;
    public int recoveryCount;
    public float hp;
    public float hpRate;
    public float criticalDamage;
    public float criticalRate;
    public float dashCoolTime;
    public float itemCoolTimeDropRate;
    
    protected BaseShop shop;
    public TMP_Text itemNameText;
    public TMP_Text itemInfoText;
    public TMP_Text itemPriceText;

    public bool isAutoPotion;
    public bool isItemUnbuyable;
    public bool isBuffItem;

    GameManager gm;

    private void Start()
    {
        gm = GameManager.instance;
        SetShopItemData();

        if (itemID == 1100 || itemID == 1101)
            isAutoPotion = true;

        if (itemID >= 1200 && itemID < 1300)
            isBuffItem = true;
    }

    //아이템 이름, 정보, 가격 표시
    public void SetShopItemData()
    {
        itemNameText.text = itemName;
        itemInfoText.text = itemInfo;
        itemPriceText.text = string.Format("<sprite=0>{0}",price);
    }

    //현재 선택된 아이템 표시
    public void ShowSelectedShopItem()
    {
        shop.showImage.sprite = itemImage;
    }

    //선택된 아이템을 바꿈
    public void SelectedShopItemChange()
    {
        shop.SelectShopItem(this);
    }

    //Shop 설정
    public void SetShop(BaseShop _shop)
    {
        shop = _shop;
    }

    public void ActivateItemAbility()
    {
        gm.Heal(hp);
        gm.Heal(gm._maxhp / 100 * hpRate);
        //최대 hp증가
        gm._damage += attackDamage;
        gm._critdmg += criticalDamage;
        gm._critchance += criticalRate;
        gm._dashCool -= dashCoolTime;
    }
}
