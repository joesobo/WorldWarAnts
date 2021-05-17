using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour {
    public event EventHandler OnItemListChanged;

    private readonly Item[] itemList;
    public List<ItemSlot> slotList = new List<ItemSlot>();
    public readonly List<int> slotIndexList = new List<int>();
    public readonly int size;
    private readonly PlayerController player;

    public ItemSlot hoverSlot;
    public bool isHovering = false;

    public RectTransform activeTransform;
    public Item activeItem = null;

    public Inventory(int size, PlayerController player) {
        itemList = new Item[size];
        this.size = size;
        this.player = player;
    }

    public Item[] GetItems() {
        return itemList;
    }

    public void AddItem(Item item) {
        if (item.IsStackable()) {
            var itemAlreadyInInventory = false;
            foreach (var inventoryItem in itemList.Where(inventoryItem => inventoryItem != null && inventoryItem.itemType == item.itemType && inventoryItem.amount < Item.maxAmount)) {
                var totalAmount = inventoryItem.amount + item.amount;
                if (totalAmount <= Item.maxAmount) {
                    inventoryItem.amount += item.amount;
                    itemAlreadyInInventory = true;
                    break;
                } else {
                    inventoryItem.amount = Item.maxAmount;
                    item.amount = totalAmount - Item.maxAmount;
                    itemAlreadyInInventory = true;
                    AddItem(item);
                    break;
                }
            }
            if (!itemAlreadyInInventory) {
                if (!FirstOpenSpot(itemList, item)) {
                    Drop(item);
                }
            }
        } else {
            if (!FirstOpenSpot(itemList, item)) {
                Drop(item);
            }
        }

        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    private static bool FirstOpenSpot(IList<Item> list, Item item) {
        for (var index = 0; index < list.Count; index++) {
            var element = list[index];
            if (element == null) {
                list[index] = item;
                return true;
            }
        }

        return false;
    }

    private void SetItem(Item item, int index) {
        itemList[index] = item;
        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RemoveItem(int index, int count) {
        var inventoryItem = itemList[index];

        if (!inventoryItem.IsStackable()) {
            itemList[index] = null;
        } else {
            itemList[index].amount -= count;
            if (itemList[index].amount <= 0) {
                itemList[index] = null;
            }
        }

        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SortInventory() {
        foreach (var item in itemList) {
            if (item == null || !item.IsStackable()) continue;
            if (item.amount < Item.maxAmount && item.amount != -1) {
                foreach (var testItem in itemList) {
                    if (testItem != null && item != testItem && item.itemType == testItem.itemType && testItem.amount < Item.maxAmount && testItem.amount != -1) {
                        var totalAmount = item.amount + testItem.amount;

                        if (totalAmount <= Item.maxAmount) {
                            item.amount = totalAmount;
                            testItem.amount = -1;
                        } else {
                            item.amount = Item.maxAmount;
                            testItem.amount = totalAmount - Item.maxAmount;
                        }
                    }
                }
            }
        }

        for (var i = 0; i < itemList.Length; i++) {
            if (itemList[i] != null && itemList[i].amount == -1) {
                itemList[i] = null;
            }
        }

        itemList.Sort(CompareItems);
    }

    private static int CompareItems(Item a, Item b) {
        if (a == null && b == null) {
            return 0;
        }
        if (a != null && b == null) {
            return -1;
        }
        if (a == null && b != null) {
            return 1;
        }

        var res = a.itemType.CompareTo(b.itemType);
        if (res == 0) {
            return -a.amount.CompareTo(b.amount);
        } else {
            return res;
        }
    }

    public void CheckHovering() {
        hoverSlot = null;
        isHovering = false;
        foreach (var itemSlot in from itemSlot in slotList let mousePos = itemSlot.transform.InverseTransformPoint(Input.mousePosition) 
            where itemSlot.rectTransform && itemSlot.rectTransform.rect.Contains(mousePos) select itemSlot) {
            hoverSlot = itemSlot;
            isHovering = true;
            break;
        }
        if (!isHovering) {
            hoverSlot = null;
        }
    }

    public void Pickup(int count) {
        if (isHovering) {
            if (hoverSlot.item != null && activeItem == null) {
                activeItem = new Item { itemType = hoverSlot.item.itemType, amount = count };
                RemoveItem(hoverSlot.index, count);
            }
        }
    }

    private void Putdown() {
        if (isHovering && activeItem != null) {
            // place
            if (hoverSlot.item == null) {
                SetItem(activeItem, hoverSlot.index);
                Destroy(activeTransform.gameObject);
                activeItem = null;
            } else {
                var tempItem = hoverSlot.item;

                // combine
                if (tempItem.itemType == activeItem.itemType) {
                    var totalAmount = tempItem.amount + activeItem.amount;

                    if (totalAmount <= Item.maxAmount) {
                        SetItem(new Item { itemType = tempItem.itemType, amount = totalAmount }, hoverSlot.index);
                        Destroy(activeTransform.gameObject);
                        activeItem = null;
                    } else {
                        SetItem(new Item { itemType = tempItem.itemType, amount = Item.maxAmount }, hoverSlot.index);
                        activeItem = new Item { itemType = tempItem.itemType, amount = totalAmount - Item.maxAmount };
                        activeTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = activeItem.amount.ToString();
                    }
                }
                // switch
                else {
                    SetItem(activeItem, hoverSlot.index);
                    activeItem = new Item { itemType = tempItem.itemType, amount = tempItem.amount };
                    activeTransform.GetComponent<Image>().sprite = activeItem.GetSprite();
                    activeTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = activeItem.amount.ToString();
                }
            }
        }
    }

    public void Drop(int count) {
        if (isHovering && hoverSlot.item != null) {
            var tempItem = new Item { itemType = hoverSlot.item.itemType, amount = count };
            WorldItem.DropItem(player.transform.position, tempItem, player.facingRight);

            RemoveItem(hoverSlot.index, count);
        }
    }

    public void Drop(Item item) {
        WorldItem.DropItem(player.transform.position, item, player.facingRight);
    }

    public void SelectSlot() {
        if (activeItem != null && isHovering && !slotIndexList.Contains(hoverSlot.index) && 
            (hoverSlot.item == null || (activeItem.itemType == hoverSlot.item.itemType && hoverSlot.item.amount < Item.maxAmount)) && slotIndexList.Count < activeItem.amount) {
            slotIndexList.Add(hoverSlot.index);
            hoverSlot.SetSelectedColor();
        }
    }

    public void Drag() {
        if (slotIndexList.Count > 1) {
            var itemsPerSlot = activeItem.amount / slotIndexList.Count;
            var itemsHeld = activeItem.amount % slotIndexList.Count;

            foreach (var index in slotIndexList) {
                if (itemList[index] != null) {
                    var totalAmount = itemList[index].amount + itemsPerSlot;
                    if (totalAmount > Item.maxAmount) {
                        SetItem(new Item { itemType = activeItem.itemType, amount = Item.maxAmount }, index);
                        itemsHeld += totalAmount - Item.maxAmount;
                    } else {
                        SetItem(new Item { itemType = activeItem.itemType, amount = totalAmount }, index);
                    }
                } else {
                    SetItem(new Item { itemType = activeItem.itemType, amount = itemsPerSlot }, index);
                }
            }

            if (itemsHeld == 0) {
                Destroy(activeTransform.gameObject);
                activeItem = null;
            } else {
                activeItem.amount = itemsHeld;
                activeTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = activeItem.amount.ToString();
            }
        } else {
            Putdown();
        }
    }

    public bool HasRoom(Item item) {
        foreach (var inventoryItem in itemList) {
            if (inventoryItem == null) {
                return true;
            }
            if (inventoryItem.itemType == item.itemType && inventoryItem.amount != Item.maxAmount) {
                return true;
            }
        }

        return false;
    }
}
