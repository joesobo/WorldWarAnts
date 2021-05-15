using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

public class UI_Inventory : MonoBehaviour {
    private Inventory inventory;
    private PlayerController player;
    private RectTransform rectTransform;
    private Vector2 localMousePosition;

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

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
        SetInventoryState(isActive);
    }

    private void Update() {
        localMousePosition = rectTransform.InverseTransformPoint(Input.mousePosition);

        if (activeTransform) {
            activeTransform.position = Input.mousePosition;

            if (isActive && !rectTransform.rect.Contains(localMousePosition)) {
                hoverItem = null;
                hoverIndex = -1;

                if (Input.GetMouseButtonDown(0)) {
                    //drop stack
                    var tempItem = new Item { itemType = activeItem.itemType, amount = activeItem.amount };

                    WorldItem.DropItem(player.transform.position, tempItem, player.facingRight);
                    Destroy(activeTransform.gameObject);
                    activeItem = null;
                }
                if (Input.GetMouseButtonDown(1)) {
                    //drop 1
                    var tempItem = new Item { itemType = activeItem.itemType, amount = 1 };

                    WorldItem.DropItem(player.transform.position, tempItem, player.facingRight);
                    activeItem.amount--;

                    if (activeItem.amount <= 0) {
                        Destroy(activeTransform.gameObject);
                        activeItem = null;
                    } else {
                        activeTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = activeItem.amount.ToString();
                    }
                }
            }
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

            slot.Hover = () => {
                hoverItem = item;
                hoverIndex = slot.GetComponent<ItemSlot>().index;
            };

            slot.Pickup = () => {
                //pickup
                if (hoverItem != null && activeItem == null) {
                    activeItem = new Item { itemType = hoverItem.itemType, amount = hoverItem.amount };

                    activeTransform = Instantiate(itemPrefab, Input.mousePosition, Quaternion.identity, transform).GetComponent<RectTransform>();
                    activeTransform.GetComponent<Image>().sprite = activeItem.GetSprite();
                    activeTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = activeItem.amount.ToString();

                    inventory.RemoveItem(hoverItem, hoverIndex, hoverItem.amount);
                }
                //put down
                else if (activeItem != null) {
                    //place
                    if (hoverItem == null) {
                        inventory.AddItemIndex(activeItem, hoverIndex);
                        Destroy(activeTransform.gameObject);
                        activeItem = null;
                    }
                    //switch
                    else {
                        //grab current inventory item
                        Item tempItem = inventory.GetItems()[hoverIndex];
                        //set inventory to new item
                        inventory.AddItemIndex(activeItem, hoverIndex);
                        //set held item to grabbed
                        activeItem = new Item { itemType = tempItem.itemType, amount = tempItem.amount };
                        activeTransform.GetComponent<Image>().sprite = activeItem.GetSprite();
                        activeTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = activeItem.amount.ToString();
                    }
                }
            };

            slot.Split = () => {
                if (hoverItem != null && activeItem == null) {
                    activeItem = new Item { itemType = hoverItem.itemType, amount = hoverItem.amount / 2 };

                    activeTransform = Instantiate(itemPrefab, Input.mousePosition, Quaternion.identity, transform).GetComponent<RectTransform>();
                    activeTransform.GetComponent<Image>().sprite = activeItem.GetSprite();
                    activeTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = activeItem.amount.ToString();

                    inventory.RemoveItem(hoverItem, hoverIndex, hoverItem.amount / 2);
                }
            };

            slot.Drop = () => {
                if (hoverItem != null) {
                    var tempItem = new Item { itemType = hoverItem.itemType, amount = 1 };
                    inventory.RemoveItem(tempItem, hoverIndex, tempItem.amount);

                    WorldItem.DropItem(player.transform.position, tempItem, player.facingRight);
                }
            };

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

        if (!isActive && activeTransform != null) {
            WorldItem.DropItem(player.transform.position, activeItem, player.facingRight);
            Destroy(activeTransform.gameObject);
            activeItem = null;
        }

        SetInventoryState(isActive);
    }

    private void SetInventoryState(bool state) {
        inventoryController.SetActive(state);
    }
}
