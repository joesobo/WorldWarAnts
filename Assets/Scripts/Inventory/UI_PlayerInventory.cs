using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PlayerInventory : UI_Inventory {
    private PauseMenu pauseMenu;
    [HideInInspector] public bool isActive = false;

    public GameObject inventoryController;

    private void Awake() {
        pauseMenu = FindObjectOfType<PauseMenu>();
        inventoryController.SetActive(isActive);
    }

    public override void InventorySpecificControls() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (isActive) {
                Toggle();
            } else {
                pauseMenu.Toggle();
            }
        }

        if (Input.GetKeyDown(KeyCode.E)) {
            if (!pauseMenu.activeState) {
                Toggle();
            }
        }
    }

    private void Toggle() {
        isActive = !isActive;

        inventoryController.SetActive(isActive);
    }
}
