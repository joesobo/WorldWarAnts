using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour {
    public List<UI_Inventory> inventoryUIs;
    public List<UI_Inventory> togglableUIs;

    private PauseMenu pauseMenu;

    private void Awake() {
        inventoryUIs = new List<UI_Inventory>();
        togglableUIs = new List<UI_Inventory>();

        pauseMenu = FindObjectOfType<PauseMenu>();
    }

    public void AddUI(UI_Inventory uI) {
        if (uI is UI_TogglableInventory) {
            togglableUIs.Add(uI);
        }

        inventoryUIs.Add(uI);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            bool isActiveUI = false;
            foreach (UI_TogglableInventory togglableUI in togglableUIs) {
                if (togglableUI.isActive) {
                    isActiveUI = true;
                    break;
                }
            }

            if (isActiveUI) {
                foreach (UI_TogglableInventory togglableUI in togglableUIs) {
                if (togglableUI.isActive) {
                    togglableUI.Toggle();
                }
            }
            } else {
                pauseMenu.Toggle();
            }
        }
    }
}
