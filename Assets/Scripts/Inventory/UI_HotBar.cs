using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_HotBar : UI_Inventory {
    private readonly KeyCode[] keyCodes = {
        KeyCode.Alpha1,
        KeyCode.Alpha2,
        KeyCode.Alpha3,
        KeyCode.Alpha4,
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8,
        KeyCode.Alpha9,
        KeyCode.Alpha0,
    };
    private int currentIndex;
    public Item currentItem;

    public override void InventorySpecificControls() {
        for (var i = 0; i < keyCodes.Length; i++) {
            if (Input.GetKeyDown(keyCodes[i])) {
                currentIndex = i;
                currentItem = inventory.itemList[i];
                break;
            }
        }

        if (currentItem != inventory.itemList[currentIndex]) {
            currentItem = inventory.itemList[currentIndex];
        }
        Equip(currentIndex);
    }

    private void Equip(int index) {
        RefreshInventory();
        slotList[index].SetSelectedColor();
    }

    public void Place()
    {
        if (currentItem == null) return;
        inventory.RemoveItem(currentIndex, 1);
        if (currentItem.amount <= 0) {
            currentItem = null;
        }
    }
}
