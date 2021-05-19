using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour {
    [HideInInspector] public int index = 0;

    public RectTransform rectTransform;
    [HideInInspector] public UI_Inventory ui;
    private RectTransform uIRectTransform;
    [HideInInspector] public Item item;
    private Vector2 localMousePosition;

    private InventoriesController inventoriesController;

    public Color selectedColor;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
        inventoriesController = FindObjectOfType<InventoriesController>();
    }

    public void StartUp(UI_Inventory uI_Inventory, Item item) {
        ui = uI_Inventory;
        this.item = item;

        uIRectTransform = uI_Inventory.GetComponent<RectTransform>();
    }

    private void Update() {
        localMousePosition = uIRectTransform.InverseTransformPoint(Input.mousePosition);

        if (uIRectTransform.rect.Contains(localMousePosition)) {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Q)) InventoryActions.Drop(ui, Item.MAXAmount);
            else if (Input.GetKeyDown(KeyCode.Q)) InventoryActions.Drop(ui, 1);
            if (Input.GetMouseButtonUp(1)) InventoryActions.Split(ui, inventoriesController);
        }
    }

    public void SetSelectedColor() {
        var background = transform.GetChild(0).GetComponent<Image>();
        background.color = selectedColor;
    }
}
