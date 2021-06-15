using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class VoxelEditor : MonoBehaviour {
    private const int UPDATE_INTERVAL = 2;

    [HideInInspector] public List<string> fillTypeNames = new List<string>();
    private static readonly string[] RadiusNames = { "0", "1", "2", "3", "4", "5" };
    private static readonly string[] StencilNames = { "Square", "Circle" };

    [HideInInspector] public int fillTypeIndex;
    private int radiusIndex, stencilIndex;

    private VoxelMesh voxelMesh;
    private ChunkCollider chunkCollider;
    private VoxelMap voxelMap;
    private BoxCollider boxCollider;
    private Camera mainCamera;
    private TerrainMap terrainMap;
    private WorldManager worldManager;
    private UI_HotBar uiHotBar;

    private Vector3 lastHitPoint;
    private int lastTypeIndex;
    private VoxelStencil activeStencil;
    private readonly List<Vector2Int> updateChunkPositions = new List<Vector2Int>();

    public GameObject playerCanvas;

    private readonly VoxelStencil[] stencils = {
        new VoxelStencil(),
        new VoxelStencilCircle()
    };

    public void StartUp(VoxelMap map) {
        voxelMap = map;
        mainCamera = Camera.main;

        terrainMap = FindObjectOfType<TerrainMap>();
        voxelMesh = FindObjectOfType<VoxelMesh>();
        chunkCollider = FindObjectOfType<ChunkCollider>();
        worldManager = FindObjectOfType<WorldManager>();
        uiHotBar = FindObjectOfType<UI_HotBar>();

        var blockCollection = BlockManager.Read();
        if (worldManager.creativeMode) {
            foreach (var block in blockCollection.blocks) {
                fillTypeNames.Add(block.blockType.ToString());
            }
        }

        boxCollider = gameObject.GetComponent<BoxCollider>();
        if (boxCollider != null) {
            DestroyImmediate(boxCollider);
        }

        boxCollider = gameObject.AddComponent<BoxCollider>();
        boxCollider.size = new Vector3((voxelMap.chunkResolution - voxelMap.viewDistance) * voxelMap.voxelResolution,
            (voxelMap.chunkResolution - voxelMap.viewDistance) * voxelMap.voxelResolution);
    }

    private void Update() {
        if (Time.frameCount % UPDATE_INTERVAL != 0) return;

        if (!EventSystem.current.IsPointerOverGameObject() || EventSystem.current.currentSelectedGameObject == playerCanvas) {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1)) {
                if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out var hitInfo)) {
                    if (hitInfo.collider.gameObject == gameObject && (lastHitPoint != hitInfo.point || lastTypeIndex != fillTypeIndex)) {
                        EditVoxels(hitInfo.point, Input.GetMouseButton(0));
                        lastHitPoint = hitInfo.point;
                        lastTypeIndex = fillTypeIndex;
                        terrainMap.RecalculateMap();
                    }
                }
            }
        }
        if (fillTypeIndex >= fillTypeNames.Count) {
            fillTypeIndex = fillTypeNames.Count - 1;
        }
    }

    private void EditVoxels(Vector3 point, bool isBreaking) {
        var chunkPos = new Vector3(Mathf.Floor(point.x / voxelMap.voxelResolution), Mathf.Floor(point.y / voxelMap.voxelResolution));
        var diff = new Vector2Int((int)Mathf.Abs(point.x - (chunkPos * voxelMap.voxelResolution).x),
            (int)Mathf.Abs(point.y - (chunkPos * voxelMap.voxelResolution).y));

        var xStart = (diff.x - radiusIndex - 1) / voxelMap.voxelResolution;
        if (xStart <= -voxelMap.chunkResolution) {
            xStart = -voxelMap.chunkResolution + 1;
        }

        var xEnd = (diff.x + radiusIndex) / voxelMap.voxelResolution;
        if (xEnd >= voxelMap.chunkResolution) {
            xEnd = voxelMap.chunkResolution - 1;
        }

        var yStart = (diff.y - radiusIndex - 1) / voxelMap.voxelResolution;
        if (yStart <= -voxelMap.chunkResolution) {
            yStart = -voxelMap.chunkResolution + 1;
        }

        var yEnd = (diff.y + radiusIndex) / voxelMap.voxelResolution;
        if (yEnd >= voxelMap.chunkResolution) {
            yEnd = voxelMap.chunkResolution - 1;
        }

        SetupStencil(isBreaking);

        var voxelYOffset = yEnd * voxelMap.voxelResolution;
        Vector2Int checkChunkPos;
        updateChunkPositions.Clear();

        var isEditing = false;

        if ((isBreaking && activeStencil.fillType == 0) || (!isBreaking && activeStencil.fillType != 0)) {
            for (var y = yStart - 1; y < yEnd + 1; y++) {
                var voxelXOffset = xEnd * voxelMap.voxelResolution;
                for (var x = xStart - 1; x < xEnd + 1; x++) {
                    activeStencil.SetCenter(diff.x - voxelXOffset, diff.y - voxelYOffset);

                    checkChunkPos = new Vector2Int((int)Mathf.Floor((point.x + voxelXOffset) / voxelMap.voxelResolution), (int)Mathf.Floor((point.y + voxelYOffset) / voxelMap.voxelResolution));

                    if (voxelMap.existingChunks.ContainsKey(checkChunkPos)) {
                        var currentChunk = voxelMap.existingChunks[checkChunkPos];
                        var tempRes = currentChunk.Apply(activeStencil);
                        if (!isEditing && tempRes) {
                            isEditing = true;
                        }
                    }
                    voxelXOffset -= voxelMap.voxelResolution;
                }
                voxelYOffset -= voxelMap.voxelResolution;
            }
        }

        if (isEditing) {
            checkChunkPos = new Vector2Int((int)Mathf.Floor((point.x) / voxelMap.voxelResolution), (int)Mathf.Floor((point.y) / voxelMap.voxelResolution));

            EditChunkAndNeighbors(checkChunkPos, new Vector3(Mathf.Floor(point.x), Mathf.Floor(point.y)));

            updateChunkPositions.Sort(SortByPosition);

            //TODO: update so it only checks the relevant one
            // foreach (var item in updateChunkPositions) {
            //     Debug.Log(item);
            // }

            foreach (var chunk in from pos in updateChunkPositions where voxelMap.existingChunks.ContainsKey(pos) select voxelMap.existingChunks[pos]) {
                voxelMesh.TriangulateChunkMesh(chunk);
                chunkCollider.Generate2DCollider(chunk, voxelMap.chunkResolution);
            }
        }
    }

    private void SetupStencil(bool isBreaking) {
        activeStencil = stencils[stencilIndex];

        if (worldManager.creativeMode) {
            activeStencil.Initialize(fillTypeIndex, radiusIndex);
        } else {
            var fillType = 0;
            var blocks = BlockManager.Read().blocks;

            for (int i = 0; i < blocks.Count; i++) {
                if (isBreaking) {
                    fillType = 0;
                    break;
                }
                if (uiHotBar.currentItem != null && blocks[i].blockType.ToString() == uiHotBar.currentItem.itemType.ToString()) {
                    fillType = i;
                    break;
                }
            }

            activeStencil.Initialize(fillType, radiusIndex);
        }
    }

    private static int SortByPosition(Vector2Int c1, Vector2Int c2) {
        return (c1.x < c2.x && c1.y < c2.y) ? 1 : 0;
    }

    private void EditChunkAndNeighbors(Vector2Int checkChunk, Vector2 pos) {
        for (var x = -1; x <= 1; x++) {
            for (var y = -1; y <= 1; y++) {
                var result = false;
                var currentChunkPos = new Vector2Int(checkChunk.x + x, checkChunk.y + y);

                for (var index = 0; index <= radiusIndex; index++) {
                    for (var index2 = 0; index2 <= radiusIndex; index2++) {
                        switch (x) {
                            case -1 when y == -1 && (Mathf.Abs(pos.x - index) % 8 == 0) && (Mathf.Abs(pos.y - index2) % 8 == 0): //1
                            case 0 when y == -1 && (Mathf.Abs(pos.y - index) % 8 == 0): //2
                            case 1 when y == -1 && (Mathf.Abs(pos.x + 1 + index) % 8 == 0) && (Mathf.Abs(pos.y - index2) % 8 == 0): //3
                            case -1 when y == 0 && (Mathf.Abs(pos.x - index) % 8 == 0): //4
                            case 0 when y == 0: //5
                            case 1 when y == 0 && (Mathf.Abs(pos.x + 1 + index) % 8 == 0): //6
                            case -1 when y == 1 && (Mathf.Abs(pos.x - index) % 8 == 0) && (Mathf.Abs(pos.y + 1 + index2) % 8 == 0): //7
                            case 0 when y == 1 && (Mathf.Abs(pos.y + 1 + index) % 8 == 0): //8
                            case 1 when y == 1 && (Mathf.Abs(pos.x + 1 - index) % 8 == 0) && (Mathf.Abs(pos.y + 1 + index2) % 8 == 0): //9
                                result = true;
                                break;
                        }
                        if (result) break;
                    }
                    if (result) break;
                }

                if (result) {
                    if (!updateChunkPositions.Contains(currentChunkPos)) {
                        updateChunkPositions.Add(currentChunkPos);
                    }
                }
            }
        }
    }

    private void OnGUI() {
        if (worldManager.creativeMode) {
            GUILayout.BeginArea(new Rect(4f, Screen.height - 250f, 150f, 1000f));
            GUILayout.Label("Fill Type");
            fillTypeIndex = GUILayout.SelectionGrid(fillTypeIndex, fillTypeNames.ToArray(), 2);
            GUILayout.Label("Radius");
            radiusIndex = GUILayout.SelectionGrid(radiusIndex, RadiusNames, 6);
            GUILayout.Label("Stencil");
            stencilIndex = GUILayout.SelectionGrid(stencilIndex, StencilNames, 2);
            GUILayout.EndArea();
        }
    }
}