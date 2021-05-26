using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PlayerInventory : UI_TogglableInventory {
    protected override void Start() {
        base.Start();
    }

    public override void InventorySpecificControls() {
        if (Input.GetKeyDown(KeyCode.E)) {
            if (!pauseMenu.activeState) {
                Toggle();
            }
        }
    }
}
