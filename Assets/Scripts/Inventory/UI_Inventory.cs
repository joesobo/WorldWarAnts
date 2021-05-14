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
    public Transform itemPrefab;
    [HideInInspector]
    public bool isActive = false;

    private Item hoverItem = null;
    private RectTransform activeItem;

    private void Awake() {
        SetInventoryState(isActive);
    }

    private void Update() {
        if (activeItem) {
            activeItem.position = Input.mousePosition;
        }
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

        hoverItem = null;
        for (var i = 0; i < inventory.size; i++) {
            var itemSlotTransform = Instantiate(itemSlotPrefab, Vector3.zero, Quaternion.identity, slotContainer).GetComponent<RectTransform>();
            itemSlotTransform.gameObject.SetActive(true);
            var slot = itemSlotTransform.GetComponent<ItemSlot>();
            var slotImage = itemSlotTransform.Find("Image").GetComponent<Image>();
            var text = itemSlotTransform.Find("Amount").GetComponent<TextMeshProUGUI>();

            if (i < itemList.Count) {
                var item = itemList[i];

                //hover item
                slot.Hover = () => {
                    hoverItem = item;
                };

                //pickup item
                slot.Pickup = () => {
                    // inventory.PickupItem(hoverItem);
                    Debug.Log("Pickup");
                    inventory.RemoveItem(hoverItem);
                    activeItem = Instantiate(itemPrefab, Input.mousePosition, Quaternion.identity).GetComponent<RectTransform>();
                };

                //drop item
                slot.Drop = () => {
                    var tempItem = new Item { itemType = hoverItem.itemType, amount = 1 };
                    inventory.RemoveItem(tempItem);
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
