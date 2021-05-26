using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_TogglableInventory : UI_Inventory {
    public bool isActive;
    protected PauseMenu pauseMenu;
    private UIController uIController;

    public GameObject inventoryController;

    protected virtual void Awake() {
        pauseMenu = FindObjectOfType<PauseMenu>();
        uIController = FindObjectOfType<UIController>();

        uIController.AddUI(this);

        inventoryController.SetActive(isActive);
    }

    public override void InventorySpecificControls() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (isActive) {
                Toggle();
            }
        }
    }

    public void Toggle() {
        isActive = !isActive;

        inventoryController.SetActive(isActive);

        if (isActive) {
            inventoriesController.activeInventories.Add(inventory);
        } else {
            if (inventoriesController.activeInventories.Contains(inventory)) {
                inventoriesController.activeInventories.Remove(inventory);
            }
        }
    }
}
