using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum CategoryType {
    Test1,
    Test2,
};

public class UI_Crafting : MonoBehaviour {
    public bool isActive;
    public GameObject inventoryController;
    public GameObject slotPrefab;
    public Transform slotContainer;

    public GameObject displayImage;
    public Transform displayRequirementsGrid;
    public GameObject displayRequirementPrefab;

    private UIController uIController;
    private RecipeAssets recipeAssets;
    private PauseMenu pauseMenu;
    private CategoryType currentCategory;

    private List<Recipe> currentRecipes = new List<Recipe>();
    private List<CraftingSlot> slotList = new List<CraftingSlot>();
    [HideInInspector] public int currentRecipeIndex = -1;
    [HideInInspector] public bool shouldUpdateRecipe = false;

    private void Start() {
        uIController = FindObjectOfType<UIController>();
        recipeAssets = FindObjectOfType<RecipeAssets>();
        pauseMenu = FindObjectOfType<PauseMenu>();
        currentCategory = CategoryType.Test1;

        uIController.togglableUIs.Add(gameObject);

        if (isActive) {
            foreach (Recipe recipe in recipeAssets.recipes) {
                if (recipe.category == currentCategory) {
                    currentRecipes.Add(recipe);
                }
            }

            slotList.Clear();
            foreach (Transform child in slotContainer) {
                Destroy(child.gameObject);
            }

            int index = 0;
            foreach (Recipe recipe in currentRecipes) {
                var displayItem = recipe.outputList[0];

                var craftingSlot = Instantiate(slotPrefab, Vector3.zero, Quaternion.identity, slotContainer);
                CraftingSlot slot = craftingSlot.GetComponent<CraftingSlot>();
                slot.recipeIndex = index;
                slotList.Add(slot);

                var itemSlotTransform = craftingSlot.GetComponent<RectTransform>();
                itemSlotTransform.gameObject.SetActive(true);
                var slotImage = itemSlotTransform.Find("Image").GetComponent<Image>();
                var text = itemSlotTransform.Find("Amount").GetComponent<TextMeshProUGUI>();

                if (displayItem != null) {
                    slotImage.sprite = displayItem.GetSprite();
                    slotImage.color = Color.white;
                    text.SetText(displayItem.amount > 1 ? displayItem.amount.ToString() : "");
                }

                index++;
            }
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.C)) {
            if (!pauseMenu.activeState) {
                Toggle();
            }
        }

        if (currentRecipeIndex != -1 && shouldUpdateRecipe) {
            shouldUpdateRecipe = false;
            var img = displayImage.GetComponent<Image>();
            var displayItem = currentRecipes[currentRecipeIndex].outputList[0];
            img.sprite = displayItem.GetSprite();
            img.color = Color.white;
        }
    }

    public void Toggle() {
        isActive = !isActive;

        inventoryController.SetActive(isActive);
    }
}
