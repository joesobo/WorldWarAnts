using System;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType {
    Empty,
    Dirt,
    Stone,
    Test,
    GrassBlade,
};

[Serializable]
public class Item {
    public string name;
    public ItemType itemType;
    public int amount;
    public static int MaxAmount = 64;

    public Sprite GetSprite() {
        switch (itemType) {
            default:
            case ItemType.Empty: return null;
            case ItemType.Dirt: return ItemAssets.Instance.dirtSprite;
            case ItemType.Stone: return ItemAssets.Instance.stoneSprite;
            case ItemType.Test: return ItemAssets.Instance.testSprite;
            case ItemType.GrassBlade: return ItemAssets.Instance.grassBladeSprite;
        }
    }

    public bool IsStackable() {
        switch (itemType) {
            default:
            case ItemType.Empty: return false;
            case ItemType.Dirt: return true;
            case ItemType.Stone: return true;
            case ItemType.Test: return false;
            case ItemType.GrassBlade: return true;
        }
    }
}

[Serializable]
public class ItemCollection {
    public List<Item> items = new List<Item>();

    public ItemCollection(List<Item> items) {
        this.items = items;
    }
}