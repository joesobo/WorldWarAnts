using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PlayerInventory : UI_TogglableInventory {
    protected override void Awake() {
        base.Awake();
    }

    public override void InventorySpecificControls() {
        if (Input.GetKeyDown(KeyCode.E)) {
            if (!pauseMenu.activeState) {
                Toggle();
            }
        }
    }
}
