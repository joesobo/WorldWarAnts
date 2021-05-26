using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Crafting : UI_TogglableInventory {
    private PlayerController playerController;

    protected override void Awake() {
        playerController = FindObjectOfType<PlayerController>();

        this.inventory = new Inventory(1, playerController);
        SetupInventory(inventory);

        base.Awake();
    }

    public override void InventorySpecificControls() {
        if (Input.GetKeyDown(KeyCode.C)) {
            if (!pauseMenu.activeState) {
                Toggle();
            }
        }
    }
}
