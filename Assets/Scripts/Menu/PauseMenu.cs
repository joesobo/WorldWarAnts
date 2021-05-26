using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {
    [HideInInspector]
    public bool activeState = false;
    private bool needsUpdating = true;

    private ChunkSaveLoadManager chunkSaveLoadManager;
    private WorldDataHandler worldDataHandler;
    private UI_Inventory inventory;
    private UIController uIController;

    private void Awake() {
        chunkSaveLoadManager = FindObjectOfType<ChunkSaveLoadManager>();
        worldDataHandler = FindObjectOfType<WorldDataHandler>();
        inventory = FindObjectOfType<UI_Inventory>();
        uIController = FindObjectOfType<UIController>();
    }

    private void Update() {
        if (needsUpdating) {
            foreach (Transform child in transform) {
                child.gameObject.SetActive(activeState);
            }

            if (activeState) {
                Time.timeScale = 0;
            } else {
                Time.timeScale = 1;
            }
        }

        needsUpdating = false;
    }

    public void Toggle() {
        activeState = !activeState;
        needsUpdating = true;
    }

    public void Deactivate() {
        activeState = false;
        needsUpdating = true;
    }

    public void OpenControls() {

    }

    public void OpenOptions() {

    }

    public void SaveAndQuit() {
        chunkSaveLoadManager.SaveAllChunks();
        worldDataHandler.UpdateWorld();

        SceneManager.LoadScene(0);
    }
}
