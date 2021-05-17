using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour {
    [HideInInspector] public int index = 0;

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
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Q)) uI_Inventory.Drop(Item.maxAmount);
            else if (Input.GetKeyDown(KeyCode.Q)) uI_Inventory.Drop(1);
            if (Input.GetMouseButtonUp(0)) uI_Inventory.InventoryClick();
            if (Input.GetMouseButtonUp(1)) uI_Inventory.Split();
        }
    }

    public void SetSelectedColor() {
        var background = transform.GetChild(0).GetComponent<Image>();
        background.color = selectedColor;
    }
}
