using System;
using UnityEngine;

public class ItemSlot : MonoBehaviour {
    public Action Drop = null;
    public Action Pickup = null;
    public Action Split = null;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Q)) Drop();
        if (Input.GetMouseButtonDown(1)) Pickup();
        if (Input.GetMouseButtonDown(2)) Split();
    }
}
