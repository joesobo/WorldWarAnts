using System;
using System.Collections.Generic;
using UnityEngine;

public enum BlockType {
    Empty,
    Stone,
    Dirt,
    Rock,
    Grass,
    White,
    GrassBlade,
};

[Serializable]
public class Block {
    public BlockType blockType;
    public string texturePath;
    public Color color;
    public ItemType itemType;
    public int amount;

    public Block(BlockType blockType, Color color, string texturePath, ItemType itemType, int amount) {
        this.blockType = blockType;
        this.color = color;
        this.texturePath = texturePath;
        this.itemType = itemType;
        this.amount = amount;
    }
}

[Serializable]
public class BlockCollection {
    public List<Block> blocks;

    public BlockCollection(List<Block> blocks) {
        this.blocks = blocks;
    }
}