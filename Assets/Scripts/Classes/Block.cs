using System;
using System.Collections.Generic;
using UnityEngine;

public enum BlockType {
    Empty,
    Stone,
    Dirt,
    Rock,
    Grass,
    White
};

[Serializable]
public class Block {
    public string texturePath;
    public BlockType blockType;
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
    public List<Block> blocks = new List<Block>();
}