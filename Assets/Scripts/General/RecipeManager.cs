using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecipeManager : MonoBehaviour {
    private static readonly string Path = Application.persistentDataPath + "/recipes.json";
    public static readonly Dictionary<RecipeType, int> IndexDictionary = new Dictionary<RecipeType, int>();

    public static void Write(RecipeCollection collection, Recipe element) {
        if (element != null) {
            collection.recipes.Add(element);
        }

        JsonManager.Write(Path, collection);
    }

    public static void Remove(List<Recipe> collection, int index) {
        JsonManager.Remove(Path, collection, index);
    }

    public static RecipeCollection Read() {
        var collection = JsonManager.Read<RecipeCollection>(Path);

        IndexDictionary.Clear();
        for (var i = 0; i < collection.recipes.Count; i++) {
            var element = collection.recipes[i];
            IndexDictionary.Add(element.recipeType, i);
        }

        return collection;
    }
}
