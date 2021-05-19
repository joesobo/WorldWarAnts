using System;
using UnityEngine;

public enum ItemType {
    Empty,
    Dirt,
    Stone,
    Test
};

[Serializable]
public class Item {
    public ItemType itemType;
    public int amount;
    public static int MAXAmount = 64;

    public Sprite GetSprite() {
        switch (itemType) {
            default:
            case ItemType.Empty: return null;
            case ItemType.Dirt: return ItemAssets.Instance.dirtSprite;
            case ItemType.Stone: return ItemAssets.Instance.stoneSprite;
            case ItemType.Test: return ItemAssets.Instance.testSprite;
        }
    }

    public bool IsStackable() {
        switch (itemType) {
            default:
            case ItemType.Empty: return false;
            case ItemType.Dirt: return true;
            case ItemType.Stone: return true;
            case ItemType.Test: return false;
        }
    }
}
