using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class TerrainNoise : MonoBehaviour {
    [PropertySpace(SpaceAfter = 10)]
    public TerrainType terrainType = TerrainType.Perlin;

    [ShowIfGroup("terrainType", TerrainType.Perlin), BoxGroup("terrainType/Perlin"), DisableIf("useRandomSeed")]
    public int seed = 0;
    [BoxGroup("terrainType/Perlin")]
    public bool useRandomSeed;
    [BoxGroup("terrainType/Perlin")]
    public float height1, height2, height3, height4 = 0;

    [ShowIfGroup("terrainType", TerrainType.Perlin), BoxGroup("terrainType/Height Noise"), Range(0.1f, 1)]
    public float frequency = 1f;
    [BoxGroup("terrainType/Height Noise"), Range(1, 8)]
    public int octaves = 1;
    [BoxGroup("terrainType/Height Noise"), Range(1f, 4f)]
    public float lacunarity = 2f;
    [BoxGroup("terrainType/Height Noise"), Range(0f, 1f)]
    public float persistence = 0.5f;
    [BoxGroup("terrainType/Height Noise"), Range(0, 2)]
    public float amplitude = 1f;
    [BoxGroup("terrainType/Height Noise"), Range(0, 2)]
    public float range = 1f;

    [Space, ShowIfGroup("terrainType", TerrainType.Perlin), BoxGroup("terrainType/Cave Noise"), Range(0.1f, 1)]
    public float caveFrequency = 1f;
    [BoxGroup("terrainType/Cave Noise"), Range(1, 8)]
    public int caveOctaves = 1;
    [BoxGroup("terrainType/Cave Noise"), Range(0f, 2f)]
    public float caveLacunarity = 2f;
    [BoxGroup("terrainType/Cave Noise"), Range(0f, 2f)]
    public float cavePersistence = 0.5f;
    [BoxGroup("terrainType/Cave Noise"), Range(0, 2)]
    public float caveAmplitude = 1f;
    [BoxGroup("terrainType/Cave Noise"), Range(0.0001f, 2)]
    public float scale;
    [BoxGroup("terrainType/Cave Noise")]
    public Vector2 offset = Vector2.zero;

    private int voxelResolution, chunkResolution;
    private float maxNoiseVal, minNoiseVal;

    private Vector2[] octaveOffsets;

    public enum TerrainType {
        Off,
        On,
        Random,
        RandomFull,
        Perlin,
    }

    public void StartUp(int voxelResolution, int chunkResolution) {
        this.voxelResolution = voxelResolution;
        this.chunkResolution = chunkResolution;

        if (useRandomSeed) {
            seed = (int)Random.Range(0f, 10000f);
        }
        Random.InitState(seed);

        octaveOffsets = new Vector2[caveOctaves];
        for (var i = 0; i < caveOctaves; i++) {
            var offsetX = Random.Range(-100000, 100000) + offset.x;
            var offsetY = Random.Range(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        maxNoiseVal = float.MinValue;
        minNoiseVal = float.MaxValue;
    }

    public void GenerateNoiseValues(VoxelChunk chunk) {
        var position = chunk.transform.position;
        var chunkX = position.x;
        var chunkY = position.y;

        foreach (var voxel in chunk.voxels) {
            voxel.state = terrainType switch {
                TerrainType.Off => GetBlockTypeIndex(BlockType.Empty),
                TerrainType.On => GetBlockTypeIndex(BlockType.Stone),
                TerrainType.Random => Random.Range(0, 5),
                TerrainType.RandomFull => Random.Range(1, 5),
                TerrainType.Perlin => PerlinNoise(chunkX, chunkY, voxel),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    private int PerlinNoise(float chunkX, float chunkY, Voxel voxel) {
        var x = Mathf.RoundToInt(voxel.position.x * (voxelResolution - 1) + chunkX * voxelResolution);
        var y = Mathf.RoundToInt(voxel.position.y * (voxelResolution - 1) + chunkY * voxelResolution);

        return PerlinCalculate(x, y);
    }

    public int PerlinCalculate(int x, int y) {
        float voxelState = Perlin1D(x, y);
        if (voxelState == 0) return (int)voxelState;
        if (PerlinGrass(x, y) != -1) {
            return PerlinGrass(x, y);
        }
        voxelState = Perlin2D(x, y);
        return PerlinCaves(y, voxelState);
    }

    public int PerlinCaves(int y, float voxelState) {
        if (y < height1) {
            return GetBlockTypeIndex(PercentChangeBlocks(voxelState, 33, BlockType.Rock, BlockType.Empty));
        } else if (y < height2) {
            return GetBlockTypeIndex(PercentChangeBlocks(voxelState, 66, BlockType.Rock, PercentChangeBlocks(voxelState, 33, BlockType.Stone, BlockType.Empty)));
        } else if (y < height3) {
            return GetBlockTypeIndex(PercentChangeBlocks(voxelState, 66, BlockType.Dirt, PercentChangeBlocks(voxelState, 33, BlockType.Stone, BlockType.Empty)));
        } else {
            return GetBlockTypeIndex(PercentChangeBlocks(voxelState, 50, BlockType.Dirt, PercentChangeBlocks(voxelState, 33, BlockType.Stone, BlockType.Empty)));
        }
    }

    public int Perlin1D(float x, int y) {
        var noiseHeight = Noise1D(x);

        return y > noiseHeight ? 0 : 1;
    }

    public float Noise1D(float x) {
        var scaledXHeight = x / 1f / voxelResolution;
        var freq = frequency;
        var amp = amplitude;
        var noiseRange = range;
        var sum = Mathf.PerlinNoise((scaledXHeight + seed) * freq, 0);

        for (var o = 1; o < octaves; o++) {
            freq *= lacunarity;
            amp *= persistence;
            noiseRange += amp;
            sum += Mathf.PerlinNoise((scaledXHeight + seed) * freq, 0) * amp;
        }
        var noiseHeight = sum / noiseRange;
        noiseHeight *= chunkResolution * voxelResolution;
        return noiseHeight;
    }

    public float Perlin2D(float x, int y) {
        return Mathf.InverseLerp(minNoiseVal, maxNoiseVal, Noise2D(x, y));
    }

    private float Noise2D(float x, int y) {
        var scaledX = x / 1f / voxelResolution;
        var scaledY = y / 1f / voxelResolution;
        var noiseVal = 0f;
        var freq = caveFrequency;
        var amp = caveAmplitude;

        for (var o = 0; o < caveOctaves; o++) {
            var sampleX = (scaledX) / scale * freq + octaveOffsets[o].x;
            var sampleY = (scaledY) / scale * freq + octaveOffsets[o].y;

            var perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
            noiseVal += perlinValue * amp;

            freq *= caveLacunarity;
            amp *= cavePersistence;
        }
        if (noiseVal > maxNoiseVal) {
            maxNoiseVal = noiseVal;
        } else if (noiseVal < minNoiseVal) {
            minNoiseVal = noiseVal;
        }

        return noiseVal;
    }

    public int PerlinGrass(float x, int y) {
        var noiseHeight = Noise1D(x);
        var noiseVal = Mathf.InverseLerp(minNoiseVal, maxNoiseVal, Noise2D(x, y));

        if (InRange(y, Mathf.RoundToInt(noiseHeight), 3)) {
            return GetBlockTypeIndex(BlockType.Grass);
        }
        if (InRange(y, Mathf.RoundToInt(noiseHeight), 8)) {
            return GetBlockTypeIndex(PercentChangeBlocks(noiseVal, 80, BlockType.Stone, PercentChangeBlocks(noiseVal, 25, BlockType.Dirt, BlockType.Empty)));
        }
        return -1;
    }

    private static BlockType PercentChangeBlocks(float noiseVal, int percent, BlockType block1, BlockType block2) {
        return noiseVal > (percent / 100f) ? block1 : block2;
    }

    private static int GetBlockTypeIndex(BlockType type) {
        return BlockManager.BlockIndexDictionary[type];
    }

    public static bool InRange(float input, float value, float range) {
        return input + range > value && input - range < value;
    }
}