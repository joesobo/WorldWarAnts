using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory {

    public event EventHandler OnItemListChanged;

    private List<Item> itemList;
    public int size;

    public Inventory(int size) {
        itemList = new List<Item>();
        this.size = size;

        //TEST: please remove
        // AddItem(new Item { itemType = ItemType.Dirt, amount = 1 });
        // AddItem(new Item { itemType = ItemType.Rock, amount = 1 });
        // AddItem(new Item { itemType = ItemType.Test, amount = 1 });
    }

    public void AddItem(Item item) {
        if (item.IsStackable()) {
            var itemAlreadyInInventory = false;
            foreach (var inventoryItem in itemList.Where(inventoryItem => inventoryItem.itemType == item.itemType)) {
                inventoryItem.amount += item.amount;
                itemAlreadyInInventory = true;
            }
            if (!itemAlreadyInInventory) {
                itemList.Add(item);
            }
        } else {
            itemList.Add(item);
        }

        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveItem(Item item) {
        if (item.IsStackable()) {
            Item itemInInventory = null;
            foreach (var inventoryItem in itemList.Where(inventoryItem => inventoryItem.itemType == item.itemType)) {
                inventoryItem.amount -= item.amount;
                itemInInventory = inventoryItem;
            }
            if (itemInInventory != null && itemInInventory.amount <= 0) {
                itemList.Remove(itemInInventory);
            }
        } else {
            itemList.Remove(item);
        }

        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    public static void PickupItem(Item item) {
        //TODO: implement picking up
    }

    public List<Item> GetItems() {
        return itemList;
    }
}
