using UnityEngine;

public class PlayerInventoryController : MonoBehaviour {
    private PlayerController playerController;
    private UI_HotBar uihotBar;
    [HideInInspector] public UI_PlayerInventory uimainInventory;

    [HideInInspector] public Inventory hotBarInventory, mainInventory;

    public int hotBarSize = 8;
    public int inventorySize = 32;

    private void Awake() {
        playerController = GetComponent<PlayerController>();
        uihotBar = FindObjectOfType<UI_HotBar>();
        uimainInventory = FindObjectOfType<UI_PlayerInventory>();

        hotBarInventory = new Inventory(hotBarSize, playerController);
        mainInventory = new Inventory(inventorySize, playerController);

        uihotBar.SetupInventory(hotBarInventory);
        uimainInventory.SetupInventory(mainInventory);
    }

    public void RemoveFirstItem(ItemType type) {
        int index = mainInventory.IndexOfFirstLocationFound(type);
        if (index != -1) {
            mainInventory.RemoveItem(index, 1);
        }
    }
}