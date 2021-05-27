using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour {
    public List<UI_Inventory> inventoryUIs;
    public List<GameObject> togglableUIs;

    private PauseMenu pauseMenu;

    private void Awake() {
        inventoryUIs = new List<UI_Inventory>();
        togglableUIs = new List<GameObject>();

        pauseMenu = FindObjectOfType<PauseMenu>();
    }

    public void AddUI(UI_Inventory uI) {
        if (uI is UI_TogglableInventory) {
            togglableUIs.Add(uI.gameObject);
        }

        inventoryUIs.Add(uI);
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            bool isActiveUI = false;
            foreach (GameObject ui in togglableUIs) {
                UI_TogglableInventory inventory = ui.GetComponent<UI_TogglableInventory>();
                UI_Crafting crafting = ui.GetComponent<UI_Crafting>();

                if (inventory != null && inventory.isActive) {
                    isActiveUI = true;
                    break;
                } else if (crafting != null && crafting.isActive) {
                    isActiveUI = true;
                    break;
                }
            }

            if (isActiveUI) {
                foreach (GameObject ui in togglableUIs) {
                    UI_TogglableInventory inventory = ui.GetComponent<UI_TogglableInventory>();
                    UI_Crafting crafting = ui.GetComponent<UI_Crafting>();

                    if (inventory != null && inventory.isActive) {
                        inventory.Toggle();
                    } else if (crafting != null && crafting.isActive) {
                        crafting.Toggle();
                    }
                }
            } else {
                pauseMenu.Toggle();
            }
        }
    }
}
