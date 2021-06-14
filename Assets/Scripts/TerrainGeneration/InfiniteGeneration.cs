using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InfiniteGeneration : MonoBehaviour {
    public VoxelChunk voxelChunkPrefab;

    private VoxelMap voxelMap;
    private VoxelMesh voxelMesh;
    private TerrainNoise terrainNoise;
    private TerrainMap terrainMap;
    private ChunkSaveLoadManager chunkSaveLoadManager;

    private Vector2 voxelPlayerPos, playerOffset;
    private Vector2Int playerCoord, newChunkCoord;
    private float sqrViewDist;
    public bool useVoxelReferences = false;
    public float colliderRadius = 2;
    public bool useColliders = true;
    private ChunkCollider chunkCollider;
    private ChunkObjectSpawner chunkObjectSpawner;
    private Rigidbody2D playerRb;

    public void StartUp(VoxelMap map) {
        voxelMesh = FindObjectOfType<VoxelMesh>();
        terrainNoise = FindObjectOfType<TerrainNoise>();
        terrainMap = FindObjectOfType<TerrainMap>();
        chunkCollider = FindObjectOfType<ChunkCollider>();
        chunkObjectSpawner = FindObjectOfType<ChunkObjectSpawner>();
        chunkSaveLoadManager = FindObjectOfType<ChunkSaveLoadManager>();
        playerRb = FindObjectOfType<PlayerController>().GetComponent<Rigidbody2D>();

        voxelMap = map;

        terrainNoise.seed = voxelMap.worldScriptableObject.seed;
        terrainNoise.StartUp(voxelMap.voxelResolution, voxelMap.chunkResolution);
        voxelMesh.StartUp(voxelMap.voxelResolution, voxelMap.chunkResolution, voxelMap.viewDistance, useColliders, colliderRadius);

        InvokeRepeating(nameof(UpdateMap), 0.0f, terrainMap.updateInterval);
    }

    private void UpdateMap() {
        if (playerRb.velocity.magnitude > 0) {
            terrainMap.RecalculateMap();
        }
    }

    public void UpdateAroundPlayer() {
        if (playerRb.velocity.magnitude > 0) {
            voxelPlayerPos = voxelMap.player.position / voxelMap.voxelResolution;
            playerCoord = new Vector2Int(Mathf.RoundToInt(voxelPlayerPos.x), Mathf.RoundToInt(voxelPlayerPos.y));

            voxelMesh.CreateBuffers();

            RemoveOutOfBoundsChunks();

            CreateNewChunksInRange();

            UpdateNewChunks();

            RecreateUpdatedChunkMeshes();
        }
    }

    private void RemoveOutOfBoundsChunks() {
        sqrViewDist = voxelMap.viewDistance * voxelMap.viewDistance;

        for (var i = voxelMap.currentChunks.Count - 1; i >= 0; i--) {
            var testChunk = voxelMap.currentChunks[i];
            if (!ReferenceEquals(testChunk, null)) {
                var position = testChunk.transform.position;
                var testChunkPos = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
                playerOffset = playerCoord - testChunkPos;
                var offset = new Vector2(Mathf.Abs(playerOffset.x), Mathf.Abs(playerOffset.y)) -
                         (Vector2.one * voxelMap.viewDistance) / 2;
                var sqrDst = new Vector2(Mathf.Max(offset.x, 0), Mathf.Max(offset.y, 0)).sqrMagnitude;

                if (sqrDst > sqrViewDist) {
                    chunkSaveLoadManager.SaveChunk(testChunk.transform.position, testChunk);
                    voxelMap.existingChunks.Remove(testChunkPos);
                    voxelMap.recycleableChunks.Enqueue(testChunk);
                    voxelMap.currentChunks.RemoveAt(i);
                }
            }
        }

        chunkSaveLoadManager.CheckForEmptyRegions();
    }

    private void CreateNewChunksInRange() {
        for (var y = -voxelMap.chunkResolution / 2; y < voxelMap.chunkResolution / 2; y++) {
            for (var x = -voxelMap.chunkResolution / 2; x < voxelMap.chunkResolution / 2; x++) {
                newChunkCoord = new Vector2Int(x, y) + playerCoord;

                if (voxelMap.existingChunks.ContainsKey(newChunkCoord)) continue;
                playerOffset = voxelPlayerPos - newChunkCoord;
                var offset = new Vector2(Mathf.Abs(playerOffset.x), Mathf.Abs(playerOffset.y)) -
                         (Vector2.one * voxelMap.viewDistance) / 2;
                var sqrDst = offset.sqrMagnitude;

                if (sqrDst <= sqrViewDist - 4) {
                    VoxelChunk currentChunk;
                    if (voxelMap.recycleableChunks.Count > 0) {
                        currentChunk = voxelMap.recycleableChunks.Dequeue();
                        currentChunk.StartUp();
                    } else {
                        currentChunk = CreateChunk();
                    }

                    currentChunk.SetNewChunk(newChunkCoord);
                    var resultChunk = chunkSaveLoadManager.LoadChunk(newChunkCoord, currentChunk);
                    if (!ReferenceEquals(resultChunk, null)) {
                        currentChunk = resultChunk;
                        currentChunk.SetNewChunk(newChunkCoord);
                    } else {
                        terrainNoise.GenerateNoiseValues(currentChunk);
                        chunkObjectSpawner.SpawnObject(currentChunk);
                    }

                    currentChunk.transform.parent = chunkSaveLoadManager.GetRegionTransformForChunk(newChunkCoord);

                    voxelMap.existingChunks.Add(newChunkCoord, currentChunk);
                    currentChunk.shouldUpdateCollider = true;
                    voxelMap.currentChunks.Add(currentChunk);
                }
            }
        }
    }

    private VoxelChunk CreateChunk() {
        var chunk = Instantiate(voxelChunkPrefab, null, true);
        chunk.Initialize(useVoxelReferences, voxelMap.voxelResolution);
        chunk.gameObject.layer = 3;

        return chunk;
    }

    private void UpdateNewChunks() {
        foreach (var chunk in voxelMap.currentChunks.Where(chunk => chunk.shouldUpdateMesh)) {
            var position = chunk.transform.position;
            newChunkCoord = new Vector2Int(Mathf.RoundToInt(position.x),
                Mathf.RoundToInt(position.y));
            SetupChunkNeighbors(newChunkCoord, chunk);
        }
    }

    private void SetupChunkNeighbors(Vector2Int setupCoord, VoxelChunk chunk) {
        var axcoord = new Vector2Int(setupCoord.x - 1, setupCoord.y);
        var aycoord = new Vector2Int(setupCoord.x, setupCoord.y - 1);
        var axycoord = new Vector2Int(setupCoord.x - 1, setupCoord.y - 1);
        var bxcoord = new Vector2Int(setupCoord.x + 1, setupCoord.y);
        var bycoord = new Vector2Int(setupCoord.x, setupCoord.y + 1);
        var bxycoord = new Vector2Int(setupCoord.x + 1, setupCoord.y + 1);
        VoxelChunk tempChunk;

        if (!voxelMap.existingChunks.ContainsKey(setupCoord)) return;
        if (voxelMap.existingChunks.ContainsKey(axcoord)) {
            tempChunk = voxelMap.existingChunks[axcoord];
            tempChunk.shouldUpdateMesh = true;
            tempChunk.xNeighbor = chunk;
        }

        if (voxelMap.existingChunks.ContainsKey(aycoord)) {
            tempChunk = voxelMap.existingChunks[aycoord];
            tempChunk.shouldUpdateMesh = true;
            tempChunk.yNeighbor = chunk;
        }

        if (voxelMap.existingChunks.ContainsKey(axycoord)) {
            tempChunk = voxelMap.existingChunks[axycoord];
            tempChunk.shouldUpdateMesh = true;
            tempChunk.xyNeighbor = chunk;
        }

        if (voxelMap.existingChunks.ContainsKey(bxcoord)) {
            tempChunk = voxelMap.existingChunks[bxcoord];
            chunk.xNeighbor = tempChunk;
        }

        if (voxelMap.existingChunks.ContainsKey(bycoord)) {
            tempChunk = voxelMap.existingChunks[bycoord];
            chunk.yNeighbor = tempChunk;
        }

        if (voxelMap.existingChunks.ContainsKey(bxycoord)) {
            tempChunk = voxelMap.existingChunks[bxycoord];
            chunk.xyNeighbor = tempChunk;
        }
    }

    private void RecreateUpdatedChunkMeshes() {
        foreach (var chunk in voxelMap.currentChunks) {
            if (chunk.shouldUpdateMesh) {
                voxelMesh.TriangulateChunkMesh(chunk);
                chunk.shouldUpdateMesh = false;
            }

            if (!useColliders || !chunk.shouldUpdateCollider) continue;
            if (Vector3.Distance(voxelPlayerPos, chunk.transform.position) < colliderRadius) {
                chunkCollider.Generate2DCollider(chunk, voxelMap.chunkResolution);
                chunk.shouldUpdateCollider = false;
            }
        }
    }
}