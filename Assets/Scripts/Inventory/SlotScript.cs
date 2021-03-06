﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotScript : MonoBehaviour, IPointerClickHandler, IClickable
{
    private ObservableStack<Item1> items = new ObservableStack<Item1>();
    [SerializeField]
    protected Image icon;

    [SerializeField]
    protected Text stackSize;

    private Gold gold;

    public BagScript MyBag { get; set; }

    public bool startWithPickaxe = false;

    public bool IsEmpty
    {
        get
        {
            return items.Count == 0;
        }
    }

    public bool IsFull
    {
        get
        {
            if(IsEmpty|| MyCount < MyItem.MyStackSize)
            {
                return false;
            }
            return true;
        }
    }

    public Item1 MyItem
    {
        get
        {
            if(!IsEmpty)
            {
                return items.Peek();
            }

            return null;
        }
    }

    public Image MyIcon
    {
        get
        {
            return icon;
        }

        set
        {
            icon = value;
        }
    }

    public int MyCount
    {
        get
        {
            return items.Count;
        }
    }

    public Text MyStackText
    {
        get
        {
            return stackSize;
        }
    }

    public ObservableStack<Item1> MyItems
    {
        get
        {
            return items;
        }
    }

    private void Awake()
    {
        items.OnPop += new UpdateStackEvent(UpdateSlot);
        items.OnPush += new UpdateStackEvent(UpdateSlot);
        items.OnClear += new UpdateStackEvent(UpdateSlot);

        gold = GameObject.Find("Gold").GetComponent<Gold>();

        if (startWithPickaxe)
        {
            Item1 startingPickaxe = Instantiate(InventoryScript.MyInstance.items[1]);
            AddItem(startingPickaxe);
        }
    }

    public bool AddItem(Item1 item)
    {
        items.Push(item);
        icon.sprite = item.MyIcon;
        icon.color = Color.white;
        item.MySlot = this;
        return true;
    }

    public bool AddItems(ObservableStack<Item1> newItems)
    {
        if(IsEmpty || newItems.Peek().GetType() == MyItem.GetType())
        {
            int count = newItems.Count;

            for (int i = 0; i<count; i++)
            {
                if(IsFull)
                {
                    return false;
                }

                AddItem(newItems.Pop());
            }
            return true;
        }
        return false;
    }

    public void RemoveItem()
    {
        if(!IsEmpty)
        {
            items.Pop();
        } 
    }

    public void Clear()
    {
        if(items.Count>0)
        {
            items.Clear();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if(InventoryScript.MyInstance.Fromslot==null && !IsEmpty) //dont have something to move
            {
                HandScript.MyInstance.TakeMoveable(MyItem as IMoveable);
                InventoryScript.MyInstance.Fromslot = this;
            }
            else if(InventoryScript.MyInstance.Fromslot == null && IsEmpty && (HandScript.MyInstance.MyMoveable is Bag))
            {
                Bag bag = (Bag)HandScript.MyInstance.MyMoveable;

                if(bag.MyBagScript != MyBag && InventoryScript.MyInstance.MyEmptySlotCount - bag.Slots > 0)
                {
                    AddItem(bag);
                    bag.MyBagButton.RemoveBag();
                    HandScript.MyInstance.Drop();
                }

                
            }
            else if(InventoryScript.MyInstance.Fromslot!=null) // have something to move
            {
                if(PutItemBack() || MergeItems(InventoryScript.MyInstance.Fromslot) ||SwapItems(InventoryScript.MyInstance.Fromslot)||AddItems(InventoryScript.MyInstance.Fromslot.items))
                {
                    HandScript.MyInstance.Drop();
                    InventoryScript.MyInstance.Fromslot = null;
                }
            }
        }
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            UseItem();
        }
    }

    public void UseItem()
    {
        if(!IsEmpty)
        {
            if (VendorWindow.MyInstance == null)
            {
                if (MyItem.MyTitle == "Box of Rations")
                {
                    Stats stat = Object.FindObjectOfType<Stats>();
                    stat.vFatigue += 100f;
                    if (stat.vFatigue > stat.maxFatigue)
                    {
                        stat.vFatigue = stat.maxFatigue;
                    }
                    RemoveItem();
                }
                else if (MyItem.MyTitle == "Rune of Teleportation")
                {
                    TileBasedMover moveScript = Object.FindObjectOfType<TileBasedMover>();
                    moveScript.teleportToSpawn();
                    RemoveItem();
                }
            }
            else if(VendorWindow.MyInstance.vwindow.activeSelf)
            {
                int sellPrice = (int)(MyItem.MyPrice * 0.8f);
                if (MyItem.myMaxDurability > 0)
                {
                    sellPrice *= (MyItem.myDurability / MyItem.myMaxDurability);
                }
                if (sellPrice < 0)
                {
                    sellPrice = 0;
                }
                gold.addGold((int)(MyItem.MyPrice * 0.8f));
                RemoveItem();
                Object.FindObjectOfType<Stats>().itemsSold++;
            }
            else
            {
                if (MyItem.MyTitle == "Box of Rations")
                {
                    Stats stat = Object.FindObjectOfType<Stats>();
                    stat.vFatigue += 100f;
                    if (stat.vFatigue > stat.maxFatigue)
                    {
                        stat.vFatigue = stat.maxFatigue;
                    }
                    RemoveItem();
                }
                else if (MyItem.MyTitle == "Rune of Teleportation")
                {
                    TileBasedMover moveScript = Object.FindObjectOfType<TileBasedMover>();
                    moveScript.teleportToSpawn();
                    RemoveItem();
                }
            }
        } 
    }

    public bool StackItem(Item1 item)
    {
        if(!IsEmpty && item.name == MyItem.name && items.Count <MyItem.MyStackSize)
        {
            items.Push(item);
            item.MySlot = this;
            return true;
        }
        return false;
    }

    public bool PutItemBack()
    {
        if(InventoryScript.MyInstance.Fromslot == this)
        {
            InventoryScript.MyInstance.Fromslot.MyIcon.color = Color.white;
            return true;
        }
        return false;
    }

    private bool SwapItems(SlotScript from)
    {
        if (IsEmpty)
        {
            return false;
        }
        if(from.MyItem.GetType()!= MyItem.GetType() || from.MyCount+MyCount > MyItem.MyStackSize)
        {
            ObservableStack<Item1> tmpFrom = new ObservableStack<Item1>(from.items);

            from.items.Clear();

            from.AddItems(items);

            items.Clear();

            AddItems(tmpFrom);

            return true;
        }
        return false;
    }

    private bool MergeItems(SlotScript from)
    {
        if(IsEmpty)
        {
            return false;
        }
        if(from.MyItem.GetType() == MyItem.GetType() && !IsFull)
        {
            // how many free slots do we have in the stack
            int free = MyItem.MyStackSize - MyCount;

            for( int i = 0; i< free; i++)
            {
                AddItem(from.items.Pop());
            }
            return true;
        }
        return false;
    }

    public void UpdateSlot()
    {
        Experience.MyInstance.UpdateStackSize(this);
    }
}
