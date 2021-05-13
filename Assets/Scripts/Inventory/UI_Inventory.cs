using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

public class UI_Inventory : MonoBehaviour {
    private Inventory inventory;
    private PlayerController player;

    public GameObject inventoryController;
    public Transform slotContainer;
    public Transform itemSlotPrefab;
    [HideInInspector]
    public bool isActive = false;

    private void Awake() {
        SetInventoryState(isActive);
    }

    public void SetPlayer(PlayerController player) {
        this.player = player;
    }

    public void SetInventory(Inventory inventory) {
        this.inventory = inventory;

        inventory.OnItemListChanged += Inventory_OnListItemChanged;
        RefreshInventoryItems();
    }

    private void Inventory_OnListItemChanged(object sender, EventArgs e) {
        RefreshInventoryItems();
    }

    private void RefreshInventoryItems() {
        var itemList = inventory.GetItems();

        foreach (Transform child in slotContainer) {
            Destroy(child.gameObject);
        }

        for (var i = 0; i < inventory.size; i++) {
            var itemSlotTransform = Instantiate(itemSlotPrefab, Vector3.zero, Quaternion.identity, slotContainer).GetComponent<RectTransform>();
            itemSlotTransform.gameObject.SetActive(true);
            var slot = itemSlotTransform.GetComponent<ItemSlot>();
            var slotImage = itemSlotTransform.Find("Image").GetComponent<Image>();
            var text = itemSlotTransform.Find("Amount").GetComponent<TextMeshProUGUI>();

            if (i < itemList.Count) {
                var item = itemList[i];

                //pickup item
                slot.Pickup = () => {
                    Inventory.PickupItem(item);
                };

                //drop item
                slot.Drop = () => {
                    var tempItem = new Item { itemType = item.itemType, amount = item.amount };
                    inventory.RemoveItem(item);
                    WorldItem.DropItem(player.transform.position, tempItem);
                };

                slotImage.sprite = item.GetSprite();
                slotImage.color = Color.white;
                text.SetText(item.amount > 1 ? item.amount.ToString() : "");
            }
        }
    }

    public void ToggleInventory() {
        isActive = !isActive;
        SetInventoryState(isActive);
    }

    private void SetInventoryState(bool state) {
        inventoryController.SetActive(state);
    }
}
