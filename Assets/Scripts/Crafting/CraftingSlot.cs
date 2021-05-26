using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingSlot : MonoBehaviour {
    public RectTransform rectTransform;
    public Color selectedColor;

    public int recipeIndex;

    private UI_Crafting uI_Crafting;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
        uI_Crafting = FindObjectOfType<UI_Crafting>();
    }

    private void Update() {
        var localMousePosition = rectTransform.InverseTransformPoint(Input.mousePosition);

        if (rectTransform.rect.Contains(localMousePosition) && Input.GetMouseButtonDown(0)) {
            uI_Crafting.currentRecipeIndex = recipeIndex;
            uI_Crafting.shouldUpdateRecipe = true;
        }
    }

    public void SetSelectedColor() {
        var background = transform.GetChild(0).GetComponent<Image>();
        background.color = selectedColor;
    }
}
