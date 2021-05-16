using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour {
    [HideInInspector] public int index = 0;
    public Transform itemPrefab;

    public RectTransform rectTransform;
    private UI_Inventory uI_Inventory;
    private RectTransform uIRectTransform;
    private PlayerController player;
    private Inventory inventory;
    [HideInInspector] public Item item;
    private Vector2 localMousePosition;

    public Color selectedColor;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }

    public void StartUp(UI_Inventory uI_Inventory, PlayerController player, Inventory inventory, Item item) {
        this.uI_Inventory = uI_Inventory;
        uIRectTransform = uI_Inventory.GetComponent<RectTransform>();
        this.player = player;
        this.inventory = inventory;
        this.item = item;
    }

    private void Update() {
        localMousePosition = uIRectTransform.InverseTransformPoint(Input.mousePosition);

        if (uIRectTransform.rect.Contains(localMousePosition)) {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Q)) DropStack();
            if (Input.GetKeyDown(KeyCode.Q)) Drop();
            if (Input.GetMouseButtonUp(0)) Pickup();
            if (Input.GetMouseButtonUp(1)) Split();
        }
    }

    private void Pickup() {
        //pickup
        if (uI_Inventory.isHovering) {
            if (uI_Inventory.hoverSlot.item != null && (uI_Inventory.activeItem == null || uI_Inventory.activeItem.itemType == ItemType.Empty)) {
                PickupLogic(uI_Inventory.hoverSlot.item.amount);
            }
        }
    }

    public void Place() {
        //put down
        if (uI_Inventory.activeItem != null && uI_Inventory.activeItem.itemType != ItemType.Empty) {
            //place
            if (uI_Inventory.hoverSlot.item == null) {
                inventory.AddItemIndex(uI_Inventory.activeItem, uI_Inventory.hoverSlot.index);
                Destroy(uI_Inventory.activeTransform.gameObject);
                uI_Inventory.activeItem = null;
            }
            //switch
            else {
                //grab current inventory item
                var tempItem = uI_Inventory.hoverSlot.item;

                if (tempItem.itemType == uI_Inventory.activeItem.itemType) {
                    var totalAmount = tempItem.amount + uI_Inventory.activeItem.amount;

                    //add stacks together
                    if (totalAmount <= Item.maxAmount) {
                        inventory.AddItemIndex(new Item { itemType = tempItem.itemType, amount = totalAmount }, uI_Inventory.hoverSlot.index);
                        Destroy(uI_Inventory.activeTransform.gameObject);
                        uI_Inventory.activeItem = null;
                    } else {
                        inventory.AddItemIndex(new Item { itemType = tempItem.itemType, amount = Item.maxAmount }, uI_Inventory.hoverSlot.index);
                        uI_Inventory.activeItem = new Item { itemType = tempItem.itemType, amount = totalAmount - Item.maxAmount };
                        uI_Inventory.activeTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = uI_Inventory.activeItem.amount.ToString();
                    }
                } else {
                    //set inventory to new item
                    inventory.AddItemIndex(uI_Inventory.activeItem, uI_Inventory.hoverSlot.index);
                    //set held item to grabbed
                    uI_Inventory.activeItem = new Item { itemType = tempItem.itemType, amount = tempItem.amount };
                    uI_Inventory.activeTransform.GetComponent<Image>().sprite = uI_Inventory.activeItem.GetSprite();
                    uI_Inventory.activeTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = uI_Inventory.activeItem.amount.ToString();
                }
            }
        }
    }

    private void Split() {
        if (uI_Inventory.hoverSlot.item != null && (uI_Inventory.activeItem == null || uI_Inventory.activeItem.itemType == ItemType.Empty)) {
            PickupLogic(uI_Inventory.hoverSlot.item.amount / 2);
        }
    }

    private void PickupLogic(int count) {
        uI_Inventory.activeItem = new Item { itemType = uI_Inventory.hoverSlot.item.itemType, amount = count };

        var tempTransform = Instantiate(itemPrefab, Input.mousePosition, Quaternion.identity, uI_Inventory.transform).GetComponent<RectTransform>();
        tempTransform.GetComponent<Image>().sprite = uI_Inventory.activeItem.GetSprite();
        tempTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = uI_Inventory.activeItem.amount.ToString();
        uI_Inventory.activeTransform = tempTransform;

        inventory.RemoveItem(uI_Inventory.hoverSlot.item, uI_Inventory.hoverSlot.index, count);
    }

    private void Drop() {
        if (uI_Inventory.hoverSlot.item != null) {
            DropLogic(1);
        }
    }

    private void DropStack() {
        if (uI_Inventory.hoverSlot.item != null) {
            DropLogic(uI_Inventory.hoverSlot.item.amount);
        }
    }

    private void DropLogic(int count) {
        var tempItem = new Item { itemType = uI_Inventory.hoverSlot.item.itemType, amount = count };
        inventory.RemoveItem(tempItem, uI_Inventory.hoverSlot.index, tempItem.amount);

        WorldItem.DropItem(player.transform.position, item, player.facingRight);
    }

    public void SetSelectedColor() {
        var background = transform.GetChild(0).GetComponent<Image>();
        background.color = selectedColor;
    }
}
