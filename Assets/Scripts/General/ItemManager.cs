using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemManager {
    private static readonly string Path = Application.persistentDataPath + "/items.json";
    public static readonly Dictionary<ItemType, int> IndexDictionary = new Dictionary<ItemType, int>();

    public static void Write(ItemCollection collection, Item element) {
        if (element != null) {
            collection.items.Add(element);
        }

        JsonManager.Write(Path, collection);
    }

    public static void Remove(List<Item> collection, int index) {
        JsonManager.Remove(Path, collection, index);
    }

    public static ItemCollection Read() {
        var collection = JsonManager.Read<ItemCollection>(Path);

        IndexDictionary.Clear();
        for (var i = 0; i < collection.items.Count; i++) {
            var element = collection.items[i];
            IndexDictionary.Add(element.itemType, i);
        }

        return collection;
    }
}
