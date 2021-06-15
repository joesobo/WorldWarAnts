using System;
using System.Collections.Generic;
using UnityEngine;

public static class BlockManager {
    private static readonly string Path = Application.persistentDataPath + "/blocks.json";
    public static readonly Dictionary<BlockType, int> IndexDictionary = new Dictionary<BlockType, int>();
    public static readonly Dictionary<BlockType, Color> ColorDictionary = new Dictionary<BlockType, Color>();

    public static void Write(BlockCollection collection, Block element) {
        if (element != null) {
            collection.blocks.Add(element);
        }

        JsonManager.Write(Path, collection);
    }

    public static void Remove(BlockCollection collection, int index) {
        JsonManager.Remove(Path, collection.blocks, index);
    }

    public static BlockCollection Read() {
        var collection = JsonManager.Read<BlockCollection>(Path);

        IndexDictionary.Clear();
        ColorDictionary.Clear();
        for (var i = 0; i < collection.blocks.Count; i++) {
            var element = collection.blocks[i];
            if (!IndexDictionary.ContainsKey(element.blockType)) {
                IndexDictionary.Add(element.blockType, i);
                ColorDictionary.Add(element.blockType, element.color);
            }
        }

        return collection;
    }
}
