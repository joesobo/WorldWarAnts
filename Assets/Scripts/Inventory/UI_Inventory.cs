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
    public bool isActive = false;

    private void Awake() {
        SetInventoryState(isActive);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.E)) {
            ToggleInventory();
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
        List<Item> itemList = inventory.GetItems();

        foreach (Transform child in slotContainer) {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < inventory.size; i++) {
            RectTransform transform = Instantiate(itemSlotPrefab, Vector3.zero, Quaternion.identity, slotContainer).GetComponent<RectTransform>();
            transform.gameObject.SetActive(true);
            ItemSlot slot = transform.GetComponent<ItemSlot>();
            Image slotImage = transform.Find("Image").GetComponent<Image>();
            TextMeshProUGUI text = transform.Find("Amount").GetComponent<TextMeshProUGUI>();

            if (i < itemList.Count) {
                Item item = itemList[i];

                //pickup item
                slot.LeftClick = () => {
                    inventory.PickupItem(item);
                };

                //drop item
                slot.RightClick = () => {
                    Item tempItem = new Item { itemType = item.itemType, amount = item.amount };
                    inventory.RemoveItem(item);
                    WorldItem.DropItem(player.transform.position, tempItem);
                };

                slotImage.sprite = item.GetSprite();
                slotImage.color = Color.white;
                if (item.amount > 1) {
                    text.SetText(item.amount.ToString());
                } else {
                    text.SetText("");
                }
            }
        }
    }

    private void ToggleInventory() {
        isActive = !isActive;
        SetInventoryState(isActive);
    }

    private void SetInventoryState(bool state) {
        inventoryController.SetActive(state);
    }
}
