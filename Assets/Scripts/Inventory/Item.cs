using System;
using UnityEngine;

[Serializable]
public class Item {
    public enum ItemType {
        dirt,
        rock,
        test
    };

    public ItemType itemType;
    public int amount;

    public Sprite GetSprite() {
        switch (itemType) {
            default:
            case ItemType.dirt: return ItemAssets.Instance.dirtSprite;
            case ItemType.rock: return ItemAssets.Instance.rockSprite;
            case ItemType.test: return ItemAssets.Instance.testSprite;
        }
    }

    public bool IsStackable() {
        switch (itemType) {
            default:
            case ItemType.dirt: return true;
            case ItemType.rock: return true;
            case ItemType.test: return false;
        }
    }
}
