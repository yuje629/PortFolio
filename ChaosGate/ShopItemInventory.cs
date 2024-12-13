using DuloGames.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItemInventory : MonoBehaviour
{
    public InvenSlot[] slots;
    public InvenSlot[] shopslots;
    [SerializeField] GameObject[] slotsPr;

    private void Update()
    {
        Test();
    }
    
    void Test()
    {
        for (int j = 0; j < slots.Length; j++)
        {
            if (slots[j].itemCount > 0)
            {
                slotsPr[j].gameObject.SetActive(true);
                shopslots[j].item = slots[j].item;
                shopslots[j].itemCount = slots[j].itemCount;
                shopslots[j].itemImage.sprite = slots[j].itemImage.sprite;
                shopslots[j].itemImage.color = slots[j].itemImage.color;
                shopslots[j].textCount.text = shopslots[j].itemCount.ToString();
            }
            else
                slotsPr[j].gameObject.SetActive(false);
        }
    }
}
