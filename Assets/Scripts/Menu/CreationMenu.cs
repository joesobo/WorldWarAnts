using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreationMenu : MonoBehaviour {
    public InputField worldNameField;
    public InputField seedField;
    public Text errorText;
    public bool isCreative = true;

    private string worldName;
    private int seed;
    private WorldDataHandler worldDataHandler;

    private void OnEnable() {
        worldDataHandler = FindObjectOfType<WorldDataHandler>();
        errorText.text = "";
    }

    public void UpdateWorldTitle() {
        worldName = worldNameField.text;
        errorText.text = "";
    }

    public void UpdateSeed() {
        seed = int.Parse(seedField.text);
    }

    public void ToggleMode() {
        isCreative = !isCreative;
    }

    public void Play() {
        if (worldNameField.text != null || worldNameField.text != "") {
            if (worldDataHandler.ContainsWorld(worldName)) {
                errorText.text = "Error: World Name is already taken";
            } else {
                worldDataHandler.NewWorld(new WorldData(worldName, seed, DateTime.Now.ToString(), isCreative));
                SceneManager.LoadScene(1);
            }
        }
    }
}
