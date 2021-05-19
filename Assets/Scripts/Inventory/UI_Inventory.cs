using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class UI_Inventory : MonoBehaviour {
    public Inventory inventory;
    [HideInInspector] public List<ItemSlot> slotList;

    public Transform itemSlotPrefab;
    public Transform slotContainer;
    [HideInInspector] public ItemSlot hoverSlot;
    [HideInInspector] public bool isHovering = false;

    public void SetupInventory(Inventory inventory) {
        this.inventory = inventory;
        slotList = new List<ItemSlot>();

        RefreshInventory();

        inventory.OnItemListChanged += Inventory_OnListItemChanged;
    }

    private void Inventory_OnListItemChanged(object sender, EventArgs e) {
        RefreshInventory();
    }

    protected void RefreshInventory() {
        var itemList = inventory.itemList;

        slotList.Clear();
        foreach (Transform child in slotContainer) {
            Destroy(child.gameObject);
        }

        for (var i = 0; i < inventory.size; i++) {
            var item = itemList[i];
            var itemSlot = Instantiate(itemSlotPrefab, Vector3.zero, Quaternion.identity, slotContainer);
            slotList.Add(itemSlot.GetComponent<ItemSlot>());
            itemSlot.GetComponent<ItemSlot>().index = i;

            var itemSlotTransform = itemSlot.GetComponent<RectTransform>();
            itemSlotTransform.gameObject.SetActive(true);
            var slot = itemSlotTransform.GetComponent<ItemSlot>();
            var slotImage = itemSlotTransform.Find("Image").GetComponent<Image>();
            var text = itemSlotTransform.Find("Amount").GetComponent<TextMeshProUGUI>();

            slot.StartUp(this, item);

            if (item != null) {
                slotImage.sprite = item.GetSprite();
                slotImage.color = Color.white;
                text.SetText(item.amount > 1 ? item.amount.ToString() : "");
            }
        }
    }

    public abstract void InventorySpecificControls();
}