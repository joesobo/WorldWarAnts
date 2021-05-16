using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;
using System.Collections.Generic;

public class UI_Inventory : MonoBehaviour {
    [HideInInspector] public Inventory inventory;
    private PlayerController player;
    private RectTransform rectTransform;
    private Vector2 localMousePosition;

    public GameObject inventoryController;
    public Transform slotContainer;
    public Transform itemSlotPrefab;
    [HideInInspector] public bool isActive = false;

    [HideInInspector] public ItemSlot hoverSlot;
    [HideInInspector] public Item hoverItem = null;
    [HideInInspector] public int hoverIndex = -1;
    [HideInInspector] public RectTransform activeTransform;
    [HideInInspector] public Item activeItem = null;

    private List<int> slotIndexList = new List<int>();

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
        player = FindObjectOfType<PlayerController>();
        SetInventoryState(isActive);
    }

    private void Update() {
        if (activeTransform && isActive) {
            activeTransform.position = Input.mousePosition;
            localMousePosition = rectTransform.InverseTransformPoint(Input.mousePosition);

            if (!rectTransform.rect.Contains(localMousePosition)) {
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
            } else {
                //clicking logic
                if (Input.GetMouseButtonDown(0)) {
                    slotIndexList.Clear();
                }
                if (Input.GetMouseButton(0)) {
                    if (!slotIndexList.Contains(hoverSlot.index) && (hoverItem == null || hoverItem.itemType == ItemType.Empty) && slotIndexList.Count < activeItem.amount) {
                        slotIndexList.Add(hoverSlot.index);
                    }
                }
                if (Input.GetMouseButtonUp(0)) {
                    //drag
                    if (slotIndexList.Count > 1) {
                        var itemsPerSlot = activeItem.amount / slotIndexList.Count;
                        var itemsHeld = activeItem.amount % slotIndexList.Count;

                        foreach (var index in slotIndexList) {
                            //set texture
                            inventory.AddItemIndex(new Item { itemType = activeItem.itemType, amount = itemsPerSlot }, index);
                        }

                        if (itemsHeld == 0) {
                            Destroy(activeTransform.gameObject);
                            activeItem = null;
                        } else {
                            activeItem.amount = itemsHeld;
                            activeTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = activeItem.amount.ToString();
                        }
                    }
                    //place
                    else {
                        hoverSlot.Place();
                    }
                }
            }
        }

        //sort
        if (Input.GetKey(KeyCode.R)) {
            inventory.SortInventory();
            RefreshInventoryItems();
        }
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

            slot.StartUp(this, player, inventory, item);

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
