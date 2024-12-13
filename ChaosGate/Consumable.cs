using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Consumable : MonoBehaviour
{
    [SerializeField] private Sprite consumableImage;

    private Image showImage;

    private void Start()
    {
        showImage = transform.parent.parent.parent.parent.GetChild(4).GetChild(1).GetComponent<Image>();
    }

    public void ChangeShopShowImage()
    {
        showImage.sprite = consumableImage;
    }
}
