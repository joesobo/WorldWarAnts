using System;
using UnityEngine;

public class ItemSlot : MonoBehaviour {
    public Action Hover = null;
    public Action Drop = null;
    public Action Pickup = null;
    public Action Split = null;

    public int index = 0;

    private RectTransform rectTransform;

    void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update() {
        if (IsHovering() && Hover != null) Hover();
        if (Input.GetKeyDown(KeyCode.Q) && Drop != null) Drop();
        if (Input.GetMouseButtonDown(0) && Pickup != null) Pickup();
        if (Input.GetMouseButtonDown(2) && Split != null) Split();
    }

    private bool IsHovering() {
        Vector2 localMousePosition = rectTransform.InverseTransformPoint(Input.mousePosition);
        if (rectTransform.rect.Contains(localMousePosition)) {
            return true;
        }

        return false;
    }
}
