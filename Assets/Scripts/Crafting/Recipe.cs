using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Recipe : MonoBehaviour {
    public List<Item> inputList = new List<Item>();
    public List<Item> outputList = new List<Item>();
    public CategoryType category;
    //public string machine??
    public int craftingTime;
    public bool unlocked;
}
