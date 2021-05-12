using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldItemSpawner : MonoBehaviour {
    public Item item;

    private void Start() {
        WorldItem.SpawnWorldItem(transform.position, item);
        Destroy(gameObject);
    }
}
