using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Crafting : MonoBehaviour {
    public bool isActive = false;
    public GameObject inventoryController;
    public GameObject slotPrefab;
    public Transform slotContainer;

    public GameObject displayImage;
    public Transform displayRequirementsGrid;
    public GameObject displayRequirementPrefab;

    private UIController uIController;
    private PauseMenu pauseMenu;
    private CategoryType currentCategory;
    private UI_PlayerInventory playerInventory;
    private UI_HotBar hotbarInventory;

    private RecipeCollection recipeCollection;
    private ItemCollection itemCollection;
    private readonly List<Recipe> currentRecipes = new List<Recipe>();
    private readonly List<CraftingSlot> slotList = new List<CraftingSlot>();
    [HideInInspector] public int currentRecipeIndex = -1;
    [HideInInspector] public bool shouldUpdateRecipe = false;

    private void Start() {
        uIController = FindObjectOfType<UIController>();
        pauseMenu = FindObjectOfType<PauseMenu>();
        currentCategory = CategoryType.Test1;
        playerInventory = FindObjectOfType<UI_PlayerInventory>();
        hotbarInventory = FindObjectOfType<UI_HotBar>();

        recipeCollection = RecipeManager.Read();
        itemCollection = ItemManager.Read();

        isActive = false;

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
                var displayItem = itemCollection.items.Find(item => item.itemType == recipe.outputList[0]);
                img.sprite = displayItem.GetSprite();
                img.color = Color.white;

                //clear old requirements
                foreach (Transform child in displayRequirementsGrid) {
                    Destroy(child.gameObject);
                }

                //display requirements
                for (var i = 0; i < recipe.inputList.Count; i++) {
                    var requirement = recipe.inputList[i];
                    var requiredAmount = recipe.inputAmountList[i];
                    var requiredItem = itemCollection.items.Find(item => item.itemType == requirement);
                    var displayRequirement = Instantiate(displayRequirementPrefab, Vector3.zero, Quaternion.identity, displayRequirementsGrid);
                    displayRequirement.transform.GetChild(0).GetComponent<Image>().sprite = requiredItem.GetSprite();
                    displayRequirement.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = requiredItem.itemType + " x" + requiredAmount;
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
            foreach (var recipe in recipeCollection.recipes.Where(recipe => recipe.category == currentCategory)) {
                currentRecipes.Add(recipe);
            }

            slotList.Clear();
            foreach (Transform child in slotContainer) {
                Destroy(child.gameObject);
            }

            var index = 0;
            foreach (var recipe in currentRecipes) {
                var displayItemType = recipe.outputList[0];
                var displayItemAmount = recipe.outputAmountList[0];
                var displayItem = itemCollection.items.Find(item => item.itemType == displayItemType);

                var craftingSlot = Instantiate(slotPrefab, Vector3.zero, Quaternion.identity, slotContainer);
                var slot = craftingSlot.GetComponent<CraftingSlot>();
                slot.recipeIndex = index;
                slotList.Add(slot);

                var itemSlotTransform = craftingSlot.GetComponent<RectTransform>();
                itemSlotTransform.gameObject.SetActive(true);
                var slotImage = itemSlotTransform.Find("Image").GetComponent<Image>();
                var text = itemSlotTransform.Find("Amount").GetComponent<TextMeshProUGUI>();

                if (displayItem != null) {
                    slotImage.sprite = displayItem.GetSprite();
                    slotImage.color = Color.white;
                    text.SetText(displayItemAmount > 1 ? displayItemAmount.ToString() : "");
                }

                index++;
            }
        }
    }

    public void Craft() {
        if (currentRecipes.Count > currentRecipeIndex) {
            var recipe = currentRecipes[currentRecipeIndex];
            var removeInventory = new List<Inventory>();
            var inventories = new List<Inventory> { playerInventory.inventory, hotbarInventory.inventory };

            //check for requirements in inventory
            var hasAllRequirements = true;
            foreach (var requirement in recipe.inputList) {
                var hasRequirement = false;
                var requiredItem = itemCollection.items.Find(item => item.itemType == requirement);
                foreach (var inventory in inventories.Where(inventory => inventory.HasItem(requiredItem))) {
                    hasRequirement = true;
                    removeInventory.Add(inventory);
                    break;
                }
                if (!hasRequirement) {
                    hasAllRequirements = false;
                    break;
                }
            }

            //remove requirements
            if (hasAllRequirements) {
                for (var i = 0; i < recipe.inputList.Count; i++) {
                    var requirement = recipe.inputList[i];
                    var requiredItem = itemCollection.items.Find(item => item.itemType == requirement);
                    var inv = removeInventory[i];
                    inv.RemoveFirstFoundItem(requiredItem);
                }

                //add new item
                foreach (var craftedType in recipe.outputList) {
                    var craftedItem = itemCollection.items.Find(item => item.itemType == craftedType);
                    foreach (var inventory in inventories.Where(inventory => inventory.HasRoom(craftedItem))) {
                        inventory.AddItem(craftedItem);
                    }
                }
            }
        }
    }
}
