using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

public class UI_Inventory : MonoBehaviour {
    private Inventory inventory;
    private PlayerController player;
    private VoxelEditor voxelEditor;
    private WorldManager worldManager;
    private RectTransform rectTransform;
    private Vector2 localMousePosition;

    public GameObject inventoryController;
    public Transform slotContainer;
    public Transform itemSlotPrefab;
    public Transform itemInfoPrefab;
    public Transform itemPrefab;
    private Transform itemInfo;
    private TextMeshProUGUI nameText;
    private TextMeshProUGUI amountText;
    private readonly Vector3 offset = new Vector3(100, 10);
    [HideInInspector] public bool isActive;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
        player = FindObjectOfType<PlayerController>();
        voxelEditor = FindObjectOfType<VoxelEditor>();
        worldManager = FindObjectOfType<WorldManager>();
        SetInventoryState(isActive);
        RefreshInventoryItems();
    }

    private void Update() {
        if (isActive) {
            inventory.CheckHovering();
            localMousePosition = rectTransform.InverseTransformPoint(Input.mousePosition);

            if (rectTransform.rect.Contains(localMousePosition)) {
                //click logic
                if (Input.GetMouseButtonDown(0)) {
                    inventory.slotIndexList.Clear();
                }
                if (Input.GetMouseButton(0)) {
                    inventory.SelectSlot();
                }
                if (Input.GetMouseButtonUp(0)) {
                    InventoryClick();
                }

                //hover info logic
                if (Input.GetKey(KeyCode.LeftControl) && inventory.hoverSlot && inventory.hoverSlot.item != null) {
                    DisplayInfo();
                } else {
                    if (itemInfo) {
                        Destroy(itemInfo.gameObject);
                    }
                }
            } else if (inventory.activeItem != null) {
                if (Input.GetMouseButtonDown(0)) {
                    DropActive(inventory.activeItem.amount);
                }
                if (Input.GetMouseButtonDown(1)) {
                    DropActive(1);
                }
            }

            //sort
            if (Input.GetKey(KeyCode.R)) {
                inventory.SortInventory();
                RefreshInventoryItems();
            }

            if (inventory.activeTransform != null) {
                inventory.activeTransform.position = Input.mousePosition;
            }
        }
    }

    public void SetInventory(Inventory inventory) {
        this.inventory = inventory;

        inventory.OnItemListChanged += Inventory_OnListItemChanged;
    }

    private void Inventory_OnListItemChanged(object sender, EventArgs e) {
        RefreshInventoryItems();
    }

    private void RefreshInventoryItems() {
        var itemList = inventory.GetItems();

        if (!worldManager.creativeMode) {
            voxelEditor.fillTypeNames.Clear();
            voxelEditor.fillTypeNames.Add(BlockType.Empty.ToString());
            foreach (var item in itemList) {
                if (item != null) {
                    if (!voxelEditor.fillTypeNames.Contains(item.itemType.ToString())) {
                        voxelEditor.fillTypeNames.Add(item.itemType.ToString());
                    }
                }
            }
        }

        inventory.slotList.Clear();
        foreach (Transform child in slotContainer) {
            Destroy(child.gameObject);
        }

        for (var i = 0; i < inventory.size; i++) {
            var item = itemList[i];
            var itemSlot = Instantiate(itemSlotPrefab, Vector3.zero, Quaternion.identity, slotContainer);
            inventory.slotList.Add(itemSlot.GetComponent<ItemSlot>());
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

    public void ToggleInventory() {
        isActive = !isActive;

        if (!isActive && inventory.activeTransform != null) {
            WorldItem.DropItem(player.transform.position, inventory.activeItem, player.facingRight);
            Destroy(inventory.activeTransform.gameObject);
            inventory.activeItem = null;
        }

        SetInventoryState(isActive);
    }

    private void SetInventoryState(bool state) {
        inventoryController.SetActive(state);
    }

    private void DisplayInfo() {
        if (!itemInfo) {
            itemInfo = Instantiate(itemInfoPrefab, Input.mousePosition + offset, Quaternion.identity, transform);
            nameText = itemInfo.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
            amountText = itemInfo.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
        } else {
            itemInfo.position = Input.mousePosition + offset;
            nameText.text = "Name: " + inventory.hoverSlot.item.itemType;
            amountText.text = "Amount: " + inventory.hoverSlot.item.amount + "/" + Item.maxAmount;
        }
    }



    public void InventoryClick() {
        if (inventory.activeItem == null) {
            if (inventory.hoverSlot != null && inventory.hoverSlot.item != null) {
                inventory.Pickup(inventory.hoverSlot.item.amount);
                CreateActiveItem();
            }
        } else {
            inventory.Drag();
        }
    }

    public void Split() {
        int amount;

        if (inventory.hoverSlot.item.amount > 1 && inventory.hoverSlot.item.IsStackable()) {
            amount = inventory.hoverSlot.item.amount / 2;
        } else {
            amount = 1;
        }

        inventory.Pickup(amount);
        CreateActiveItem();
    }

    private void CreateActiveItem() {
        var tempTransform = Instantiate(itemPrefab, Input.mousePosition, Quaternion.identity, transform).GetComponent<RectTransform>();
        tempTransform.GetComponent<Image>().sprite = inventory.activeItem.GetSprite();
        tempTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = inventory.activeItem.amount.ToString();
        inventory.activeTransform = tempTransform;
    }

    public void Drop(int count) {
        inventory.Drop(count);
    }

    private void DropActive(int count) {
        var tempItem = new Item { itemType = inventory.activeItem.itemType, amount = count };
        inventory.Drop(tempItem);

        if (inventory.activeItem.amount == count) {
            Destroy(inventory.activeTransform.gameObject);
            inventory.activeItem = null;
        } else {
            inventory.activeItem.amount--;

            if (inventory.activeItem.amount <= 0) {
                Destroy(inventory.activeTransform.gameObject);
                inventory.activeItem = null;
            } else {
                inventory.activeTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = inventory.activeItem.amount.ToString();
            }
        }
    }

    public void RemoveFirstItem(ItemType type) {
        int index = inventory.IndexOfFirstLocationFound(type);
        if (index != -1) {
            inventory.RemoveItem(index, 1);
        }
    }
}
