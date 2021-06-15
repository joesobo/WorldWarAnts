using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class BlockManager {
    private static string path = Application.persistentDataPath + "/blocks.json";
    public static Dictionary<BlockType, int> indexDictionary = new Dictionary<BlockType, int>();
    public static Dictionary<BlockType, Color> colorDictionary = new Dictionary<BlockType, Color>();

    public static void Write(BlockCollection collection, Block element) {
        if (element != null) {
            collection.blocks.Add(element);
        }

        JsonManager.Write<BlockCollection>(path, collection);
    }

    public static void Remove(BlockCollection collection, int index) {
        JsonManager.Remove(path, collection.blocks, index);
    }

    public static BlockCollection Read() {
        BlockCollection collection = JsonManager.Read<BlockCollection>(path);

        indexDictionary.Clear();
        colorDictionary.Clear();
        for (var i = 0; i < collection.blocks.Count; i++) {
            var element = collection.blocks[i];
            if (!indexDictionary.ContainsKey(element.blockType)) {
                indexDictionary.Add(element.blockType, i);
                colorDictionary.Add(element.blockType, element.color);
            }
        }

        return collection;
    }
}
