using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecipeAssets : MonoBehaviour {
    public List<Recipe> recipes;

    private void Awake() {
        //Test 1
        Recipe testRecipe = new Recipe();
        testRecipe.inputList.Add(new Item { itemType = ItemType.Dirt, amount = 1 });
        testRecipe.outputList.Add(new Item { itemType = ItemType.Stone, amount = 1 });
        testRecipe.category = CategoryType.Test1;
        testRecipe.craftingTime = 1;
        testRecipe.unlocked = true;

        recipes.Add(testRecipe);

        //Test 2
        Recipe testRecipe2 = new Recipe();
        testRecipe2.inputList.Add(new Item { itemType = ItemType.Stone, amount = 1 });
        testRecipe2.outputList.Add(new Item { itemType = ItemType.Test, amount = 1 });
        testRecipe2.category = CategoryType.Test1;
        testRecipe2.craftingTime = 1;
        testRecipe2.unlocked = true;

        recipes.Add(testRecipe2);

        //Test 3
        Recipe testRecipe3 = new Recipe();
        testRecipe3.inputList.Add(new Item { itemType = ItemType.Dirt, amount = 1 });
        testRecipe3.outputList.Add(new Item { itemType = ItemType.Stone, amount = 1 });
        testRecipe3.category = CategoryType.Test2;
        testRecipe3.craftingTime = 1;
        testRecipe3.unlocked = true;

        recipes.Add(testRecipe3);
    }
}
