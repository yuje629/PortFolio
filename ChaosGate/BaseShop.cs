using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseShop : MonoBehaviour
{
    //����
    #region 
    public ShopItemDB shopitemDB;
    public ShopItem[] shopItems;

    public GameObject messageBox;
    public TMP_Text message;

    public TMP_Text gold_Txt;
    public Image showImage;

    public ShopItem selectedShopItem;

    [HideInInspector] public GameManager gm;
    [HideInInspector] public UIManager uiManager;

    #endregion

    virtual public void Start()
    {
        gm = GameManager.instance;
        uiManager = FindObjectOfType<UIManager>();
        LoadShopItemData();
    }

    public void Update()
    {
        gold_Txt.text = gm._gold + " Gold";

        //if (Input.GetKeyUp(KeyCode.UpArrow))
        //{
        //    TestGold();
        //}
    }

    public void LoadShopItemData()
    {
        int index = 0;

        for (int i = 0; i < shopitemDB.entities.Count; ++i)
        {
            //���� ������ ������ �ҷ����� �� �ʱ�ȭ
            if (shopItems[index].itemID == shopitemDB.entities[i].ItemID)
            {
                shopItems[index].itemName = shopitemDB.entities[i].ItemName;
                shopItems[index].itemInfo = shopitemDB.entities[i].ItemInfo;
                shopItems[index].price = shopitemDB.entities[i].BasePrice;
                shopItems[index].maxHP = shopitemDB.entities[i].MaxHP;
                shopItems[index].maxLevel = shopitemDB.entities[i].MaxLevel;
                shopItems[index].attackDamage = shopitemDB.entities[i].AttackDamage;
                shopItems[index].isAutoPotion = shopitemDB.entities[i].IsAuto;
                shopItems[index].recoveryThreshold = shopitemDB.entities[i].RecoveryThreshold;
                shopItems[index].recoveryCount = shopitemDB.entities[i].RecoveryCount;
                shopItems[index].hp = shopitemDB.entities[i].HP;
                shopItems[index].hpRate = shopitemDB.entities[i].HPRate;
                shopItems[index].criticalDamage = shopitemDB.entities[i].CriticalDamage;
                shopItems[index].criticalRate = shopitemDB.entities[i].CriticalRate;
                shopItems[index].dashCoolTime = shopitemDB.entities[i].DashCoolTime;
                shopItems[index].itemCoolTimeDropRate = shopitemDB.entities[i].ItemCoolTimeDropRate;

                shopItems[index].SetShop(this);

                index++;

                if (index == shopItems.Length)
                    break;
            }
        }
    }

    public void GoldTrade(int _cost_gold)
    {
        gm._gold -= _cost_gold;
    }

    public void SelectShopItem(ShopItem _shopitem)
    {
        selectedShopItem = _shopitem;
    }

    virtual public bool CheckCanBuyMoreItems()
    {
        return false;
    }

    public void TryBuySelectedShopItem()
    {
        if (!CheckCanBuyMoreItems())
        {
            OpenMessageBox("�� �̻� ������ �� �����ϴ�.");
            return;
        }

        //��� �پ���
        if (gm._gold < selectedShopItem.price)
        {
            OpenMessageBox("���� �����մϴ�.");
            return;
        }

        GoldTrade(selectedShopItem.price);
        BuySelectedShopItem();
    }

    virtual public void BuySelectedShopItem()
    {
        
    }

    public void OpenMessageBox(string text)
    {
        message.text = text;
        uiManager.OpenPopup(messageBox);
    }

    private ShopItem FindShopItem(int itemID)
    {
        foreach (var item in shopItems)
        {
            if (item.itemID == itemID)
            {
                return item;
            }
        }

        return null;
    }

    public void HpTrade(int _cost_hp)                           // �� ������ �� �����ϴٴ� UI(�ڸ�) �߰�
    {
        if (gm._hp > _cost_hp)
        {
            gm._hp -= _cost_hp;
        }
        else
        {
            Debug.Log("ü���� �����մϴ�.");
        }
    }

    public void TestGold()
    {
        gm._gold += 100000;
    }

    public void TestHp()
    {
        gm._hp -= 10;
    }
}
