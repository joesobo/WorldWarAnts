using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonController : MonoBehaviour {
    private PauseMenu pauseMenu;
    private UI_Inventory uI_Inventory;

    void Awake() {
        pauseMenu = FindObjectOfType<PauseMenu>();
        uI_Inventory = FindObjectOfType<UI_Inventory>();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (uI_Inventory.isActive) {
                uI_Inventory.ToggleInventory();
            } else {
                pauseMenu.Toggle();
            }
        }

        if (Input.GetKeyDown(KeyCode.E)) {
            if (!pauseMenu.activeState) {
                uI_Inventory.ToggleInventory();
            }
        }
    }
}
