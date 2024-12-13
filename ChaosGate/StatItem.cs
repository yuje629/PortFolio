using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StatItem : MonoBehaviour
{
    public ShopItem shopitem;

    public int _itemID;
    public int cur_Level;
    public int max_Level;
    public TMP_Text curLv_Txt;

    GameManager gm;
    private void Start()
    {
        gm = GameManager.instance;
        _itemID = shopitem.itemID;
        max_Level = shopitem.maxLevel;
    }

    public void ApplyStat()
    {
        cur_Level++;
        curLv_Txt.text = string.Format("Lv.{0}", cur_Level);
        gm._damage += shopitem.attackDamage;
        gm._critdmg += shopitem.criticalDamage;
        gm._critchance += shopitem.criticalRate;
        gm._maxhp += shopitem.maxHP;
        gm._dashCool += shopitem.dashCoolTime;
    }
}
