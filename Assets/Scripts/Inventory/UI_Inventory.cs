using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class UI_Inventory : MonoBehaviour {
    private Inventory inventory;
    private List<ItemSlot> slotList;
    private List<int> slotIndexList;
    private RectTransform rectTransform;

    public Transform itemSlotPrefab;
    public Transform slotContainer;
    public Transform itemPrefab;
    public Transform itemInfoPrefab;

    private readonly Vector3 offset = new Vector3(100, 10);
    private Transform itemInfo;
    private TextMeshProUGUI nameText;
    private TextMeshProUGUI amountText;
    private Vector2 localMousePosition;
    private ItemSlot hoverSlot;
    private bool isHovering = false;
    private Item activeItem;
    private Transform activeTransform;

    public void SetupInventory(Inventory inventory) {
        this.inventory = inventory;
        slotList = new List<ItemSlot>();
        slotIndexList = new List<int>();
        rectTransform = GetComponent<RectTransform>();

        RefreshInventory();

        inventory.OnItemListChanged += Inventory_OnListItemChanged;
    }

    private void Update() {
        Hover();

        localMousePosition = rectTransform.InverseTransformPoint(Input.mousePosition);

        if (rectTransform.rect.Contains(localMousePosition)) {
            //click logic
            if (Input.GetMouseButtonDown(0)) {
                slotIndexList.Clear();
            }
            if (Input.GetMouseButton(0)) {
                SelectSlot();
            }
            if (Input.GetMouseButtonUp(0)) {
                InventoryClick();
            }

            //hover info logic
            if (Input.GetKey(KeyCode.LeftControl) && hoverSlot && hoverSlot.item != null) {
                DisplayInfo();
            } else {
                if (itemInfo) {
                    Destroy(itemInfo.gameObject);
                }
            }

            //sort
            if (Input.GetKey(KeyCode.R)) {
                inventory.SortItems();
            }
        } else if (activeItem != null) {
            if (Input.GetMouseButtonDown(0)) {
                DropActive(activeItem.amount);
            }
            if (Input.GetMouseButtonDown(1)) {
                DropActive(1);
            }
        }

        if (activeTransform != null) {
            activeTransform.position = Input.mousePosition;
        }

        InventorySpecificControls();
    }

    public void InventoryClick() {
        if (activeItem == null && hoverSlot != null && hoverSlot.item != null) {
            Pickup(hoverSlot.item.amount);
        } else {
            Drag();
        }
    }

    private void Inventory_OnListItemChanged(object sender, EventArgs e) {
        RefreshInventory();
    }

    private void RefreshInventory() {
        var itemList = inventory.itemList;

        slotList.Clear();
        foreach (Transform child in slotContainer) {
            Destroy(child.gameObject);
        }

        for (var i = 0; i < inventory.size; i++) {
            var item = itemList[i];
            var itemSlot = Instantiate(itemSlotPrefab, Vector3.zero, Quaternion.identity, slotContainer);
            slotList.Add(itemSlot.GetComponent<ItemSlot>());
            itemSlot.GetComponent<ItemSlot>().index = i;

            var itemSlotTransform = itemSlot.GetComponent<RectTransform>();
            itemSlotTransform.gameObject.SetActive(true);
            var slot = itemSlotTransform.GetComponent<ItemSlot>();
            var slotImage = itemSlotTransform.Find("Image").GetComponent<Image>();
            var text = itemSlotTransform.Find("Amount").GetComponent<TextMeshProUGUI>();

            slot.StartUp(this, item);

            if (item != null) {
                slotImage.sprite = item.GetSprite();
                slotImage.color = Color.white;
                text.SetText(item.amount > 1 ? item.amount.ToString() : "");
            }
        }
    }

    public void Pickup(int count) {
        if (isHovering && activeItem == null) {
            if (hoverSlot.item != null) {
                activeItem = new Item { itemType = hoverSlot.item.itemType, amount = count };
                inventory.RemoveItem(hoverSlot.index, count);
                CreateActiveItem();
            }
        }
    }

    private void PutDown() {
        if (isHovering && activeItem != null) {
            // place
            if (hoverSlot.item == null) {
                inventory.SetItem(activeItem, hoverSlot.index);
                Destroy(activeTransform.gameObject);
                activeItem = null;
            } else {
                var tempItem = hoverSlot.item;

                // combine
                if (tempItem.itemType == activeItem.itemType) {
                    var totalAmount = tempItem.amount + activeItem.amount;

                    if (totalAmount <= Item.maxAmount) {
                        inventory.SetItem(new Item { itemType = tempItem.itemType, amount = totalAmount }, hoverSlot.index);
                        Destroy(activeTransform.gameObject);
                        activeItem = null;
                    } else {
                        inventory.SetItem(new Item { itemType = tempItem.itemType, amount = Item.maxAmount }, hoverSlot.index);
                        activeItem = new Item { itemType = tempItem.itemType, amount = totalAmount - Item.maxAmount };
                        activeTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = activeItem.amount.ToString();
                    }
                }
                // switch
                else {
                    inventory.SetItem(activeItem, hoverSlot.index);
                    activeItem = new Item { itemType = tempItem.itemType, amount = tempItem.amount };
                    activeTransform.GetComponent<Image>().sprite = activeItem.GetSprite();
                    activeTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = activeItem.amount.ToString();
                }
            }
        }
    }

    public void Drop(int count) {
        if (isHovering && hoverSlot.item != null) {
            var tempItem = new Item { itemType = hoverSlot.item.itemType, amount = count };

            inventory.DropItem(tempItem);
            inventory.RemoveItem(hoverSlot.index, count);
        }
    }

    private void DropActive(int count) {
        var tempItem = new Item { itemType = activeItem.itemType, amount = count };
        inventory.DropItem(tempItem);

        if (activeItem.amount == count) {
            Destroy(activeTransform.gameObject);
            activeItem = null;
        } else {
            activeItem.amount--;

            if (activeItem.amount <= 0) {
                Destroy(activeTransform.gameObject);
                activeItem = null;
            } else {
                activeTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = activeItem.amount.ToString();
            }
        }
    }

    private void Sort() {
        inventory.SortItems();
    }

    private void DisplayInfo() {
        if (!itemInfo) {
            itemInfo = Instantiate(itemInfoPrefab, Input.mousePosition + offset, Quaternion.identity, transform);
            nameText = itemInfo.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
            amountText = itemInfo.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
        } else {
            itemInfo.position = Input.mousePosition + offset;
            nameText.text = "Name: " + hoverSlot.item.itemType;
            amountText.text = "Amount: " + hoverSlot.item.amount + "/" + Item.maxAmount;
        }
    }

    public void Split() {
        if (isHovering && hoverSlot.item != null) {
            int amount;

            if (hoverSlot.item.amount > 1 && hoverSlot.item.IsStackable()) {
                amount = hoverSlot.item.amount / 2;
            } else {
                amount = 1;
            }

            Pickup(amount);
        }
    }

    private void CreateActiveItem() {
        var tempTransform = Instantiate(itemPrefab, Input.mousePosition, Quaternion.identity, transform).GetComponent<RectTransform>();
        tempTransform.GetComponent<Image>().sprite = activeItem.GetSprite();
        tempTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = activeItem.amount.ToString();
        activeTransform = tempTransform;
    }

    public void Hover() {
        hoverSlot = null;
        isHovering = false;
        foreach (var itemSlot in from itemSlot in slotList
                                 let mousePos = itemSlot.transform.InverseTransformPoint(Input.mousePosition)
                                 where itemSlot.rectTransform && itemSlot.rectTransform.rect.Contains(mousePos)
                                 select itemSlot) {
            hoverSlot = itemSlot;
            isHovering = true;
            break;
        }
        if (!isHovering) {
            hoverSlot = null;
        }
    }

    private void Drag() {
        if (slotIndexList.Count > 1) {
            var itemsPerSlot = activeItem.amount / slotIndexList.Count;
            var itemsHeld = activeItem.amount % slotIndexList.Count;

            foreach (var index in slotIndexList) {
                if (inventory.itemList[index] != null) {
                    var totalAmount = inventory.itemList[index].amount + itemsPerSlot;
                    if (totalAmount > Item.maxAmount) {
                        inventory.SetItem(new Item { itemType = activeItem.itemType, amount = Item.maxAmount }, index);
                        itemsHeld += totalAmount - Item.maxAmount;
                    } else {
                        inventory.SetItem(new Item { itemType = activeItem.itemType, amount = totalAmount }, index);
                    }
                } else {
                    inventory.SetItem(new Item { itemType = activeItem.itemType, amount = itemsPerSlot }, index);
                }
            }

            if (itemsHeld == 0) {
                Destroy(activeTransform.gameObject);
                activeItem = null;
            } else {
                activeItem.amount = itemsHeld;
                activeTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = activeItem.amount.ToString();
            }
        } else {
            PutDown();
        }
    }

    private void SelectSlot() {
        if (activeItem != null && isHovering && !slotIndexList.Contains(hoverSlot.index)) {
            if ((hoverSlot.item == null || (activeItem.itemType == hoverSlot.item.itemType && hoverSlot.item.amount < Item.maxAmount)) && slotIndexList.Count < activeItem.amount) {
                slotIndexList.Add(hoverSlot.index);
                hoverSlot.SetSelectedColor();
            }
        }
    }

    public abstract void InventorySpecificControls();
}