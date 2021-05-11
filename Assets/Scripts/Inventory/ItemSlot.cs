using System;
using UnityEngine;

public class ItemSlot : MonoBehaviour {
    public Action LeftClick = null;
    public Action RightClick = null;

    void Update() {
        if (Input.GetMouseButtonDown(0)) LeftClick();
        if (Input.GetMouseButtonDown(1)) RightClick();
        if (Input.GetMouseButtonDown(2));
    }
}
