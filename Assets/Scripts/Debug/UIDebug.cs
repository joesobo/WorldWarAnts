using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIDebug : MonoBehaviour {
    private Transform debugInfo;
    private TextMeshProUGUI playerPosText;
    private TextMeshProUGUI chunkPosText;
    private TextMeshProUGUI regionPosText;
    private TextMeshProUGUI mousePosText;
    private TextMeshProUGUI worldNameText;
    private TextMeshProUGUI worldSeedText;
    private TextMeshProUGUI gameModeText;

    private Transform player;
    private VoxelMap voxelMap;
    private ChunkSaveLoadManager chunkSaveLoadManager;
    private WorldManager worldManager;
    private TerrainNoise terrainNoise;
    private WorldDataHandler worldDataHandler;

    [HideInInspector] public bool isActive;

    private void Awake() {
        player = FindObjectOfType<PlayerController>().transform;
        voxelMap = FindObjectOfType<VoxelMap>();
        chunkSaveLoadManager = FindObjectOfType<ChunkSaveLoadManager>();
        worldManager = FindObjectOfType<WorldManager>();
        terrainNoise = FindObjectOfType<TerrainNoise>();
        worldDataHandler = FindObjectOfType<WorldDataHandler>();

        isActive = false;

        debugInfo = transform.GetChild(0);
        worldNameText = debugInfo.GetChild(0).GetComponent<TextMeshProUGUI>();
        worldSeedText = debugInfo.GetChild(1).GetComponent<TextMeshProUGUI>();
        gameModeText = debugInfo.GetChild(2).GetComponent<TextMeshProUGUI>();
        playerPosText = debugInfo.GetChild(3).GetComponent<TextMeshProUGUI>();
        chunkPosText = debugInfo.GetChild(4).GetComponent<TextMeshProUGUI>();
        regionPosText = debugInfo.GetChild(5).GetComponent<TextMeshProUGUI>();
        mousePosText = debugInfo.GetChild(6).GetComponent<TextMeshProUGUI>();

        SetInfoState(isActive);
    }

    private void Update() {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.I)) {
            Toggle();
        }

        if (isActive) {
            if (worldDataHandler != null) {
                var worldName = worldManager.worldName;
                worldNameText.text = "World Name: " + worldName;
            } else {
                worldNameText.text = "TESTING MODE: No World Saving";
            }

            worldSeedText.text = "Seed: " + terrainNoise.seed;
            var mode = worldManager.creativeMode ? "Creative" : "Survival";
            gameModeText.text = "Game Mode: " + mode;

            playerPosText.text = "Player Position: " + new Vector2(player.position.x, player.position.y);
            var chunkPos = voxelMap.ChunkPosFromWorldPos(player.position);
            chunkPosText.text = "Chunk Position: " + chunkPos;
            var regionPos = chunkSaveLoadManager.RegionPosFromChunkPos(chunkPos);
            regionPosText.text = "Region Position: " + regionPos;
            var worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosText.text = "Mouse Position: " + new Vector2(worldMousePos.x, worldMousePos.y);
        }
    }

    private void Toggle() {
        isActive = !isActive;
        SetInfoState(isActive);
    }

    private void SetInfoState(bool state) {
        debugInfo.gameObject.SetActive(state);
    }
}
