using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour {
    [HideInInspector] public int index = 0;
    public Transform itemPrefab;

    private RectTransform rectTransform;
    private UI_Inventory uI_Inventory;
    private PlayerController player;
    private Inventory inventory;
    private Item item;

    void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }

    public void StartUp(UI_Inventory uI_Inventory, PlayerController player, Inventory inventory, Item item) {
        this.uI_Inventory = uI_Inventory;
        this.player = player;
        this.inventory = inventory;
        this.item = item;
    }

    private void Update() {
        if (IsHovering()) Hover();
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Q)) DropStack();
        if (Input.GetKeyDown(KeyCode.Q)) Drop();
        if (Input.GetMouseButtonDown(0)) Pickup();
        if (Input.GetMouseButtonDown(1)) Split();
    }

    private bool IsHovering() {
        Vector2 localMousePosition = rectTransform.InverseTransformPoint(Input.mousePosition);
        if (rectTransform.rect.Contains(localMousePosition)) {
            return true;
        }

        return false;
    }

    private void Hover() {
        uI_Inventory.hoverItem = item;
        uI_Inventory.hoverIndex = index;
    }

    private void Pickup() {
        //pickup
        if (uI_Inventory.hoverItem != null && (uI_Inventory.activeItem == null || uI_Inventory.activeItem.itemType == ItemType.Empty)) {
            PickupLogic(uI_Inventory.hoverItem.amount);
        }
        //put down
        else if (uI_Inventory.activeItem != null) {
            //place
            if (uI_Inventory.hoverItem == null) {
                inventory.AddItemIndex(uI_Inventory.activeItem, uI_Inventory.hoverIndex);
                Destroy(uI_Inventory.activeTransform.gameObject);
                uI_Inventory.activeItem = null;
            }
            //switch
            else {
                //grab current inventory item
                Item tempItem = inventory.GetItems()[uI_Inventory.hoverIndex];
                //set inventory to new item
                inventory.AddItemIndex(uI_Inventory.activeItem, uI_Inventory.hoverIndex);
                //set held item to grabbed
                uI_Inventory.activeItem = new Item { itemType = tempItem.itemType, amount = tempItem.amount };
                uI_Inventory.activeTransform.GetComponent<Image>().sprite = uI_Inventory.activeItem.GetSprite();
                uI_Inventory.activeTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = uI_Inventory.activeItem.amount.ToString();
            }
        }
    }

    private void Split() {
        if (uI_Inventory.hoverItem != null && uI_Inventory.activeItem == null) {
            PickupLogic(uI_Inventory.hoverItem.amount / 2);
        }
    }

    private void PickupLogic(int count) {
        uI_Inventory.activeItem = new Item { itemType = uI_Inventory.hoverItem.itemType, amount = count };

        var tempTransform = Instantiate(itemPrefab, Input.mousePosition, Quaternion.identity, uI_Inventory.transform).GetComponent<RectTransform>();
        tempTransform.GetComponent<Image>().sprite = uI_Inventory.activeItem.GetSprite();
        tempTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = uI_Inventory.activeItem.amount.ToString();
        uI_Inventory.activeTransform = tempTransform;

        inventory.RemoveItem(uI_Inventory.hoverItem, uI_Inventory.hoverIndex, count);
    }

    private void Drop() {
        if (uI_Inventory.hoverItem != null) {
            DropLogic(1);
        }
    }

    private void DropStack() {
        if (uI_Inventory.hoverItem != null) {
            DropLogic(uI_Inventory.hoverItem.amount);
        }
    }

    private void DropLogic(int count) {
        var tempItem = new Item { itemType = uI_Inventory.hoverItem.itemType, amount = count };
        inventory.RemoveItem(tempItem, uI_Inventory.hoverIndex, tempItem.amount);

        WorldItem.DropItem(player.transform.position, tempItem, player.facingRight);
    }
}
