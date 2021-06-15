using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecipeManager : MonoBehaviour {
    private static string path = Application.persistentDataPath + "/recipes.json";
    public static Dictionary<RecipeType, int> indexDictionary = new Dictionary<RecipeType, int>();

    public static void Write(RecipeCollection collection, Recipe element) {
        if (element != null) {
            collection.recipes.Add(element);
        }

        JsonManager.Write<RecipeCollection>(path, collection);
    }

    public static void Remove(List<Recipe> collection, int index) {
        JsonManager.Remove(path, collection, index);
    }

    public static RecipeCollection Read() {
        RecipeCollection collection = JsonManager.Read<RecipeCollection>(path);

        indexDictionary.Clear();
        for (var i = 0; i < collection.recipes.Count; i++) {
            var element = collection.recipes[i];
            indexDictionary.Add(element.recipeType, i);
        }

        return collection;
    }
}
