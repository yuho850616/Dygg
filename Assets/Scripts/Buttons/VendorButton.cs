﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VendorButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Gold gold;
    [SerializeField]
    private Image icon;

    [SerializeField]
    private Text title;

    [SerializeField]
    private Text price;

    [SerializeField]
    private Text quantity;

    private VendorItem vendorItem;


    public void AddItem(VendorItem vendorItem)
    {
        this.vendorItem = vendorItem;

        if (Object.FindObjectOfType<Vendor>().items[0] == vendorItem)
        {
            vendorItem.MyQuantity = 4 - GameObject.FindObjectOfType<Stats>().totalBags;
            if (vendorItem.MyQuantity == 0)
            {
                gameObject.SetActive(false);
            }
        }

        if (vendorItem.MyQuantity > 0 || (vendorItem.MyQuantity == 0 && vendorItem.Unlimited))
        {
            icon.sprite = vendorItem.MyItem.MyIcon;
            title.text = string.Format("<color={0}>{1}</color>", QualityColor.MyColors[vendorItem.MyItem.MyQuality], vendorItem.MyItem.MyTitle);
            price.text = "Price: " + vendorItem.MyItem.MyPrice.ToString();

            if (!vendorItem.Unlimited)
            {
                quantity.text = vendorItem.MyQuantity.ToString();
            }
            else
            {
                quantity.text = string.Empty;
            }
            if (vendorItem.MyItem.MyPrice > 0)
            {
                price.text = "Price: " + vendorItem.MyItem.MyPrice.ToString();
            }
            else
            {
                price.text = string.Empty;
            }
            gameObject.SetActive(true);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(gold.getGold() >= vendorItem.MyItem.MyPrice)
        {
            Item1 item = Instantiate(vendorItem.MyItem);
            if (InventoryScript.MyInstance.AddItem(item))
            {
                BuyItem();
                if (Object.FindObjectOfType<Vendor>().items[0] == vendorItem)
                {
                    ((Bag)item).Use();
                    GameObject.FindObjectOfType<Stats>().totalBags++;
                }
            }
            else
            {
                Destroy(item);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }

    private void BuyItem()
    {
        gold.removeGold(vendorItem.MyItem.MyPrice);
        Object.FindObjectOfType<Stats>().itemsBought++;
        if (!vendorItem.Unlimited)
        {
            vendorItem.MyQuantity--;
            quantity.text = vendorItem.MyQuantity.ToString();
            
            if (vendorItem.MyQuantity == 0)
            {
                gameObject.SetActive(false);
            }
        }

    }
}

