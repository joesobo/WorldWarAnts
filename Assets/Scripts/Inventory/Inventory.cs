using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        itemList[index] = item;
        OnItemListChanged?.Invoke(this, EventArgs.Empty);
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
}
