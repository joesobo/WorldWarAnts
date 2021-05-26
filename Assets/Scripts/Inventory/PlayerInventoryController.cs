using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryController : MonoBehaviour {
    private PlayerController playerController;
    private InventoriesController inventoriesController;
    private UIController uIController;

    private UI_HotBar uiHotBar;
    [HideInInspector] public UI_PlayerInventory uiMainInventory;
    private Inventory hotBarInventory, mainInventory;

    public int hotBarSize = 8;
    public int inventorySize = 32;

    private void Awake() {
        playerController = FindObjectOfType<PlayerController>();
        inventoriesController = FindObjectOfType<InventoriesController>();
        uIController = FindObjectOfType<UIController>();

        hotBarInventory = new Inventory(hotBarSize, playerController);
        mainInventory = new Inventory(inventorySize, playerController);


    }

    private void Start() {
        uiHotBar = FindObjectOfType<UI_HotBar>();
        uiMainInventory = FindObjectOfType<UI_PlayerInventory>();

        uiHotBar.SetupInventory(hotBarInventory);
        uiMainInventory.SetupInventory(mainInventory);

        inventoriesController.activeInventories.Add(hotBarInventory);

        uIController.AddUI(uiHotBar);
    }

    public bool HasRoom(Item item) {
        return hotBarInventory.HasRoom(item) || mainInventory.HasRoom(item);
    }

    public void ReplaceEmpty(int index, ItemType type) {
        int hotBarIndex = hotBarInventory.IndexOfFirstLocationFound(type);

        if (hotBarIndex == -1) {
            int mainIndex = mainInventory.IndexOfFirstLocationFound(type);

            if (mainIndex == -1) {
                return;
            }

            hotBarInventory.itemList[index] = mainInventory.itemList[mainIndex];
            mainInventory.itemList[mainIndex] = null;
            uiMainInventory.RefreshInventory();
            return;
        }

        hotBarInventory.itemList[index] = hotBarInventory.itemList[hotBarIndex];
        hotBarInventory.itemList[hotBarIndex] = null;
    }

    public void AddItem(Item item) {
        if (hotBarInventory.HasRoom(item)) {
            hotBarInventory.AddItem(item);
        } else {
            mainInventory.AddItem(item);
        }
    }
}
