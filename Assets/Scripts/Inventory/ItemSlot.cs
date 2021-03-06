using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour {
    [HideInInspector] public int index;

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
        localMousePosition = rectTransform.InverseTransformPoint(Input.mousePosition);

        if (rectTransform.rect.Contains(localMousePosition)) {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Q)) InventoryActions.Drop(ui, Item.MaxAmount);
            else if (Input.GetKeyDown(KeyCode.Q)) InventoryActions.Drop(ui, 1);
            if (Input.GetMouseButtonDown(1)) InventoryActions.Split(ui, inventoriesController);
        }
    }

    public void SetSelectedColor() {
        var background = transform.GetChild(0).GetComponent<Image>();
        background.color = selectedColor;
    }
}
