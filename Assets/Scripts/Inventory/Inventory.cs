using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;

public class Inventory {

    public event EventHandler OnItemListChanged;

    private Item[] itemList;
    public int size;

    public Inventory(int size) {
        itemList = new Item[size];
        this.size = size;
    }

    public void AddItem(Item item) {
        if (item.IsStackable()) {
            var itemAlreadyInInventory = false;
            foreach (var inventoryItem in itemList.Where(inventoryItem => inventoryItem != null && inventoryItem.itemType == item.itemType && inventoryItem.amount < Item.maxAmount)) {
                int totalAmount = inventoryItem.amount + item.amount;
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
                FirstOpenSpot(itemList, item);
            }
        } else {
            FirstOpenSpot(itemList, item);
        }

        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    private void FirstOpenSpot(Item[] list, Item item) {
        //gap in list
        for (int index = 0; index < list.Length; index++) {
            var element = list[index];
            if (element == null) {
                list[index] = item;
                return;
            }
        }

        //inventory full
        //TODO: fix how items get handled
        return;
    }

    public void AddItemIndex(Item item, int index) {
        if (index != -1) {
            itemList[index] = item;
            OnItemListChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void RemoveItem(Item item, int index, int amount) {
        Item itemInInventory = null;
        var inventoryItem = itemList[index];
        if (inventoryItem != null && inventoryItem.itemType == item.itemType) {
            inventoryItem.amount -= amount;
            itemInInventory = inventoryItem;
        }

        if (item.IsStackable()) {
            if (itemInInventory != null && itemInInventory.amount <= 0) {
                itemList[index] = null;
            }
        } else {
            itemList[index] = null;
        }

        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    public Item[] GetItems() {
        return itemList;
    }

    public void SortInventory() {
        foreach (var item in itemList) {
            if (item != null && item.amount < Item.maxAmount && item.amount != -1) {
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
}
