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
    private int hoverIndex = -1;
    private RectTransform activeTransform;
    private Item activeItem = null;
    private int activeAmount = -1;

    private void Awake() {
        SetInventoryState(isActive);
    }

    private void Update() {
        if (activeTransform) {
            activeTransform.position = Input.mousePosition;
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
        hoverIndex = -1;
        for (var i = 0; i < inventory.size; i++) {
            var item = itemList[i];
            var itemSlot = Instantiate(itemSlotPrefab, Vector3.zero, Quaternion.identity, slotContainer);
            itemSlot.GetComponent<ItemSlot>().index = i;

            var itemSlotTransform = itemSlot.GetComponent<RectTransform>();
            itemSlotTransform.gameObject.SetActive(true);
            var slot = itemSlotTransform.GetComponent<ItemSlot>();
            var slotImage = itemSlotTransform.Find("Image").GetComponent<Image>();
            var text = itemSlotTransform.Find("Amount").GetComponent<TextMeshProUGUI>();

            //hover item
            slot.Hover = () => {
                hoverItem = item;
                hoverIndex = slot.GetComponent<ItemSlot>().index;
            };

            //pickup item
            slot.Pickup = () => {
                //pickup
                if (hoverItem != null && activeItem == null) {
                    activeItem = hoverItem;
                    activeAmount = hoverItem.amount;

                    activeTransform = Instantiate(itemPrefab, Input.mousePosition, Quaternion.identity, transform).GetComponent<RectTransform>();
                    activeTransform.GetComponent<Image>().sprite = activeItem.GetSprite();
                    activeTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = activeItem.amount.ToString();

                    inventory.RemoveItem(hoverItem, hoverIndex, hoverItem.amount);
                }
                //put down
                else if (hoverItem == null && activeItem != null) {
                    activeItem.amount = activeAmount;
                    inventory.AddItemIndex(activeItem, hoverIndex);
                    Destroy(activeTransform.gameObject);
                    activeItem = null;
                    activeAmount = -1;
                }
            };

            slot.Split = () => {
                //split
                if (hoverItem != null && activeItem == null) {
                    activeItem = new Item { itemType = hoverItem.itemType, amount = hoverItem.amount / 2 };
                    activeAmount = hoverItem.amount / 2;

                    activeTransform = Instantiate(itemPrefab, Input.mousePosition, Quaternion.identity, transform).GetComponent<RectTransform>();
                    activeTransform.GetComponent<Image>().sprite = activeItem.GetSprite();
                    activeTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = activeItem.amount.ToString();

                    inventory.RemoveItem(hoverItem, hoverIndex, hoverItem.amount / 2);
                }
            };

            //drop item
            slot.Drop = () => {
                if (hoverItem != null) {
                    var tempItem = new Item { itemType = hoverItem.itemType, amount = 1 };
                    inventory.RemoveItem(tempItem, hoverIndex, tempItem.amount);

                    WorldItem.DropItem(player.transform.position, tempItem, player.facingRight);
                }
            };

            //drop stack
            slot.DropStack = () => {
                if (hoverItem != null) {
                    var tempItem = new Item { itemType = hoverItem.itemType, amount = hoverItem.amount };
                    inventory.RemoveItem(tempItem, hoverIndex, tempItem.amount);

                    WorldItem.DropItem(player.transform.position, tempItem, player.facingRight);
                }
            };

            if (item != null) {
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
