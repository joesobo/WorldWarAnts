using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class InventoryActions {
    public static Vector3 offset = new Vector3(100, 10);

    public static void Hover(UI_Inventory ui) {
        ui.hoverSlot = null;
        ui.isHovering = false;
        foreach (var itemSlot in from itemSlot in ui.slotList
                                 let mousePos = itemSlot.transform.InverseTransformPoint(Input.mousePosition)
                                 where itemSlot.rectTransform && itemSlot.rectTransform.rect.Contains(mousePos)
                                 select itemSlot) {
            ui.hoverSlot = itemSlot;
            ui.isHovering = true;
            break;
        }
        if (!ui.isHovering) {
            ui.hoverSlot = null;
        }
    }

    public static void SelectSlot(UI_Inventory ui, InventoriesController invController) {
        if (invController.activeItem != null && ui.isHovering && !invController.slotList.Contains(ui.hoverSlot)) {
            foreach (var slot in invController.slotList) {
                if (slot.index == ui.hoverSlot.index) {
                    return;
                }
            }

            if ((ui.hoverSlot.item == null || ((invController.activeItem != null || invController.activeItem.itemType != ItemType.Empty) && invController.activeItem.itemType == ui.hoverSlot.item.itemType && ui.hoverSlot.item.amount < Item.MAXAmount)) &&
                invController.slotList.Count < invController.activeItem.amount) {
                invController.slotList.Add(ui.hoverSlot);
                ui.hoverSlot.SetSelectedColor();
            }
        }
    }

    public static void InventoryClick(UI_Inventory ui, InventoriesController invController) {
        if (ui.isHovering && ui.hoverSlot != null && ui.hoverSlot.item != null && (invController.activeItem == null || invController.activeItem.itemType == ItemType.Empty)) {
            Pickup(ui, invController, ui.hoverSlot.item.amount);
        } else {
            Drag(ui, invController);
        }
    }

    private static void Pickup(UI_Inventory ui, InventoriesController invController, int count) {
        if (!ui.isHovering ||
            (invController.activeItem != null && invController.activeItem.itemType != ItemType.Empty)) return;
        if (ui.hoverSlot.item != null) {
            invController.activeItem = new Item { itemType = ui.hoverSlot.item.itemType, amount = count };
            ui.inventory.RemoveItem(ui.hoverSlot.index, count);
            invController.CreateActiveItem();
        }
    }

    public static void Split(UI_Inventory ui, InventoriesController invController) {
        if (ui.isHovering && ui.hoverSlot.item != null) {
            int amount;

            if (ui.hoverSlot.item.amount > 1 && ui.hoverSlot.item.IsStackable()) {
                amount = ui.hoverSlot.item.amount / 2;
            } else {
                amount = 1;
            }

            Pickup(ui, invController, amount);
        }
    }

    private static void Drag(UI_Inventory ui, InventoriesController invController) {
        if (invController.slotList.Count > 1) {
            var itemsPerSlot = invController.activeItem.amount / invController.slotList.Count;
            var itemsHeld = invController.activeItem.amount % invController.slotList.Count;

            foreach (var slot in invController.slotList) {
                if (slot.item != null) {
                    var totalAmount = slot.item.amount + itemsPerSlot;
                    if (totalAmount > Item.MAXAmount) {
                        slot.ui.inventory.SetItem(new Item { itemType = invController.activeItem.itemType, amount = Item.MAXAmount }, slot.index);
                        itemsHeld += totalAmount - Item.MAXAmount;
                    } else {
                        slot.ui.inventory.SetItem(new Item { itemType = invController.activeItem.itemType, amount = totalAmount }, slot.index);
                    }
                } else {
                    slot.ui.inventory.SetItem(new Item { itemType = invController.activeItem.itemType, amount = itemsPerSlot }, slot.index);
                }
            }

            if (itemsHeld == 0) {
                invController.DestroyActive();
            } else {
                invController.activeItem.amount = itemsHeld;
                invController.activeTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = invController.activeItem.amount.ToString();
            }
        } else {
            PutDown(ui, invController);
        }
    }

    private static void PutDown(UI_Inventory ui, InventoriesController invController) {
        if (ui.isHovering && invController.activeItem != null && invController.activeItem.itemType != ItemType.Empty) {
            // place
            if (ui.hoverSlot.item == null) {
                ui.inventory.SetItem(invController.activeItem, ui.hoverSlot.index);
                invController.DestroyActive();
            } else {
                var tempItem = ui.hoverSlot.item;

                // combine
                if (tempItem.itemType == invController.activeItem.itemType) {
                    var totalAmount = tempItem.amount + invController.activeItem.amount;

                    if (totalAmount <= Item.MAXAmount) {
                        ui.inventory.SetItem(new Item { itemType = tempItem.itemType, amount = totalAmount }, ui.hoverSlot.index);
                        invController.DestroyActive();
                    } else {
                        ui.inventory.SetItem(new Item { itemType = tempItem.itemType, amount = Item.MAXAmount }, ui.hoverSlot.index);
                        invController.activeItem = new Item { itemType = tempItem.itemType, amount = totalAmount - Item.MAXAmount };
                        invController.activeTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = invController.activeItem.amount.ToString();
                    }
                }
                // switch
                else {
                    ui.inventory.SetItem(invController.activeItem, ui.hoverSlot.index);
                    invController.activeItem = new Item { itemType = tempItem.itemType, amount = tempItem.amount };
                    invController.activeTransform.GetComponent<Image>().sprite = invController.activeItem.GetSprite();
                    invController.activeTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = invController.activeItem.amount.ToString();
                }
            }
        }
    }

    public static void DisplayInfo(UI_Inventory ui, Transform itemInfo, TextMeshProUGUI nameText, TextMeshProUGUI amountText) {
        itemInfo.position = Input.mousePosition + offset;
        nameText.text = "Name: " + ui.hoverSlot.item.itemType;
        amountText.text = "Amount: " + ui.hoverSlot.item.amount + "/" + Item.MAXAmount;
    }

    public static void Drop(UI_Inventory ui, int count) {
        if (ui.isHovering && ui.hoverSlot.item != null) {
            var tempItem = new Item { itemType = ui.hoverSlot.item.itemType, amount = count };

            ui.inventory.DropItem(tempItem);
            ui.inventory.RemoveItem(ui.hoverSlot.index, count);
        }
    }

    public static void DropActive(PlayerController player, InventoriesController invController, int count) {
        var tempItem = new Item { itemType = invController.activeItem.itemType, amount = count };
        WorldItem.DropItem(player.transform.position, tempItem, player.facingRight);

        if (invController.activeItem.amount == count) {
            invController.DestroyActive();
        } else {
            invController.activeItem.amount--;

            if (invController.activeItem.amount <= 0) {
                invController.DestroyActive();
            } else {
                invController.activeTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = invController.activeItem.amount.ToString();
            }
        }
    }
}
