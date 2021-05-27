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
    private UI_PlayerInventory playerInventory;
    private UI_HotBar hotbarInventory;

    private List<Recipe> currentRecipes = new List<Recipe>();
    private List<CraftingSlot> slotList = new List<CraftingSlot>();
    [HideInInspector] public int currentRecipeIndex = -1;
    [HideInInspector] public bool shouldUpdateRecipe = false;

    private void Start() {
        uIController = FindObjectOfType<UIController>();
        recipeAssets = FindObjectOfType<RecipeAssets>();
        pauseMenu = FindObjectOfType<PauseMenu>();
        currentCategory = CategoryType.Test1;
        playerInventory = FindObjectOfType<UI_PlayerInventory>();
        hotbarInventory = FindObjectOfType<UI_HotBar>();

        uIController.togglableUIs.Add(gameObject);

        UpdateUI();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.C)) {
            if (!pauseMenu.activeState) {
                Toggle();
            }
        }

        if (shouldUpdateRecipe) {
            shouldUpdateRecipe = false;
            if (currentRecipeIndex != -1) {
                var recipe = currentRecipes[currentRecipeIndex];

                //set current recipe
                var img = displayImage.GetComponent<Image>();
                var displayItem = recipe.outputList[0];
                img.sprite = displayItem.GetSprite();
                img.color = Color.white;

                //clear old requirements
                foreach (Transform child in displayRequirementsGrid) {
                    Destroy(child.gameObject);
                }

                //display requirements
                foreach (var requirement in recipe.inputList) {
                    var displayRequirement = Instantiate(displayRequirementPrefab, Vector3.zero, Quaternion.identity, displayRequirementsGrid);
                    displayRequirement.transform.GetChild(0).GetComponent<Image>().sprite = requirement.GetSprite();
                    displayRequirement.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = requirement.itemType + " x" + requirement.amount;
                }
            } else {
                var img = displayImage.GetComponent<Image>();
                img.sprite = null;
                img.color = new Color32(46, 46, 46, 255);
            }
        }
    }

    public void Toggle() {
        isActive = !isActive;

        inventoryController.SetActive(isActive);
        UpdateUI();
    }

    private void UpdateUI() {
        if (isActive) {
            shouldUpdateRecipe = true;
            currentRecipeIndex = -1;

            foreach (Transform child in displayRequirementsGrid) {
                Destroy(child.gameObject);
            }

            currentRecipes.Clear();
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

    public void Craft() {
        var recipe = currentRecipes[currentRecipeIndex];
        var removeInventory = new List<Inventory>();
        var inventories = new List<Inventory>();
        inventories.Add(playerInventory.inventory);
        inventories.Add(hotbarInventory.inventory);

        //check for requirements in inventory
        bool hasAllRequirements = true;
        foreach (var requirement in recipe.inputList) {
            bool hasRequirement = false;
            foreach (var inventory in inventories) {
                if (inventory.HasItem(requirement)) {
                    hasRequirement = true;
                    removeInventory.Add(inventory);
                    break;
                }
            }
            if (!hasRequirement) {
                hasAllRequirements = false;
                break;
            }
        }

        //remove requirements
        if (hasAllRequirements) {
            for (int i = 0; i < recipe.inputList.Count; i++) {
                var requirement = recipe.inputList[i];
                var inv = removeInventory[i];
                inv.RemoveFirstFoundItem(requirement);
            }

            //add new item
            foreach (var craftedItem in recipe.outputList) {
                foreach (var inventory in inventories) {
                    if (inventory.HasRoom(craftedItem)) {
                        inventory.AddItem(craftedItem);
                    }
                }
            }
        }
    }
}
