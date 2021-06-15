using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager {
    private static string path = Application.persistentDataPath + "/items.json";
    public static Dictionary<ItemType, int> indexDictionary = new Dictionary<ItemType, int>();

    public static void Write(ItemCollection collection, Item element) {
        if (element != null) {
            collection.items.Add(element);
        }

        JsonManager.Write<ItemCollection>(path, collection);
    }

    public static void Remove(List<Item> collection, int index) {
        JsonManager.Remove(path, collection, index);
    }

    public static ItemCollection Read() {
        ItemCollection collection = JsonManager.Read<ItemCollection>(path);

        indexDictionary.Clear();
        for (var i = 0; i < collection.items.Count; i++) {
            var element = collection.items[i];
            indexDictionary.Add(element.itemType, i);
        }

        return collection;
    }
}
