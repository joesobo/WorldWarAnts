using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoriesController : MonoBehaviour {
    public Transform itemPrefab;
    public Transform itemInfoPrefab;

    [HideInInspector] public List<Inventory> activeInventories;
    [HideInInspector] public List<ItemSlot> slotList;

    private Vector2 localMousePosition;
    [HideInInspector] public Item activeItem = null;
    [HideInInspector] public Transform activeTransform;
    private Transform itemInfo;
    private TextMeshProUGUI nameText;
    private TextMeshProUGUI amountText;
    private PlayerController player;
    private UIController uIController;

    private void Awake() {
        activeInventories = new List<Inventory>();
        slotList = new List<ItemSlot>();

        player = FindObjectOfType<PlayerController>();
        uIController = FindObjectOfType<UIController>();
    }

    private void Update() {
        for (var i = 0; i < uIController.inventoryUIs.Count; i++) {
            var inventory = i < activeInventories.Count ? activeInventories[i] : null;
            var ui = uIController.inventoryUIs[i];
            var uiRectTransform = ui.GetComponent<RectTransform>();
            localMousePosition = uiRectTransform.InverseTransformPoint(Input.mousePosition);

            InventoryActions.Hover(ui);

            if (uiRectTransform.rect.Contains(localMousePosition)) {
                //click logic
                if (Input.GetMouseButtonDown(0)) {
                    slotList.Clear();
                }
                if (Input.GetMouseButton(0)) {
                    InventoryActions.SelectSlot(ui, this);
                }
                if (Input.GetMouseButtonUp(0)) {
                    InventoryActions.InventoryClick(ui, this);
                }

                //hover info logic
                if (Input.GetKey(KeyCode.LeftControl) && ui.hoverSlot && ui.hoverSlot.item != null) {
                    if (!itemInfo) {
                        CreateDisplay();
                    } else {
                        InventoryActions.DisplayInfo(ui, itemInfo, nameText, amountText);
                    }
                } else {
                    if (itemInfo) {
                        Destroy(itemInfo.gameObject);
                    }
                }

                //sort
                if (Input.GetKeyDown(KeyCode.R)) {
                    inventory.SortItems();
                }
            }

            if (activeTransform != null) {
                activeTransform.position = Input.mousePosition;
            }

            ui.InventorySpecificControls();
        }

        if (activeItem != null && activeItem.itemType != ItemType.Empty && !MouseOverUI()) {
            if (Input.GetMouseButtonDown(0)) {
                InventoryActions.DropActive(player, this, activeItem.amount);
            }
            if (Input.GetMouseButtonDown(1)) {
                InventoryActions.DropActive(player, this, 1);
            }
        }
    }

    public void CreateActiveItem() {
        var tempTransform = Instantiate(itemPrefab, Input.mousePosition, Quaternion.identity, transform).GetComponent<RectTransform>();
        tempTransform.GetComponent<Image>().sprite = activeItem.GetSprite();
        tempTransform.Find("Amount").GetComponent<TextMeshProUGUI>().text = activeItem.amount.ToString();
        activeTransform = tempTransform;
    }

    private void CreateDisplay() {
        itemInfo = Instantiate(itemInfoPrefab, Input.mousePosition + InventoryActions.offset, Quaternion.identity, transform);
        nameText = itemInfo.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        amountText = itemInfo.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    public void DestroyActive() {
        Destroy(activeTransform.gameObject);
        activeItem = null;
    }

    private bool MouseOverUI() {
        return uIController.inventoryUIs.Select(ui => ui.GetComponent<RectTransform>()).Any(rectTransform => rectTransform.rect.Contains(rectTransform.InverseTransformPoint(Input.mousePosition)));
    }
}