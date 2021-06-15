using System;
using System.Collections.Generic;

public enum CategoryType {
    Test1,
    Test2,
};

public enum RecipeType {
    Test1,
    Test2,
    Test3
};

[Serializable]
public class Recipe {
    //TODO: change to list of item id's
    public CategoryType category;
    public RecipeType recipeType;
    public List<ItemType> inputList = new List<ItemType>();
    public List<int> inputAmountList = new List<int>();
    public List<ItemType> outputList = new List<ItemType>();
    public List<int> outputAmountList = new List<int>();
    //public string machine??
    public int craftingTime;
    public bool unlocked;
}

[Serializable]
public class RecipeCollection {
    public List<Recipe> recipes;

    public RecipeCollection(List<Recipe> recipes) {
        this.recipes = recipes;
    }
}