using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkObjectSpawner : MonoBehaviour {
    private TerrainNoise terrainNoise;
    private VoxelMap voxelMap;

    private void Awake() {
        terrainNoise = FindObjectOfType<TerrainNoise>();
        voxelMap = FindObjectOfType<VoxelMap>();
    }

    public void SpawnObject(VoxelChunk chunk) {
        int x = Random.Range(0, voxelMap.voxelResolution);
        int y = Random.Range(0, voxelMap.voxelResolution);

        int xChunkPos = (int)(x + (chunk.transform.position.x * voxelMap.voxelResolution));
        int yChunkPos = (int)(chunk.transform.position.y * voxelMap.voxelResolution);

        SpawnGrass(xChunkPos, yChunkPos, x, chunk);
    }

    private void SpawnGrass(int xChunkPos, int yChunkPos, int localX, VoxelChunk chunk) {
        var noiseHeight = terrainNoise.Noise1D(xChunkPos);
        var localY = Mathf.Floor(Mathf.Abs(noiseHeight - yChunkPos));

        if (TerrainNoise.InRange(yChunkPos, noiseHeight, 1)) {
            for (int x = -1; x <= 1; x++) {
                int maxY = x == 0 ? 13 : 12;
                for (int y = -1; y < maxY; y++) {
                    SetPositionVoxel(localX + x, localY + y, chunk, 6);
                }
            }
        }
    }

    private void SetPositionVoxel(float x, float y, VoxelChunk chunk, int state) {
        int voxelIndex = chunk.VoxelIndexFromLocalPos(x, y);
        //TODO: handle voxels outside of this chunk
        if (voxelIndex >= 0 && voxelIndex < chunk.voxels.Length) {
            Voxel voxel = chunk.voxels[voxelIndex];
            voxel.state = state;
        }
    }
}
