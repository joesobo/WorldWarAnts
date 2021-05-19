using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryController : MonoBehaviour {
    private PlayerController playerController;
    private InventoriesController inventoriesController;

    private UI_HotBar uiHotBar;
    [HideInInspector] public UI_PlayerInventory uiMainInventory;
    private Inventory hotBarInventory, mainInventory;

    public int hotBarSize = 8;
    public int inventorySize = 32;

    private void Awake() {
        playerController = FindObjectOfType<PlayerController>();
        inventoriesController = FindObjectOfType<InventoriesController>();

        hotBarInventory = new Inventory(hotBarSize, playerController);
        mainInventory = new Inventory(inventorySize, playerController);

        uiHotBar = FindObjectOfType<UI_HotBar>();
        uiMainInventory = FindObjectOfType<UI_PlayerInventory>();

        uiHotBar.SetupInventory(hotBarInventory);
        uiMainInventory.SetupInventory(mainInventory);
    }

    private void Start() {
        inventoriesController.activeInventories.Add(hotBarInventory);
        inventoriesController.activeUIs.Add(uiHotBar);
        inventoriesController.activeUIs.Add(uiMainInventory);
    }

    public void RemoveFirstItem(ItemType type) {
        var index = hotBarInventory.IndexOfFirstLocationFound(type);
        if (index != -1) {
            hotBarInventory.RemoveItem(index, 1);
        }
    }

    public bool HasRoom(Item item) {
        return hotBarInventory.HasRoom(item) || mainInventory.HasRoom(item);
    }

    public void AddItem(Item item) {
        if (hotBarInventory.HasRoom(item)) {
            hotBarInventory.AddItem(item);
        } else {
            mainInventory.AddItem(item);
        }
    }
}
