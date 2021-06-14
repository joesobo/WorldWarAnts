using System;
using System.Linq;
using Sirenix.Utilities;

public class Inventory {
    public event EventHandler OnItemListChanged;
    public Item[] itemList;
    public readonly int size;
    private readonly PlayerController player;

    public Inventory(int size, PlayerController player) {
        itemList = new Item[size];
        this.size = size;
        this.player = player;
    }

    public void AddItem(Item item) {
        if (item.IsStackable()) {
            var itemAlreadyInInventory = false;
            foreach (var inventoryItem in itemList.Where(inventoryItem => inventoryItem != null && inventoryItem.itemType == item.itemType && inventoryItem.amount < Item.MaxAmount)) {
                var totalAmount = inventoryItem.amount + item.amount;
                if (totalAmount <= Item.MaxAmount) {
                    inventoryItem.amount += item.amount;
                    itemAlreadyInInventory = true;
                    break;
                } else {
                    inventoryItem.amount = Item.MaxAmount;
                    item.amount = totalAmount - Item.MaxAmount;
                    itemAlreadyInInventory = true;
                    AddItem(item);
                    break;
                }
            }
            if (!itemAlreadyInInventory) {
                if (!OpenSpot(item)) {
                    DropItem(item);
                }
            }
        } else {
            if (!OpenSpot(item)) {
                DropItem(item);
            }
        }

        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RemoveItem(int index, int count) {
        if (!itemList[index].IsStackable()) {
            itemList[index] = null;
        } else {
            itemList[index].amount -= count;
            if (itemList[index].amount <= 0) {
                itemList[index] = null;
            }
        }

        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void DropItem(Item item) {
        WorldItem.DropItem(player.transform.position, item, player.facingRight);
    }

    public void SetItem(Item item, int index) {
        itemList[index] = item;
        OnItemListChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SortItems() {
        //combine all possible stacks
        foreach (var item in itemList) {
            if (item == null || !item.IsStackable()) continue;
            if (item.amount < Item.MaxAmount && item.amount != -1) {
                foreach (var testItem in itemList) {
                    if (testItem != null && item != testItem && item.itemType == testItem.itemType && testItem.amount < Item.MaxAmount && testItem.amount != -1) {
                        var totalAmount = item.amount + testItem.amount;

                        if (totalAmount <= Item.MaxAmount) {
                            item.amount = totalAmount;
                            testItem.amount = -1;
                        } else {
                            item.amount = Item.MaxAmount;
                            testItem.amount = totalAmount - Item.MaxAmount;
                        }
                    }
                }
            }
        }

        //update empty stacks
        for (var i = 0; i < itemList.Length; i++) {
            if (itemList[i] != null && itemList[i].amount == -1) {
                itemList[i] = null;
            }
        }

        //sort by name then amount
        itemList.Sort(CompareItems);

        OnItemListChanged?.Invoke(this, EventArgs.Empty);
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

    private bool OpenSpot(Item item) {
        for (var index = 0; index < itemList.Length; index++) {
            if (itemList[index] == null) {
                itemList[index] = item;
                return true;
            }
        }

        return false;
    }

    public bool HasRoom(Item item) {
        foreach (var inventoryItem in itemList) {
            if (inventoryItem == null) {
                return true;
            }
            if (inventoryItem.itemType == item.itemType && inventoryItem.amount != Item.MaxAmount) {
                return true;
            }
        }

        return false;
    }

    public int IndexOfFirstLocationFound(ItemType type) {
        var index = 0;
        foreach (var item in itemList) {
            if (item != null && item.itemType == type) {
                return index;
            }
            index++;
        }
        return -1;
    }

    public bool HasItem(Item searchItem) {
        foreach (var item in itemList) {
            if (item != null && item.itemType == searchItem.itemType && item.amount >= searchItem.amount) {
                return true;
            }
        }

        return false;
    }

    public void RemoveFirstFoundItem(Item searchItem) {
        int index = IndexOfFirstLocationFound(searchItem.itemType);

        RemoveItem(index, searchItem.amount);
    }
}
