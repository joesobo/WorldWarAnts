using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CategoryType {
    Test1,
    Test2,
};

[Serializable]
public class Recipe {
    //TODO: change to list of item id's
    public List<Item> inputList = new List<Item>();
    public List<Item> outputList = new List<Item>();
    public CategoryType category;
    //public string machine??
    public int craftingTime;
    public bool unlocked;
}
